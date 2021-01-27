using MinaLaromedel.EbookProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MinaLaromedel
{
    public static class CredentialsService
    {
        #region Authenticate

        public static async Task<bool> TryAuthenticateAsync()
        {
            var credentials = GetProviderCredentials();

            bool successfullness = true;
            foreach (var credential in credentials)
            {
                if (!await TryAuthenticateAsync(credential))
                    successfullness = false;
            }

            return successfullness;
        }

        public static async Task<bool> TryAuthenticateAsync(PasswordCredential credential)
        {
            IEbookProvider provider = EbookProviderService.GetProvider(credential);

            if (await provider.AuthenticateAsync(credential))
                return true;
            else
            {
                EbookProviderService.RemoveProvider(provider);
                return false;
            }
        }

        #endregion Authenticate

        public static PasswordCredential GetProviderCredential(string providerName) => (new PasswordVault()).FindAllByResource(providerName).FirstOrDefault();
        public static IReadOnlyList<PasswordCredential> GetProviderCredentials() => (new PasswordVault()).RetrieveAll().Where(pc => !string.IsNullOrEmpty(pc.Resource)).ToArray();
    }
}
