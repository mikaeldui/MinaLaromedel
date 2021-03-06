﻿using MinaLaromedel.Storage;
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
using MinaLaromedel.Services;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;
using Sentry;
using Windows.UI.Xaml;

namespace MinaLaromedel.Managers
{
    public static class EbookManager
    {
        public static ObservableCollection<Ebook> Ebooks { get; } = new ObservableCollection<Ebook>();

        private static readonly SemaphoreSlim _ebooksAsyncLock = new SemaphoreSlim(1, 1);

        public static async Task LoadEbooksAsync()
        {
            if (Ebooks.Count != 0)
                return;

            await _ebooksAsyncLock.WaitAsync();

            try
            {
                var ebooks = await EbookService.LoadEbooksAsync();

                await UIThread.RunAsync(() =>
                {
                    foreach (var ebook in ebooks)
                        Ebooks.Add(ebook);
                });

                EbooksLoaded?.Invoke(null, new EventArgs());
            }
            finally
            {
                _ebooksAsyncLock.Release();
            }
        }

        public static async Task RefreshEbooksAsync()
        {
            await _ebooksAsyncLock.WaitAsync();

            try
            {
                var credentials = CredentialsService.GetProviderCredentials();

                foreach (var credential in credentials)
                {
                    var provider = EbookProviderService.GetProvider(credential);

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

                    await UIThread.RunAsync(() =>
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
                                Ebooks.Add(ebook);
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
            var credential = CredentialsService.GetProviderCredential(ebook.Provider);

            var provider = EbookProviderService.GetProvider(credential);

            try
            {

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
            catch (EbookException e)
            {
                ContentDialog downloadErrorDialog = new ContentDialog()
                {
                    Title = "Kunde inte ladda ner e-bok",
                    Content = $"E-boken {ebook.Title} (publicerad av {ebook.Publisher}) kunde inte laddas ner. Skapa gärna ett nytt Issue på GitHub om felet inträffar flera gånger.",
                    CloseButtonText = "Stäng",
                    PrimaryButtonText = "Öppna GitHub",
                    PrimaryButtonCommand = new RelayCommand(async () =>
                    {
                        var uriBing = new Uri(@"https://github.com/mikaeldui/MinaLaromedel/issues");

                        await Windows.System.Launcher.LaunchUriAsync(uriBing);
                    })
                };

                SentrySdk.CaptureException(e);

                await UIThread.RunAsync(async () => await downloadErrorDialog.ShowAsync());
            }
        }

        public static event EventHandler EbooksLoaded;
    }
}
