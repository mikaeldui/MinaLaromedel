using MinaLaromedel.Storage;
using MinaLaromedel.ViewModels;
using HermodsNovo;
using LiberOnlinebok;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;

namespace MinaLaromedel.Services
{
    public static class EbookService
    {
        private static Lazy<HermodsNovoClient> _hermodsNovoClient = new Lazy<HermodsNovoClient>();

        public static ObservableCollection<EbookViewModel> Ebooks { get; } = new ObservableCollection<EbookViewModel>();

        #region Authenticate

        public static async Task<bool> TryAuthenticateAsync() => await TryAuthenticateAsync(SettingsService.Username, SettingsService.Password);

        public static async Task<bool> TryAuthenticateAsync(string username, string password)
        {
            try
            {
                await _hermodsNovoClient.Value.AuthenticateAsync(username, password);
                SettingsService.Username = username;
                SettingsService.Password = password;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Authenticate

        public static async Task LoadEbooksAsync(CoreDispatcher dispatcher)
        {
            if (Ebooks.Count > 0)
                return;

            var ebooks = await EbookStorage.GetEbooksAsync();

            if (ebooks != null)
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                        foreach (var ebook in ebooks)
                            Ebooks.Add(new EbookViewModel(ebook));
                });
        }

        public static async Task RefreshEbooksAsync(CoreDispatcher dispatcher)
        {
            HermodsNovoEbook[] ebooks;
            try
            {
                ebooks = await _hermodsNovoClient.Value.GetEbooksAsync();
            }
            catch
            {
                await TryAuthenticateAsync();
                ebooks = await _hermodsNovoClient.Value.GetEbooksAsync();
            }

            await EbookStorage.SaveEbooksAsync(ebooks);

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // remove
                {
                    var oldEbooks = Ebooks.Where(eb => !ebooks.Select(ebb => ebb.Isbn).Contains(eb.Isbn)).ToArray();
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
            });
        }

        public static async Task DownloadEbookAsync(HermodsNovoEbook hermodsEbook)
        {
            var liberEbook = await _hermodsNovoClient.Value.GetLiberOnlinebokEbookAsync(hermodsEbook);

            try
            {
                using (var liberClient = new LiberOnlinebokClient())
                {
                    var gotPage = true;

                    int pageIndex = 1;
                    while (gotPage)
                    {
                        var ebookPage = await liberClient.GetPageAsync(pageIndex, liberEbook);

                        await PageStorage.SavePageAsync(hermodsEbook, ebookPage.Stream, pageIndex);

                        pageIndex++;
                    }
                }
            }
            catch (LiberOnlinebokPageNotFoundException)
            {
                return; // Done
            }
        }
    }
}
