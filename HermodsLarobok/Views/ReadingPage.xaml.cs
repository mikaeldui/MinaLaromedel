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

            base.OnNavigatedTo(e);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Values["selectedIndex"] = EbookFlipView.SelectedIndex;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Settings = ApplicationData.Current.RoamingSettings.CreateContainer(Ebook.Isbn, ApplicationDataCreateDisposition.Always);

            AsyncContext.Run(async () =>
            {
                var pages = await PageStorage.GetPagePathsAsync(Ebook.Isbn);

                var selectedIndex = (int)Settings.Values["selectedIndex"];

                EbookOpenings.Add(new EbookOpeningViewModel(null, pages[0]));

                for (int i = 1; i < pages.Length; i++)
                {
                    EbookOpenings.Add(new EbookOpeningViewModel(pages[i], i + 1 == pages.Length ? null : pages[i + 1]));
                    i++;
                }

                Settings.Values["selectedIndex"] = EbookFlipView.SelectedIndex = selectedIndex;

                LoadingProgressRing.IsActive = false;
            });
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
    }
}
