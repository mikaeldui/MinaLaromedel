using Hermods.Novo;
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

        public static async Task<HermodsNovoEbook[]> GetEbooksAsync() => await StorageFolder.ContainsFileAsync(FILE_NAME) ? await StorageFolder.GetObjectAsync<HermodsNovoEbook[]>(FILE_NAME) : null;

        public static async Task SaveEbooksAsync(HermodsNovoEbook[] ebooks) => await StorageFolder.CreateFileAsync(FILE_NAME, CreationCollisionOption.ReplaceExisting, ebooks);
    }
}
