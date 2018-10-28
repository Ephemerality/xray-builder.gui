using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    public class RtfLogger
    {
        private readonly RichTextBox _ctrl;
        public bool Enabled = true;

        public RtfLogger(RichTextBox ctrl)
        {
            _ctrl = ctrl;
        }

        public void Log(LogEventArgs e)
        {
            if (!Enabled) return;
            if (!e.Message.EndsWith("\r\n")) e.Message += "\r\n";
            if (_ctrl == null)
                Console.WriteLine(e.Message);
            else
                _ctrl.SafeAppendText(e.Message);
        }
    }

    public static class RtfExtensions
    {
        // TODO: Rely on log levels rather than strings to determine colour
        public static void SafeAppendText(this RichTextBox rtfBox, string message)
        {
            if (rtfBox.InvokeRequired)
                rtfBox.BeginInvoke(new Action(() => SafeAppendText(rtfBox, message)));
            else
            {

                if (!rtfBox.Text.StartsWith("Running X-Ray Builder GUI"))
                    rtfBox.AppendText(Functions.TimeStamp());

                if (message.ContainsIgnorecase("successfully"))
                {
                    rtfBox.SelectionStart = rtfBox.TextLength;
                    rtfBox.SelectionLength = 0;
                    rtfBox.SelectionColor = Color.Green;
                }

                List<string> redFlags = new List<string> { "error", "failed", "problem", "skipping", "unable" };
                if (redFlags.Any(message.ContainsIgnorecase))
                {
                    rtfBox.SelectionStart = rtfBox.TextLength;
                    rtfBox.SelectionLength = 0;
                    rtfBox.SelectionColor = Color.Red;
                }

                if (message.ContainsIgnorecase("warning"))
                {
                    rtfBox.SelectionStart = rtfBox.TextLength;
                    rtfBox.SelectionLength = 0;
                    rtfBox.SelectionColor = Color.DarkOrange;
                }

                rtfBox.AppendText(message);
                rtfBox.SelectionColor = rtfBox.ForeColor;
                rtfBox.Refresh();
            }
        }

        public static void SafeClearText(this RichTextBox rtfBox)
        {
            rtfBox.BeginInvoke((Action)rtfBox.Clear);
        }
    }
}
