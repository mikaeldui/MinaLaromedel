using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace MinaLaromedel.Services
{
    public static class SettingsService
    {
        private static IPropertySet _settings;

        static SettingsService() => _settings = ApplicationData.Current.LocalSettings.Values;

        public static string Username
        {
            get => _get<string>();
            set => _set(value);
        }

        public static string Password
        {
            get => _get<string>();
            set => _set(value);
        }

        public static bool IsCredentialsSaved() => _settings.ContainsKey("username");

        private static void _set(object value, [CallerMemberName] string name = null) => _settings[name.ToLower()] = value;
        private static TType _get<TType>([CallerMemberName] string name = null) => (TType)_settings[name.ToLower()];
    }
}
