using System;

namespace HermodsNovo
{
    public class HermodsNovoInvalidCredentialsException : ApplicationException
    {
        public HermodsNovoInvalidCredentialsException(string message) : base(message) { }

        public HermodsNovoInvalidCredentialsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
