using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermods.Novo
{
    public class HermodsNovoEbook
    {
        /// <summary>
        /// E.g. "Biologi 1 Onlinebok (12 mån)".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// E.g. "Liber".
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// E.g. "Licens till 2022-01-04".
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// E.g. "9789147107025".
        /// </summary>
        public string Isbn { get; set; }

        /// <summary>
        /// E.g. "2021-01-04".
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// E.g. "2022-01-04".
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// E.g. "linkresolver.php?isbn=9789147107025".
        /// </summary>
        public Uri Url { get; set; }
    }

}
