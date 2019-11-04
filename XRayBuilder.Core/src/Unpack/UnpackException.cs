using System;

namespace XRayBuilder.Core.Unpack
{
    public sealed class UnpackException : Exception
    {
        public UnpackException(string message) : base(message) { }
    }
}