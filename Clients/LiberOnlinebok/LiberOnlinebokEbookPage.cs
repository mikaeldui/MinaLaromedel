using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiberOnlinebok
{
    public class LiberOnlinebokEbookPage
    {
        public LiberOnlinebokEbookPage(Stream stream, int pageIndex, LiberOnlinebokEbook ebook)
        {
            Stream = stream;
            PageIndex = pageIndex;
            Ebook = ebook;
        }

        public Stream Stream { get; }
        public int PageIndex { get; }
        public LiberOnlinebokEbook Ebook { get; }
    }
}
