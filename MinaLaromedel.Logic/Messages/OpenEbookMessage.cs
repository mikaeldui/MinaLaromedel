using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel.Messages
{
    public class OpenEbookMessage
    {
        public OpenEbookMessage(string isbn)
        {
            Isbn = isbn;
        }

        public string Isbn { get; set; }
    }
}
