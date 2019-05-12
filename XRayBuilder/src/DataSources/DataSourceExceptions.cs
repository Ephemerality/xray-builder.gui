using System;

namespace XRayBuilderGUI.DataSources
{
    public class FormatChangedException : Exception
    {
        public FormatChangedException(string source, string message, Exception previous = null)
            : base($"Format changed on {source} ({message}).", previous)
        {
        }
    }
}
