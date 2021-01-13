using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HermodsLarobok.Storage
{
    public interface IStorageHelper
    {
        Task<bool> FileExistsAsync(string name);
        Task<TType> ReadFileAsync<TType>(string name);
        Task SaveFileAsync<TType>(string name, TType toSave);
    }

    public abstract class ObjectStorageHelperBase : IStorageHelper
    {
        private StorageFolder _storageFolder;

        protected ObjectStorageHelperBase(StorageFolder storageFolder) => _storageFolder = storageFolder;

        public async Task<bool> FileExistsAsync(string name) => null != await _storageFolder.TryGetItemAsync(name);

        public async Task<TType> ReadFileAsync<TType>(string name)
        {
            var file = await _storageFolder.GetFileAsync(name);

            using (var randomAccessStream = await file.OpenReadAsync())
            using (var stream = randomAccessStream.AsStreamForRead())
            using (var streamReader = new StreamReader(stream))
            {
                var text = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<TType>(text);
            }
        }

        public async Task SaveFileAsync<TType>(string name, TType toSave)
        {
            var file = await _storageFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            using (var streamWriter = new StreamWriter(stream))
            {
                var text = JsonConvert.SerializeObject(toSave);
                await streamWriter.WriteAsync(text);
                await streamWriter.FlushAsync();
            }
        }
    }

    public class LocalObjectStorageHelper : ObjectStorageHelperBase
    {
        public LocalObjectStorageHelper() : base(ApplicationData.Current.LocalFolder)
        {
        }
    }

    public class RoamingObjectStorageHelper : ObjectStorageHelperBase
    {
        public RoamingObjectStorageHelper(StorageFolder storageFolder) : base(ApplicationData.Current.RoamingFolder)
        {
        }
    }
}
