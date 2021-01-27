using GalaSoft.MvvmLight.Messaging;
using Hermods.Novo;
using Liber.Onlinebok;
using MinaLaromedel.Messages;
using MinaLaromedel.Models;
using MinaLaromedel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MinaLaromedel.EbookProviders
{
    [EbookProviderName("Hermods Novo")]
    public class HermodsNovoEbookProvider : IEbookProvider, IDisposable
    {
        private HermodsNovoClient _hermodsNovoClient = new HermodsNovoClient();

        public async Task<bool> AuthenticateAsync(PasswordCredential credential)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));

            try
            {
                credential.RetrievePassword();
                await _hermodsNovoClient.AuthenticateAsync(credential.UserName, credential.Password);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose() => ((IDisposable)_hermodsNovoClient).Dispose();

        public async Task DownloadEbookAsync(Ebook ebook)
        {
            switch (ebook.Publisher)
            {
                case "Liber":
                    
                    var hermodsClient = _hermodsNovoClient;

                    LiberOnlinebokDocument document;
                    LiberOnlinebokAssetsClient assetsClient;

                    using (LiberOnlinebokClient liberClient = await _hermodsNovoClient.GetLiberOnlinebokClientAsync(ebook.ProviderAttributes["url"]))
                    {
                        document = await liberClient.GetDocumentAsync();
                        assetsClient = await liberClient.GetAssetsClientAsync();
                    }

                    using (assetsClient)
                    {
                        var pageAssets = document.Content.ContentItems.Values.Select(val => (val.OrderingIndex, val.Assets.First().Uri)).ToArray();

                        foreach (var asset in pageAssets)
                        {
                            var ebookPage = await assetsClient.GetAssetAsync(asset.Uri);

                            await PageStorage.SavePageAsync(ebook, ebookPage, asset.OrderingIndex);

                            Messenger.Default.Send(new DownloadStatusMessage(Array.IndexOf(pageAssets, asset), pageAssets.Length), ebook.Isbn);
                        }
                    }

                    break;

                default:
                    throw new NotImplementedException("This publisher hasn't been implemented yet.");
            }
        }

        public async Task<Ebook[]> GetEbooksAsync()
        {
            var hermodsEbooks = await _hermodsNovoClient.GetEbooksAsync();

            return hermodsEbooks.Select(he => new Ebook
            {
                Title = he.Title,
                Isbn = he.Isbn,
                Expires = he.EndDate,
                Publisher = he.Publisher,

                Provider = "Hermods Novo",
                ProviderAttributes =
                {
                    { "url", he.Url?.ToString() }
                }
            }).ToArray();
        }
    }
}
