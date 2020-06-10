using System;

namespace XRayBuilder.Core.Model.Exceptions
{
    public sealed class InitializationException : Exception
    {
        public InitializationException(string message, Exception previous = null) : base(message, previous) { }
    }
}