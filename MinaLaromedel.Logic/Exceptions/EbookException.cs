using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel
{
    public class EbookException : ApplicationException
    {
        public EbookException(Ebook ebook, string message) : base(message)
        {
            Ebook = ebook;
        }

        public EbookException(Ebook ebook, string message, Exception innerException) : base(message, innerException)
        {
            Ebook = ebook;
        }

        public Ebook Ebook { get; }
    }
}
