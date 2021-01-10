using HermodsLarobok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;

namespace HermodsLarobok.Storage
{
    public static class InkStorage
    {
        public static async Task SaveInkAsync(string isbn, int pageOpening, IInkStrokeContainer ink)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var ebookFolder = await storageFolder.CreateFolderAsync(isbn + "\\ink", Windows.Storage.CreationCollisionOption.OpenIfExists);

            var file = await ebookFolder.CreateFileAsync($"{pageOpening}.gif", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            Windows.Storage.CachedFileManager.DeferUpdates(file);

            IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

            using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
            {
                await ink.SaveAsync(outputStream);
                await outputStream.FlushAsync();
            }

            stream.Dispose();

            // Finalize write so other apps can update file.
            Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
        }

        public static async Task LoadInkAsync(string isbn, int pageOpening, IInkStrokeContainer ink)
        {
            Windows.Storage.StorageFile file = await _getInkFileAsync(isbn, pageOpening);

            if (file == null)
                return;

            IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            // Read from file.
            using (var inputStream = stream.GetInputStreamAt(0))
            {
                await ink.LoadAsync(inputStream);
            }
            stream.Dispose();            
        }

        /// <summary>
        /// Gets paths for openings.
        /// </summary>
        public static async Task<string[]> GetInkPathsAsync(string isbn)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                var ebookFolder = await storageFolder.GetFolderAsync(isbn + "\\ink");

                return (await ebookFolder.GetFilesAsync()).OrderBy(pf => pf.Name.Split('.')[0].PadLeft(5, '0')).Select(pf => pf.Path).ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        private static async Task<StorageFile> _getInkFileAsync(string isbn, int pageOpening)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var inkFolder = await storageFolder.GetFolderAsync(isbn + "\\ink");

            return await inkFolder.TryGetItemAsync($"{pageOpening}.gif") as StorageFile;
        }
    }
}
