using MinaLaromedel.Storage;
using MinaLaromedel.ViewModels;
using Hermods.Novo;
using Liber.Onlinebok;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using System.Net;
using GalaSoft.MvvmLight.Messaging;
using MinaLaromedel.Messages;
using System.Threading;

namespace MinaLaromedel.Services
{
    public static class EbookService
    {
        private static Lazy<HermodsNovoClient> _hermodsNovoClient = new Lazy<HermodsNovoClient>();

        public static ObservableCollection<EbookViewModel> Ebooks { get; } = new ObservableCollection<EbookViewModel>();

        private static readonly SemaphoreSlim _ebooksAsyncLock = new SemaphoreSlim(1, 1);

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
            await _ebooksAsyncLock.WaitAsync();

            try
            {
                if (Ebooks.Count == 0)
                {
                    var ebooks = await EbookStorage.GetEbooksAsync();

                    if (ebooks != null)
                    {
                        // One place to do it.
                        foreach(var ebook in ebooks.Where(eb => eb.EndDate <= DateTime.Today))
                        {
                            await PageStorage.DeleteEbookAsync(ebook);

                            // Maybe unpin, or change the tiles image.
                        }

                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            foreach (var ebook in ebooks)
                                Ebooks.Add(new EbookViewModel(ebook));
                        });
                    }
                }
            }
            finally
            {
                _ebooksAsyncLock.Release();
            }
        }

        public static async Task RefreshEbooksAsync(CoreDispatcher dispatcher)
        {
            await _ebooksAsyncLock.WaitAsync();

            try
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
            finally
            {
                _ebooksAsyncLock.Release();
            }
        }

        public static async Task DownloadEbookAsync(HermodsNovoEbook hermodsEbook)
        {
            switch (hermodsEbook.Publisher)
            {
                case "Liber":
                    try
                    {
                        var hermodsClient = _hermodsNovoClient.Value;

                        LiberOnlinebokClient liberClient;
                        LiberOnlinebokDocument document;
                        LiberOnlinebokAssetsClient assetsClient;

                        try
                        {
                            liberClient = await _hermodsNovoClient.Value.GetLiberOnlinebokClientAsync(hermodsEbook);
                        }
                        catch
                        {
                            await TryAuthenticateAsync();
                            liberClient = await _hermodsNovoClient.Value.GetLiberOnlinebokClientAsync(hermodsEbook);
                        }

                        using (liberClient)
                        {
                            document = await liberClient.GetDocumentAsync();
                            assetsClient = await liberClient.GetAssetsClientAsync();
                        }

                        using (assetsClient)
                        {
                            var pageAssets = document.Content.ContentItems.Values.Select(val => (val.OrderingIndex, val.Assets.First().Uri)).ToArray();

                            foreach(var asset in pageAssets)
                            {
                                var ebookPage = await assetsClient.GetAssetAsync(asset.Uri);

                                await PageStorage.SavePageAsync(hermodsEbook, ebookPage, asset.OrderingIndex);

                                Messenger.Default.Send(new DownloadStatusMessage(Array.IndexOf(pageAssets, asset), pageAssets.Length), hermodsEbook.Isbn);
                            }
                        }
                    }
                    catch (LiberOnlinebokAssetNotFoundException)
                    {
                        return; // Done
                    }
                    break;

                default:
                    throw new NotImplementedException("This publisher hasn't been implemented yet.");
            }
        }
    }
}
