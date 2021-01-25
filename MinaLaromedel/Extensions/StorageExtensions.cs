using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Windows.Storage
{
    public static class StorageExtensions
    {
        /// <summary>
        /// Creates a new file in the current folder. This method also specifies what to do if a file with the same name already exists in the current folder, and the content of the new file.
        /// </summary>
        /// <param name="folder">The folder in which to create the new file.</param>
        /// <param name="desiredName">The name of the new file to create in the current folder.</param>
        /// <param name="options">One of the enumeration values that determines how to handle the collision if a file with the specified *desiredName* already exists in the current folder.</param>
        /// <param name="content">The content of the new file.</param>
        /// <returns>When this method completes, it returns a StorageFile that represents the new file.</returns>
        public static async Task<StorageFile> CreateFileAsync(this StorageFolder folder, string desiredName, CreationCollisionOption options, Stream content)
        {
            var file = await folder.CreateFileAsync(desiredName, options);
            using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                content.Seek(0, SeekOrigin.Begin);
                await content.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
            return file;
        }

        public static async Task<StorageFile> CreateFileAsync<TObject>(this StorageFolder folder, string desiredName, CreationCollisionOption options, TObject content)
        {
            var file = await folder.CreateFileAsync(desiredName, options);
            await FileIO.WriteTextAsync(file, await Task.Run(() => JsonConvert.SerializeObject(content)));
            return file;
        }

        public static async Task<TObject> GetObjectAsync<TObject>(this StorageFile storageFile) => await Task.Run(async () => JsonConvert.DeserializeObject<TObject>(await FileIO.ReadTextAsync(storageFile)));

        public static async Task<TObject> GetObjectAsync<TObject>(this StorageFolder storageFolder, string name) => await (await storageFolder.GetFileAsync(name)).GetObjectAsync<TObject>();

        public static async Task<BitmapImage> GetBitmapImageAsync(this StorageFile storageFile)
        {
            using (var stream = await storageFile.OpenReadAsync())
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(stream);
                return image;
            }
        }

        public static async Task<bool> ContainsItemAsync(this StorageFolder storageFolder, string name) => null != await storageFolder.TryGetItemAsync(name);

        public static async Task<bool> ContainsFolderAsync(this StorageFolder storageFolder, string folderName) => null != await storageFolder.TryGetItemAsync(folderName) as StorageFolder;

        public static async Task<bool> ContainsFileAsync(this StorageFolder storageFolder, string fileName) => null != (await storageFolder.TryGetItemAsync(fileName)) as StorageFile;

        public static IOrderedEnumerable<StorageFile> OrderByNumberedFileName(this IEnumerable<StorageFile> storageFiles, int padLeft = 5) => storageFiles.OrderBy(pf => pf.Name.Split('.')[0].PadLeft(padLeft, '0'));
    }
}
