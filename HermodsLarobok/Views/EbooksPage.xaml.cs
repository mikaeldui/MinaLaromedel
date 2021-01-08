using HermodsLarobok.Models;
using HermodsLarobok.Storage;
using HermodsLarobok.ViewModels;
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

namespace HermodsLarobok.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EbooksPage : Page
    {
        public ObservableCollection<EbookViewModel> Ebooks { get; } = new ObservableCollection<EbookViewModel>();

        public EbooksPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var ebooks = await EbookStorage.GetEbooksAsync();

            if (ebooks != null)
                foreach (var ebook in ebooks)
                    Ebooks.Add(new EbookViewModel(ebook));
            else
            {
                ebooks = await App.HermodsNovoClient.GetEbooksAsync();
                foreach (var ebook in ebooks)
                    Ebooks.Add(new EbookViewModel(ebook));

                await EbookStorage.SaveEbooksAsync(ebooks);
            }
        }

        private async void EbooksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var selectedBook = e.AddedItems[0] as EbookViewModel;

            await ReadingPage.TryShowWindowAsync(selectedBook);
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var ebooks = await App.HermodsNovoClient.GetEbooksAsync();

            // remove
            {
                var oldEbooks = Ebooks.Where(eb => !ebooks.Select(ebb => ebb.Isbn).Contains(eb.Isbn));
                foreach (var oldEbook in oldEbooks)
                    Ebooks.Remove(oldEbook);
            }

            // Add
            foreach (var ebook in ebooks)
            {
                var existing = Ebooks.FirstOrDefault(eb => eb.Isbn == ebook.Isbn);

                if (existing == null)
                    Ebooks.Add(new EbookViewModel(ebook));
            }

            await EbookStorage.SaveEbooksAsync(ebooks);
        }
    }
}
