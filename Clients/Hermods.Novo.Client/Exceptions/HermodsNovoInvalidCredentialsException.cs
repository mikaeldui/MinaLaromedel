using System;

namespace Hermods.Novo
{
    public class HermodsNovoInvalidCredentialsException : ApplicationException
    {
        public HermodsNovoInvalidCredentialsException(string message) : base(message) { }

        public HermodsNovoInvalidCredentialsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
