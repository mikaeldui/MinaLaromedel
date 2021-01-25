using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;

namespace MinaLaromedel.Services
{
    public static class SettingsService
    {
        static SettingsService()
        {
            // Migrate
            var settings = ApplicationData.Current.LocalSettings.Values;
            var roamingSettings = ApplicationData.Current.RoamingSettings.Values;
            if (settings.ContainsKey("username"))
            {
                var vault = new PasswordVault();
                vault.Add(new PasswordCredential("Hermods Novo", (string)settings["username"], (string)settings["password"]));

                settings.Remove("username");
                settings.Remove("password");
                try
                {
                    roamingSettings.Remove("username");
                    roamingSettings.Remove("password");
                } catch { }
            }
        }

        public static PasswordCredential GetPasswordCredential(string distributor)
        {
            PasswordCredential credential = null;

            var vault = new Windows.Security.Credentials.PasswordVault();
            var credentialList = vault.FindAllByResource(distributor);
            if (credentialList.Count > 0)
            {
                if (credentialList.Count == 1)
                {
                    credential = credentialList[0];
                }
                else
                {
                    //Will not happen yet
                    //credential = vault.Retrieve(resourceName, defaultUserName);
                }
            }

            return credential;
        }

        public static bool IsCredentialsSaved() => (new PasswordVault()).RetrieveAll().Any();
    }
}
