using GalaSoft.MvvmLight.Messaging;
using Hermods.Novo;
using Liber.Onlinebok;
using MinaLaromedel.Messages;
using MinaLaromedel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MinaLaromedel.EbookProviders
{
    [EbookProviderName(PROVIDER_NAME)]
    public class HermodsNovoEbookProvider : IEbookProvider, IDisposable
    {
        private const string PROVIDER_NAME = "Hermods Novo";

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

                    try
                    {

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
                    }
                    catch(Exception e)
                    {
                        throw new EbookDownloadException(ebook, e);
                    }

                    break;

                default:
                    throw new EbookPublisherNotImplementedException(ebook);
            }
        }

        public async Task<Ebook[]> GetEbooksAsync()
        {
            var hermodsEbooks = await _hermodsNovoClient.GetEbooksAsync();

            return hermodsEbooks.Select(ConvertToEbook).ToArray();
        }

        private static EbookChapter[] _getEbookChapters(LiberOnlinebokDocument document) => document.Structure.Root.Children.Select(c => c.Value).Select(c => new EbookChapter
        {
            Title = c.Label,
            PageIndex = document.Content.ContentItems
                .Select(ci => ci.Value)
                .First(ci => ci.Uuid == c.ContentId)
                .OrderingIndex
        }).ToArray();

        public static Ebook ConvertToEbook(HermodsNovoEbook hermodsNovoEbook) => new Ebook
        {
            Title = hermodsNovoEbook.Title,
            Isbn = hermodsNovoEbook.Isbn,
            Expires = hermodsNovoEbook.EndDate,
            Publisher = hermodsNovoEbook.Publisher,

            Provider = PROVIDER_NAME,
            ProviderAttributes =
                {
                    { "url", hermodsNovoEbook.Url?.ToString() }
                }
        };
    }
}
