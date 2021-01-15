using MinaLaromedel.Services;
using MinaLaromedel.Storage;
using MinaLaromedel.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

namespace MinaLaromedel.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EbooksPage : Page
    {
        public ObservableCollection<EbookViewModel> Ebooks => EbookService.Ebooks;

        public EbooksPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _ = EbookService.LoadEbooksAsync(Dispatcher)
                            .ContinueWith(async _ => 
                            { 
                                if (EbookService.Ebooks.Count == 0)
                                {
                                    try
                                    {
                                        await EbookService.RefreshEbooksAsync(Dispatcher);
                                    }
                                    catch
                                    {

                                    }
                                }

                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    RefreshButton.IsEnabled = true;
                                });
                            });

            base.OnNavigatedTo(e);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = EbookService.RefreshEbooksAsync(Dispatcher);
        }

        private async void EbooksGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedBook = e.ClickedItem as EbookViewModel;

            if (clickedBook.IsDownloaded)
                await ReadingPage.TryShowWindowAsync(clickedBook);
            else if (clickedBook.IsDownloadable && !clickedBook.IsDownloading && clickedBook.Download.CanExecute(null))
                clickedBook.Download.Execute(null);
        }
    }
}
