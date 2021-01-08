using HermodsLarobok.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace HermodsLarobok.Storage
{
    public static class PageStorage
    {
        public static async Task SavePageAsync(Ebook ebook, Stream image, int page)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var ebookFolder = await storageFolder.CreateFolderAsync(ebook.Isbn, Windows.Storage.CreationCollisionOption.OpenIfExists);

            var file = await ebookFolder.CreateFileAsync($"{page}.jpg", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                image.Seek(0, SeekOrigin.Begin);
                await image.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
        }

        public static async Task SaveEbookAsync(Ebook ebook, Stream[] pages)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var ebookFolder = await storageFolder.CreateFolderAsync(ebook.Isbn, Windows.Storage.CreationCollisionOption.ReplaceExisting);

            for (int i = 0; i < pages.Length; i++)
            {
                var file = await ebookFolder.CreateFileAsync($"{i}.jpg", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var page = pages[i];
                    page.Seek(0, SeekOrigin.Begin);
                    await page.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
        }

        private static async Task<StorageFile> _getPageFileAsync(Ebook ebook, int page)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var ebookFolder = await storageFolder.GetFolderAsync(ebook.Isbn);

            return await ebookFolder.GetFileAsync($"{page}.jpg");
        }

        public static async Task<string> GetPagePathAsync(Ebook ebook, int page) => (await _getPageFileAsync(ebook, page)).Path;

        public static async Task<string[]> GetPagePathsAsync(Ebook ebook) => await GetPagePathsAsync(ebook.Isbn);

        public static async Task<string[]> GetPagePathsAsync(string isbn)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var ebookFolder = await storageFolder.GetFolderAsync(isbn);

            return (await ebookFolder.GetFilesAsync()).OrderBy(pf => pf.Name.Split('.')[0].PadLeft(5, '0')).Select(pf => pf.Path).ToArray();
        }

        public static async Task<BitmapImage> GetPageImageAsync(Ebook ebook, int page)
        {
            using (var stream = await (await _getPageFileAsync(ebook, page)).OpenReadAsync())
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(stream);
                return image;
            }
        }

        public static async Task<bool> EbookExistsAsync(Ebook ebook)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var folders = await storageFolder.GetFoldersAsync();

            return folders.Any(f => f.Name == ebook.Isbn);
        }
    }
}
