using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermods.Novo
{
    public class HermodsNovoUnauthenticatedException : ApplicationException
    {
        public HermodsNovoUnauthenticatedException(string message) : base(message) { }

        public HermodsNovoUnauthenticatedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
