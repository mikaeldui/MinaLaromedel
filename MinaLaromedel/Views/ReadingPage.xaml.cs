using GalaSoft.MvvmLight.Messaging;
using MinaLaromedel.Messages;
using MinaLaromedel.Services;
using MinaLaromedel.Storage;
using MinaLaromedel.ViewModels;
using Hermods.Novo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MinaLaromedel.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReadingPage : Page
    {
        public double PageMaxHeight { get; set; }
        public double PageMaxWidth { get; set; }

        public EbookViewModel Ebook { get; set; }

        public AppWindow AppWindow => App.ReadingWindows[Ebook];

        public ApplicationDataContainer Settings { get; set; }

        public ObservableCollection<EbookOpeningViewModel> EbookOpenings { get; } = new ObservableCollection<EbookOpeningViewModel>();

        public ReadingPage()
        {
            this.InitializeComponent();

//#if DEBUG
//            // PIP testing.
//            var menuFlyout = EbookFlipView.ContextFlyout as MenuFlyout;

//            var pipItem = new MenuFlyoutItem()
//            {
//                Text = "PIP",
//                Icon = new SymbolIcon(Symbol.FullScreen)
//            };

//            pipItem.Click += (_, _2) => AppWindow.Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay);

//            menuFlyout.Items.Add(pipItem);
//#endif
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (e.Parameter)
            {
                case EbookViewModel ebook:
                    Ebook = ebook;
                    break;
                //case string isbn:
                //    Ebook = new EbookViewModel(await EbookStorage.GetEbooksAsync(isbn));
                //    break;
            }

            Messenger.Default.Register<ShowEbookPageMessage>(this, Ebook.Isbn, message => 
            {
                EbookFlipView.SelectedIndex = message.PageNumber / 2;
            });

            Settings = ApplicationData.Current.RoamingSettings.CreateContainer(Ebook.Isbn, ApplicationDataCreateDisposition.Always);

            _ = Task.Run(async () =>
            {
                var pages = await PageStorage.GetPagePathsAsync(Ebook.Isbn);

                List<EbookOpeningViewModel> viewModels = new List<EbookOpeningViewModel>();

                // Add the front page
                viewModels.Add(new EbookOpeningViewModel(null, pages[0]));

                for (int i = 1; i < pages.Length; i++)
                {
                    viewModels.Add(new EbookOpeningViewModel(pages[i], i + 1 == pages.Length ? null : pages[i + 1]));
                    i++;
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var vm in viewModels)
                        EbookOpenings.Add(vm);

                    if (Settings.Values.ContainsKey("selectedIndex"))
                        EbookFlipView.SelectedIndex = (int)Settings.Values["selectedIndex"];

                    EbookFlipView.SelectionChanged += FlipView_SelectionChanged;

                    LoadingProgressRing.IsActive = false;
                });
            });

            base.OnNavigatedTo(e);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Values["selectedIndex"] = EbookFlipView.SelectedIndex;
        }

        #region Context Menu

        private void EbookFlipView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            //flyoutBase.ShowAt(senderElement);
            flyoutBase.ShowAt(sender as UIElement, new FlyoutShowOptions() { Position = e.GetPosition(sender as UIElement) });
        }

        private void EbookFlipView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            //flyoutBase.ShowAt(senderElement);
            flyoutBase.ShowAt(sender as UIElement, new FlyoutShowOptions() { Position = e.GetPosition(sender as UIElement) });
        }

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText($"mina-laromedel:{Ebook.Isbn}?page={EbookFlipView.SelectedIndex * 2}");
            Clipboard.SetContent(dataPackage);
        }

        #region Full Screen

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            var configuration = AppWindow.Presenter.GetConfiguration();
            if (FullScreenButton.Text == "Avsluta helskärmsläge" && _tryExitFullScreen())
            {
                // Nothing
            }
            else
            {
                if (AppWindow.Presenter.RequestPresentation(AppWindowPresentationKind.FullScreen))
                {
                    FullScreenButton.Text = "Avsluta helskärmsläge";
                    FullScreenButton.Icon = new SymbolIcon(Symbol.BackToWindow);

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500);
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            AppWindow.Changed += AppWindow_Changed;
                        });
                    });
                }
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange)
            {
                _tryExitFullScreen();
            }
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                _tryExitFullScreen();
            }
        }

        private bool _tryExitFullScreen()
        {
            if (AppWindow.Presenter.RequestPresentation(AppWindowPresentationKind.Default))
            {
                FullScreenButton.Text = "Helskärmsläge";
                FullScreenButton.Icon = new SymbolIcon(Symbol.FullScreen);
                AppWindow.Changed -= AppWindow_Changed;

                return true;
            }

            return false;
        }

        #endregion Full Screen

        #endregion Context Menu

        #region TryShowWindowAsync

        public static async Task<bool> TryShowWindowAsync(HermodsNovoEbook ebook) => await TryShowWindowAsync(new EbookViewModel(ebook));

        public static async Task<bool> TryShowWindowAsync(EbookViewModel ebookViewModel)
        {
            if (App.ReadingWindows.ContainsKey(ebookViewModel))
                return await App.ReadingWindows[ebookViewModel].TryShowAsync();

            AppWindow readingWindow = await AppWindow.TryCreateAsync();
            App.ReadingWindows.Add(ebookViewModel, readingWindow);
            readingWindow.Title = ebookViewModel.Title;

            readingWindow.Closed += (sender, e) =>
            {
                App.ReadingWindows.Remove(App.ReadingWindows.First(_ => _.Value == sender).Key);
            };

            Frame readingWindowContentFrame = new Frame();
            readingWindowContentFrame.Navigate(typeof(ReadingPage), ebookViewModel);

            ElementCompositionPreview.SetAppWindowContent(readingWindow, readingWindowContentFrame);


            return await readingWindow.TryShowAsync();
        }

        #endregion TryShowWindowAsync


    }
}
