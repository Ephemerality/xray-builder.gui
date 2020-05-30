using System;

namespace XRayBuilder.Core.DataSources
{
    public class FormatChangedException : Exception
    {
        public FormatChangedException(string source, string message, Exception previous = null)
            : base($"Format changed on {source} ({message}).", previous)
        {
        }
    }
}
