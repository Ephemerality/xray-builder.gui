using System.Threading;
using XRayBuilder.Core.Libraries.Progress;

namespace XRayBuilder.Console.Logic
{
    public sealed class ProgressConsole : IProgressBar
    {
        private int _value;
        private int _max;

        public void Add(int value)
        {
            Interlocked.Add(ref _value, value);
        }

        public void Set(int value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        public void SetMax(int max)
        {
            Interlocked.Exchange(ref _max, max);
        }

        public void Set(int value, int max)
        {
            Set(value);
            SetMax(max);
        }
    }
}