using HermodsNovo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace HermodsLarobok.Storage
{
    public static class EbookStorage
    {
        private const string KEY = "ebooksList.json";

        public static async Task<HermodsNovoEbook[]> GetEbooksAsync()
        {
            var helper = new LocalObjectStorageHelper();

            // Read complex/large objects 
            if (await helper.FileExistsAsync(KEY))
            {
                return await helper.ReadFileAsync<HermodsNovoEbook[]>(KEY);
            }
            else
                return null;
        }

        public static async Task<HermodsNovoEbook> GetEbookAsync(string isbn) => (await GetEbooksAsync()).First(eb => eb.Isbn == isbn);

        public static async Task SaveEbooksAsync(HermodsNovoEbook[] ebooks)
        {
            var helper = new LocalObjectStorageHelper();
            await helper.SaveFileAsync(KEY, ebooks);
        }
    }
}
