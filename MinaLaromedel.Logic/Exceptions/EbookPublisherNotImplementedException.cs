using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel
{
    public class EbookPublisherNotImplementedException : EbookException
    {
        public EbookPublisherNotImplementedException(Ebook ebook) : base(ebook, $"The publisher {ebook.Publisher} of e-book {ebook.Title} (ISBN: {ebook.Isbn}) has not been implemented yet for the provider: {ebook.Provider}.") { }
        public EbookPublisherNotImplementedException(Ebook ebook, Exception innerException) : base(ebook, $"The publisher {ebook.Publisher} of e-book {ebook.Title} (ISBN: {ebook.Isbn}) has not been implemented yet for the provider: {ebook.Provider}. See innerException for more details.", innerException) { }
    }
}
