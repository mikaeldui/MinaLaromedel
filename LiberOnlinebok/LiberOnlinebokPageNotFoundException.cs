using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiberOnlinebok
{
    public class LiberOnlinebokPageNotFoundException : ApplicationException
    {
        public LiberOnlinebokPageNotFoundException(LiberOnlinebokEbook ebook, int pageIndex) : base($"Couldn't find page {pageIndex} of e-book {ebook.Guid}.")
        {
            Ebook = ebook;
            PageIndex = pageIndex;
        }

        public LiberOnlinebokEbook Ebook { get; }
        public int PageIndex { get; }
    }
}
