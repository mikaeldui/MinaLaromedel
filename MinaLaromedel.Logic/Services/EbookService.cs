using MinaLaromedel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel.Services
{
    public static class EbookService
    {
        public static async Task<Ebook[]> LoadEbooksAsync()
        {
            var ebooks = await EbookStorage.GetEbooksAsync();

            if (ebooks != null)
            {
                // One place to do it.
                foreach (var ebook in ebooks.Where(eb => eb.Expires < DateTime.Today))
                {
                    await PageStorage.DeleteEbookAsync(ebook);

                    // Maybe unpin, or change the tiles image.
                }                    
            }

            return ebooks;
        }
    }
}
