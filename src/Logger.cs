using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public static class Logger
    {
        public static RichTextBox ctrl;
        public static bool enabled = true;

        public static void Log(string message)
        {
            if (!enabled) return;
            if (!message.EndsWith("\r\n")) message += "\r\n";
            if (ctrl == null)
                Console.WriteLine(message);
            else
                ctrl.SafeAppendText(message);
        }

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
