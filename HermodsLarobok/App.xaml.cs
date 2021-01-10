using GalaSoft.MvvmLight.Messaging;
using HermodsLarobok.Clients;
using HermodsLarobok.Messages;
using HermodsLarobok.Storage;
using HermodsLarobok.Views;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HermodsLarobok
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static HermodsNovoClient HermodsNovoClient = new HermodsNovoClient();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    var settings = ApplicationData.Current.LocalSettings;

                    if (!settings.Values.ContainsKey("username"))
                        rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                    else
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);

                        AsyncContext.Run(async () => await HermodsNovoClient.AuthenticateWithAsync(settings.Values["username"] as string, settings.Values["password"] as string));
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            if(e.TileId != null && e.TileId.StartsWith("isbn-"))
            {
                AsyncContext.Run(async () =>
                {
                    var decoder = new WwwFormUrlDecoder(e.Arguments);
                    var ebook = await EbookStorage.GetEbookAsync(decoder.GetFirstValueByName("isbn"));
                    await ReadingPage.TryShowWindowAsync(ebook);
                });
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                // TODO: Handle URI activation
                // The received URI is eventArgs.Uri.AbsoluteUri

                if (eventArgs.Uri.Scheme == "hermodsebook")
                {
                    var decoder = new WwwFormUrlDecoder(eventArgs.Uri.Query);
                    Messenger.Default.Send(new ShowEbookPageMessage { PageNumber = int.Parse(decoder.GetFirstValueByName("page")) }, eventArgs.Uri.AbsolutePath);
                }
            }
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
