using MinaLaromedel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MinaLaromedel.Storage
{
    public static class EbookStorage
    {
        private const string FILE_NAME = "ebooksList.json";

        private static StorageFolder StorageFolder => ApplicationData.Current.LocalFolder;

        /// <summary>
        /// Returns an empty array if no e-books were found.
        /// </summary>
        public static async Task<Ebook[]> GetEbooksAsync() => await StorageFolder.ContainsFileAsync(FILE_NAME) ? await StorageFolder.GetObjectAsync<Ebook[]>(FILE_NAME) : new Ebook[0];

        public static async Task SaveEbooksAsync(Ebook[] ebooks) => await StorageFolder.CreateFileAsync(FILE_NAME, CreationCollisionOption.ReplaceExisting, ebooks);
    }
}
