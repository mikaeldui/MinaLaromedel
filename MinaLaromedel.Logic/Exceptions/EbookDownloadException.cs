using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel
{
    public class EbookDownloadException : EbookException
    {
        public EbookDownloadException(Ebook ebook) : base(ebook, $"An exception occured while downloading the e-book {ebook.Title} (ISBN: {ebook.Isbn}) from the publisher {ebook.Publisher} through the provider {ebook.Provider}.") { }
        public EbookDownloadException(Ebook ebook, Exception innerException) : base(ebook, $"An exception occured while downloading the e-book {ebook.Title} (ISBN: {ebook.Isbn}) from the publisher {ebook.Publisher} through the provider {ebook.Provider}. See innerException for more details.", innerException) { }
    }
}
