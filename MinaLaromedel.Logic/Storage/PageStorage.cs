using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace MinaLaromedel.Storage
{
    // TODO: Maybe cache them instead, and absolutely set the correct expiration date.
    public static class PageStorage
    {
        private static StorageFolder LocalCacheFolder => ApplicationData.Current.LocalCacheFolder;

        private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

        public static async Task SavePageAsync(Ebook ebook, Stream image, int pageIndex) => await LocalCacheFolder.CreateFileAsync($"{ebook.Isbn}\\{pageIndex}.jpg", CreationCollisionOption.ReplaceExisting, image);

        public static async Task<string> GetPagePathAsync(Ebook ebook, int page) => (await GetPageFileAsync(ebook, page)).Path;

        public static async Task<string[]> GetPagePathsAsync(Ebook ebook) => await GetPagePathsAsync(ebook.Isbn);

        public static async Task<string[]> GetPagePathsAsync(string isbn) => (await (await LocalCacheFolder.GetFolderAsync(isbn)).GetFilesAsync()).OrderByNumberedFileName().Select(pf => pf.Path).ToArray();

        public static async Task<BitmapImage> GetPageImageAsync(Ebook ebook, int page) => await (await GetPageFileAsync(ebook, page)).GetBitmapImageAsync();

        public static async Task<bool> EbookExistsAsync(Ebook ebook) => await LocalCacheFolder.ContainsFolderAsync(ebook.Isbn);

        public static async Task<StorageFile> GetPageFileAsync(Ebook ebook, int pageIndex) => await LocalCacheFolder.GetFileAsync($"{ebook.Isbn}\\{pageIndex}.jpg");

        internal static async Task DeleteEbookAsync(Ebook ebook) => await (await LocalCacheFolder.GetFolderAsync(ebook.Isbn)).DeleteAsync(StorageDeleteOption.PermanentDelete);
    }
}
