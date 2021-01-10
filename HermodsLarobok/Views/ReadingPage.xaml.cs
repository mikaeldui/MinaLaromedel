using GalaSoft.MvvmLight.Messaging;
using HermodsLarobok.Messages;
using HermodsLarobok.Models;
using HermodsLarobok.Services;
using HermodsLarobok.Storage;
using HermodsLarobok.ViewModels;
using Nito.AsyncEx;
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
using Windows.UI.Input;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HermodsLarobok.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReadingPage : Page
    {
        public double PageMaxHeight { get; set; }
        public double PageMaxWidth { get; set; }

        public EbookViewModel Ebook { get; set; }

        public ApplicationDataContainer Settings { get; set; }

        public ObservableCollection<EbookOpeningViewModel> EbookOpenings { get; } = new ObservableCollection<EbookOpeningViewModel>();

        public ReadingPage()
        {
            this.InitializeComponent();
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

            base.OnNavigatedTo(e);
        }

        private async void EbookFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Values["selectedIndex"] = EbookFlipView.SelectedIndex;

            {
                await _loadInkForOpeningAsync(e.AddedItems[0] as EbookOpeningViewModel);
            }

            if (e.RemovedItems.Count == 1)
            {
                await _saveInkForOpeningAsync(e.RemovedItems[0] as EbookOpeningViewModel);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Settings = ApplicationData.Current.RoamingSettings.CreateContainer(Ebook.Isbn, ApplicationDataCreateDisposition.Always);

            Task.Run(async () =>
            {
                // Do at the same time
                var pages = await PageStorage.GetPagePathsAsync(Ebook.Isbn);
                var ink = await InkStorage.GetInkPathsAsync(Ebook.Isbn);

                var selectedIndex = (int)Settings.Values["selectedIndex"];

                EbookOpenings.Add(new EbookOpeningViewModel(null, pages[0], ink?.DefaultIfEmpty(null).FirstOrDefault(i => i?.Split('.')[0] == "0")));

                for (int i = 1; i < pages.Length; i++)
                {
                    EbookOpenings.Add(new EbookOpeningViewModel(pages[i], i + 1 == pages.Length ? null : pages[i + 1], ink?.DefaultIfEmpty(null).FirstOrDefault(_ => _.Split('.')[0] == i.ToString())));
                    i++;
                }

                EbookFlipView.SelectionChanged += EbookFlipView_SelectionChanged;

                Settings.Values["selectedIndex"] = EbookFlipView.SelectedIndex = selectedIndex;

                LoadingProgressRing.IsActive = false;
            }).Forget();
        }

        #region Mouse Panning

        Pointer pointer;
        PointerPoint scrollMousePoint;
        double hOff = 1;
        double vOff = 1;

        private void MainScrollviewer_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            pointer = e.Pointer;
            scrollMousePoint = e.GetCurrentPoint(scrollViewer);
            hOff = scrollViewer.HorizontalOffset;
            vOff = scrollViewer.VerticalOffset;
            scrollViewer.CapturePointer(pointer);
        }

        private void MainScrollviewer_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            
            var scrollViewer = sender as ScrollViewer;
            scrollViewer.ReleasePointerCaptures();
        }

        private void MainScrollviewer_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                scrollViewer.ChangeView(hOff + (scrollMousePoint.Position.X - e.GetCurrentPoint(scrollViewer).Position.X),
                                        vOff + (scrollMousePoint.Position.Y - e.GetCurrentPoint(scrollViewer).Position.Y), null);
            }
        }

        private void ScrollViewerMain_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        private void ScrollViewerMain_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        #endregion Mouse Panning

        #region TryShowWindow

        public static async Task TryShowWindowAsync(Ebook ebook) => await TryShowWindowAsync(new EbookViewModel(ebook));

        public static async Task<bool> TryShowWindowAsync(EbookViewModel ebookViewModel)
        {
            AppWindow readingWindow = await AppWindow.TryCreateAsync();
            readingWindow.Title = ebookViewModel.Title;

            Frame readingWindowContentFrame = new Frame();
            readingWindowContentFrame.Navigate(typeof(ReadingPage), ebookViewModel);

            ElementCompositionPreview.SetAppWindowContent(readingWindow, readingWindowContentFrame);

            return await readingWindow.TryShowAsync();
        }

        #endregion TryShowWindow

        #region Right-Click Menu

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText($"hermodsebook:{Ebook.Isbn}?page={EbookFlipView.SelectedIndex * 2}");
            Clipboard.SetContent(dataPackage);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            //flyoutBase.ShowAt(senderElement);
            flyoutBase.ShowAt(sender as UIElement, new FlyoutShowOptions() { Position = e.GetPosition(sender as UIElement) });
        }

        #endregion Right-Click Menu

        #region Ink

        private async Task _loadInkForOpeningAsync(EbookOpeningViewModel opening)
        {
            var container = EbookFlipView.ContainerFromItem(opening);

            if (container != null)
            {
                var item = container as FlipViewItem;
                var index = EbookFlipView.Items.IndexOf(opening);

                InkCanvas ink = item.FindVisualChildren<InkCanvas>().FirstOrDefault();

                if (ink.InkPresenter.StrokeContainer.GetStrokes().Count == 0)
                {
                    await InkStorage.LoadInkAsync(Ebook.Isbn, index, ink.InkPresenter.StrokeContainer);

                    Image inkImage = item.FindVisualChildren<Image>().FirstOrDefault(i => i.Name == "TemporaryInkImage");

                    if (inkImage != null)
                        inkImage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async Task _saveInkForOpeningAsync(EbookOpeningViewModel opening)
        {
            var deselectedItem = EbookFlipView.ContainerFromItem(opening) as FlipViewItem;

            if (deselectedItem != null)
            {
                var deselectedIndex = EbookFlipView.Items.IndexOf(opening);

                var ink = deselectedItem.FindVisualChildren<InkCanvas>().FirstOrDefault();

                await InkStorage.SaveInkAsync(Ebook.Isbn, deselectedIndex, ink.InkPresenter.StrokeContainer);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        #endregion Ink

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await _saveInkForOpeningAsync(EbookFlipView.SelectedItem as EbookOpeningViewModel);
        }
    }
}
