using System;

namespace Ephemerality.Unpack.Exceptions
{
    public sealed class UnpackException : Exception
    {
        public UnpackException(string message) : base(message) { }
        public UnpackException(string message, Exception e) : base(message, e) { }
    }
}