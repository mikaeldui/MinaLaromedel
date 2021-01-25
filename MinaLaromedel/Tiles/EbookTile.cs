using Hermods.Novo;
using MinaLaromedel.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.StartScreen;

namespace MinaLaromedel.Tiles
{
    public class EbookTile
    {
        public static async Task<bool> RequestCreateAsync(HermodsNovoEbook ebook)
        {
            // Provide all the required info in arguments so that when user
            // clicks your tile, you can navigate them to the correct content
            string arguments = "action=viewEbook&isbn=" + ebook.Isbn;

            var uri = new Uri(await _getThumbnailPathAsync(ebook));

            // Initialize the tile with required arguments
            SecondaryTile tile = new SecondaryTile(_getTileId(ebook.Isbn), ebook.Title, arguments, uri, TileSize.Default);

            return await tile.RequestCreateAsync();
        }

        public static async Task<bool> RequestDeleteAsync(HermodsNovoEbook ebook)
        {
            SecondaryTile toBeDeleted = new SecondaryTile(_getTileId(ebook.Isbn));

            return await toBeDeleted.RequestDeleteAsync();
        }

        public static bool Exists(string isbn) => SecondaryTile.Exists(_getTileId(isbn));

        private static string _getTileId(string isbn) => "isbn-" + isbn;

        private static async Task<string> _getThumbnailPathAsync(HermodsNovoEbook ebook, int page = 1)
        {
            var thumbnailFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"tiles\\thumbnails\\{ebook.Isbn}.jpg", CreationCollisionOption.OpenIfExists);
            var properties = await thumbnailFile.GetBasicPropertiesAsync();

            if (properties.Size == 0)
            {
                var pageFile = await PageStorage.GetPageFileAsync(ebook, page);

                var thumbnail = await pageFile.GetScaledImageAsThumbnailAsync(ThumbnailMode.DocumentsView, 150);

                using (var stream = thumbnail.AsStreamForRead())
                using (var outStream = await thumbnailFile.OpenStreamForWriteAsync())
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                }
            }

            return "ms-appdata:///local/tiles/thumbnails/" + thumbnailFile.Name;
        }
    }
}
