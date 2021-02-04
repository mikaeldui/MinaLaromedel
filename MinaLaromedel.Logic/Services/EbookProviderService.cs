using MinaLaromedel.EbookProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MinaLaromedel.EbookProviders
{
    public static class EbookProviderService
    {
        private static readonly Dictionary<(string, string), IEbookProvider> _activeProviders = new Dictionary<(string, string), IEbookProvider>();
        private static readonly Dictionary<string, Type> _providerTypes = new Dictionary<string, Type>();
        public static IEbookProvider GetProvider(string providerName, string username)
        {
            lock (_activeProviders)
            {
                if (_activeProviders.ContainsKey((providerName, username)))
                    return _activeProviders[(providerName, username)];

                Type providerType;

                if (_providerTypes.ContainsKey(providerName))
                    providerType = _providerTypes[providerName];
                else
                {
                    providerType = _getProviderTypeFromName(providerName);

                    if (providerType == null)
                        throw new ArgumentException("The provider doesn't exist");

                    _providerTypes.Add(providerName, providerType);
                }

                var providerInstance = (IEbookProvider)Activator.CreateInstance(providerType);

                _activeProviders.Add((providerName, username), providerInstance);

                return providerInstance;
            }
        }

        public static IEbookProvider GetProvider(PasswordCredential credential)
        {
            if (string.IsNullOrEmpty(credential.Resource))
                throw new ArgumentException("The PasswordCredential resource can't be empty");

            return GetProvider(credential.Resource, credential.UserName);
        }

        internal static bool RemoveProvider(IEbookProvider provider)
        {
            lock (_activeProviders)
            {
                var item = _activeProviders.FirstOrDefault(i => i.Value == provider);

                return _activeProviders.Remove(item.Key);
            }
        }

        public static bool RemoveProviderFor(string providerName, string username)
        {
            lock(_activeProviders)
                return _activeProviders.Remove((providerName, username));
        }

        private static Type _getProviderTypeFromName(string providerName)
        {
            var type = typeof(IEbookProvider);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            var typeWithAttribute = types.FirstOrDefault(o => o.GetCustomAttribute<EbookProviderNameAttribute>().Name == providerName);

            if (typeWithAttribute != null)
                return typeWithAttribute;

            return types.FirstOrDefault(t => t.Name.StartsWith(providerName));
        }
    }
}
