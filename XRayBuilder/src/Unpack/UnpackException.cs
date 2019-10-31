using System;

namespace XRayBuilderGUI.Unpack
{
    public sealed class UnpackException : Exception
    {
        public UnpackException(string message) : base(message) { }
    }
}