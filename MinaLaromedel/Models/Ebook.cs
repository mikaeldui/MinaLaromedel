using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel.Models
{
    public class Ebook
    {
        public string Title { get; set; }

        public string Isbn { get; set; }

        public DateTime? Expires { get; set; }

        public EbookChapter[] Chapters { get; set; }
        public EbookPage[] Pages { get; set; }
    }

    public class EbookChapter
    {
        public string Title { get; set; }
        public int PageIndex { get; set; }
    }

    public class EbookPage
    {
        public string Text { get; set; }
    }
}
