using System;
using System.Threading;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public class ProgressBarCtrl
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
                var current = (int) _prgBar.GetPropertyTS(nameof(_prgBar.Value));
                var max = (int)_prgBar.GetPropertyTS(nameof(_prgBar.Maximum));
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
        // Set max value
        // Set value
        // Add value

        //public static void Log(string message)
        //{
        //    if (!enabled) return;
        //    if (!message.EndsWith("\r\n")) message += "\r\n";
        //    if (ctrl == null)
        //        Console.WriteLine(message);
        //    else
        //        ctrl.SafeAppendText(message);
        //}

        //public static void SafeAppendText(this RichTextBox rtfBox, string message)
        //{
        //    if (rtfBox.InvokeRequired)
        //        rtfBox.BeginInvoke(new Action(() => SafeAppendText(rtfBox, message)));
        //    else
        //    {

        //        if (!rtfBox.Text.StartsWith("Running X-Ray Builder GUI"))
        //            rtfBox.AppendText(Functions.TimeStamp());

        //        if (message.ContainsIgnorecase("successfully"))
        //        {
        //            rtfBox.SelectionStart = rtfBox.TextLength;
        //            rtfBox.SelectionLength = 0;
        //            rtfBox.SelectionColor = Color.Green;
        //        }
        //        List<string> redFlags = new List<string> { "error", "failed", "problem", "skipping", "warning", "unable" };
        //        if (redFlags.Any(message.ContainsIgnorecase))
        //        {
        //            rtfBox.SelectionStart = rtfBox.TextLength;
        //            rtfBox.SelectionLength = 0;
        //            rtfBox.SelectionColor = Color.Red;
        //        }
        //        rtfBox.AppendText(message);
        //        rtfBox.SelectionColor = rtfBox.ForeColor;
        //        rtfBox.Refresh();
        //    }
        //}
    }
}
