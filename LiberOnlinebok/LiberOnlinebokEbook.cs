using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiberOnlinebok
{
    public class LiberOnlinebokEbook
    {
        public LiberOnlinebokEbook(Guid guid)
        {
            Guid = guid;
        }

        public Guid Guid { get; }
    }
}
