using GalaSoft.MvvmLight.Messaging;
using MinaLaromedel.Messages;
using MinaLaromedel.Services;
using MinaLaromedel.Storage;
using MinaLaromedel.ViewModels;
using MinaLaromedel.Views;
using Hermods.Novo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MinaLaromedel.Managers;
using Sentry;

namespace MinaLaromedel
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Dictionary<EbookViewModel, AppWindow> ReadingWindows { get; } = new Dictionary<EbookViewModel, AppWindow>();

        private IDisposable _sentry;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            SentryEnabled = SettingsService.IsAutomaticErrorReportingEnabled;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    if (!CredentialsService.IsCredentialsSaved())
                        rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                    else
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            if(e.TileId != null && e.TileId.StartsWith("isbn-") && CredentialsService.IsCredentialsSaved())
            {
                string isbn;
                {
                    var decoder = new WwwFormUrlDecoder(e.Arguments);
                    isbn = decoder.GetFirstValueByName("isbn");
                }

                _showReadingWindow(isbn);
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame == null)
                {
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    Window.Current.Content = rootFrame;
                }

                if (rootFrame.Content == null)
                {
                    if (!CredentialsService.IsCredentialsSaved())
                        rootFrame.Navigate(typeof(LoginPage));
                    else
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        rootFrame.Navigate(typeof(MainPage));
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();

                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                // TODO: Handle URI activation
                // The received URI is eventArgs.Uri.AbsoluteUri

                if (eventArgs.Uri.Scheme == "mina-laromedel" && CredentialsService.IsCredentialsSaved())
                {
                    var decoder = new WwwFormUrlDecoder(eventArgs.Uri.Query);
                    string isbn = eventArgs.Uri.AbsolutePath;
                    int page = int.Parse(decoder.GetFirstValueByName("page"));
                    _showReadingWindow(isbn, page);
                }
            }
        }

        private void _showReadingWindow(string isbn, int? pageIndex = null)
        {
            if (EbookManager.Ebooks.Count == 0)
            {
                EbookManager.EbooksLoaded += EbookManager_EbooksLoaded;

                void EbookManager_EbooksLoaded(object sender, EventArgs e)
                {
                    EbookManager.EbooksLoaded -= EbookManager_EbooksLoaded;

                    _ = showAsync(isbn, pageIndex);
                }
            }
            else
            {
                _ = showAsync(isbn, pageIndex);
            }

            async Task showAsync(string isbn, int? pageIndex)
            {
                await UIThread.RunAsync(async () =>
                {
                    await ReadingPage.TryShowWindowAsync(EbookManager.Ebooks.First(eb => eb.Isbn == isbn));
                    if(pageIndex != null)
                        Messenger.Default.Send(new ShowEbookPageMessage { PageNumber = pageIndex.Value }, isbn);
                });
            }
        }

        public bool SentryEnabled
        {
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
                        UnhandledException += App_UnhandledException;
                    }
                }
                else if (_sentry != null)
                {
                    _sentry.Dispose();
                    UnhandledException -= App_UnhandledException;
                }
            }
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            // If you want to avoid the application from crashing:
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
