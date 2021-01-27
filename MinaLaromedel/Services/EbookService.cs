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
using Windows.Security.Credentials;
using MinaLaromedel.EbookProviders;
using MinaLaromedel.Models;

namespace MinaLaromedel.Services
{
    public static class EbookService
    {
        public static ObservableCollection<EbookViewModel> Ebooks { get; } = new ObservableCollection<EbookViewModel>();

        private static readonly SemaphoreSlim _ebooksAsyncLock = new SemaphoreSlim(1, 1);


        #region Authenticate

        public static async Task<bool> TryAuthenticateAsync()
        {
            var credentials = _getProviderCredentials();

            bool successfullness = true;
            foreach(var credential in credentials)
            {
                if (!await TryAuthenticateAsync(credential))
                    successfullness = false;
            }

            return successfullness;
        }

        public static async Task<bool> TryAuthenticateAsync(PasswordCredential credential)
        {
            IEbookProvider provider = EbookProviderManager.GetProvider(credential);

            if (await provider.AuthenticateAsync(credential))
                return true;
            else
            {
                EbookProviderManager.RemoveProvider(provider);
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
                        foreach(var ebook in ebooks.Where(eb => eb.Expires < DateTime.Today))
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
                var credentials = _getProviderCredentials();

                foreach (var credential in credentials)
                {
                    var provider = EbookProviderManager.GetProvider(credential);

                    Ebook[] ebooks;

                    try
                    {
                        ebooks = await provider.GetEbooksAsync();
                    }
                    catch
                    {
                        await provider.AuthenticateAsync(credential);
                        ebooks = await provider.GetEbooksAsync();
                    }

                    await EbookStorage.SaveEbooksAsync(ebooks);

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // TODO: Support for multiple providers of the same books
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
            }
            finally
            {
                _ebooksAsyncLock.Release();
            }
        }

        public static async Task DownloadEbookAsync(Ebook ebook)
        {
            var credential = _getProviderCredential(ebook.Provider);

            var provider = EbookProviderManager.GetProvider(credential);

            try
            {
                await provider.DownloadEbookAsync(ebook);
            }
            catch
            {
                await provider.AuthenticateAsync(credential);
                await provider.DownloadEbookAsync(ebook);
            }
        }

        private static PasswordCredential _getProviderCredential(string providerName) => (new PasswordVault()).FindAllByResource(providerName).FirstOrDefault();
        private static IReadOnlyList<PasswordCredential> _getProviderCredentials() => (new PasswordVault()).RetrieveAll().Where(pc => !string.IsNullOrEmpty(pc.Resource)).ToArray();
    }
}
