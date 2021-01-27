using Hermods.Novo;
using MinaLaromedel.EbookProviders;
using MinaLaromedel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace MinaLaromedel.Tasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // From 1.0.5.0
            {
                const string FILE_NAME = "ebooksList.json";

                StorageFolder StorageFolder = ApplicationData.Current.LocalFolder;

                var hermodsNovoEbooks = await StorageFolder.ContainsFileAsync(FILE_NAME) ? await StorageFolder.GetObjectAsync<HermodsNovoEbook[]>(FILE_NAME) : new HermodsNovoEbook[0];

                if (hermodsNovoEbooks.Any(hne => hne.Url != null))
                {
                    var ebooks = hermodsNovoEbooks.Select(HermodsNovoEbookProvider.ConvertToEbook).ToArray();
                    await EbookStorage.SaveEbooksAsync(ebooks);
                }
            }

            deferral.Complete();
        }
    }
}
