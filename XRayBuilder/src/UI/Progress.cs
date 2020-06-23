using System;
using System.Windows.Forms;
using XRayBuilder.Core.Libraries.Progress;

namespace XRayBuilderGUI.UI
{
    public sealed class ProgressBarCtrl : IProgressBar
    {
        private readonly ProgressBar _prgBar;
        private readonly object _semaphore = new object();

        public ProgressBarCtrl(ProgressBar prgBar)
        {
            _prgBar = prgBar;
        }

        public void Add(int value)
        {
            lock (_semaphore)
            {
                var current = (int) _prgBar.GetPropertyThreadSafe(nameof(_prgBar.Value));
                var max = (int)_prgBar.GetPropertyThreadSafe(nameof(_prgBar.Maximum));
                _prgBar.SetPropertyThreadSafe(nameof(_prgBar.Value), Math.Min(max, current + value));
            }
        }

        public void Set(int value)
        {
            _prgBar.SetPropertyThreadSafe(nameof(_prgBar.Value), value);
        }

        public void SetMax(int max)
        {
            _prgBar.SetPropertyThreadSafe(nameof(_prgBar.Maximum), max);
        }

        public void Set(int value, int max)
        {
            Set(value);
            SetMax(max);
        }
    }
}
