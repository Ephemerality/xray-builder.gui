using System;

namespace Ephemerality.Unpack
{
    public sealed class UnpackException : Exception
    {
        public UnpackException(string message) : base(message) { }
    }
}