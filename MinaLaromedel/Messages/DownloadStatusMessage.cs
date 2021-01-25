using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel.Messages
{
    public class DownloadStatusMessage
    {
        public DownloadStatusMessage(int done, int total)
        {
            Done = done;
            Total = total;
        }

        public int Done { get; }
        public int Total { get; }

        float PercentDone => Done / Total;
    }
}
