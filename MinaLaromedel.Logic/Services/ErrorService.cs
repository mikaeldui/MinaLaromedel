using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace MinaLaromedel.Services
{
    public static class ErrorService
    {
        private static IDisposable _sentry;

        public static void Init()
        {
            IsSentryActive = SettingsService.IsAutomaticErrorReportingEnabled;
        }

        public static bool IsSentryActive
        {
            get => _sentry == null;
            set
            {
                if (value)
                {
                    if (_sentry == null)
                    {
                        _sentry = SentrySdk.Init(o =>
                        {
                            o.Dsn = "https://356c701be29e4197a3b9e31f29ae5051@o511625.ingest.sentry.io/5609165";

                            Package package = Package.Current;
                            PackageId packageId = package.Id;
                            PackageVersion version = packageId.Version;
                            o.Release = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
#if RELEASE
                            o.Environment = "Production";
#endif 
#if DEBUG
                            o.Environment = "Debug";
#endif
                        });
                        Application.Current.UnhandledException += App_UnhandledException;
                    }
                }
                else if (_sentry != null)
                {
                    _sentry.Dispose();
                    _sentry = null;
                    Application.Current.UnhandledException -= App_UnhandledException;
                }

                static void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
                {
                    SentrySdk.CaptureException(e.Exception);

                    // If you want to avoid the application from crashing:
                    e.Handled = true;
                }
            }
        }
    }
}
