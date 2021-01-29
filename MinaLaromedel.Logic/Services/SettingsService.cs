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
        private static IPropertySet LocalSettings => ApplicationData.Current.LocalSettings.Values;
        private static IPropertySet RoamingSettings => ApplicationData.Current.RoamingSettings.Values;

        public static bool IsAutomaticErrorReportingEnabled
        {
            get => LocalSettings.ContainsKey(nameof(IsAutomaticErrorReportingEnabled)) ? (bool)LocalSettings[nameof(IsAutomaticErrorReportingEnabled)] : false;
            set => LocalSettings[nameof(IsAutomaticErrorReportingEnabled)] = ErrorService.IsSentryActive = value;
        }
    }
}
