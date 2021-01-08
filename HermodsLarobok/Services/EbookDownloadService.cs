using HermodsLarobok.Clients;
using HermodsLarobok.Models;
using HermodsLarobok.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermodsLarobok.Services
{
    public static class EbookDownloadService
    {
        public static async Task DownloadEbookAsync(Ebook ebook)
        {
            var guid = await App.HermodsNovoClient.GetLiberGuidAsync(ebook);

            using (var liberClient = new LiberOnlinebokClient())
            {
                var gotPage = true;

                int page = 1;
                while (gotPage)
                {
                    var image = await liberClient.GetPageAsync(guid, page);

                    if (image == null)
                        return;

                    await PageStorage.SavePageAsync(ebook, image, page);

                    page++;
                }
            }
        }
    }
}
