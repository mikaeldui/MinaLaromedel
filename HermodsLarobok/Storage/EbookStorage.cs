using HermodsLarobok.Models;
using Microsoft.Toolkit.Uwp.Helpers;
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
        private const string KEY = "ebooksList";

        public static async Task<Ebook[]> GetEbooksAsync()
        {
            var helper = new LocalObjectStorageHelper();

            // Read complex/large objects 
            if (await helper.FileExistsAsync(KEY))
            {
                return await helper.ReadFileAsync<Ebook[]>(KEY);
            }
            else
                return null;
        }

        public static async Task<Ebook> GetEbookAsync(string isbn) => (await GetEbooksAsync()).First(eb => eb.Isbn == isbn);

        public static async Task SaveEbooksAsync(Ebook[] ebooks)
        {
            var helper = new LocalObjectStorageHelper();
            await helper.SaveFileAsync(KEY, ebooks);
        }
    }
}
