using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public static class Logger
    {
        public static RichTextBox ctrl = null;
        public static bool enabled = true;

        public static void Log(string message)
        {
            if (ctrl == null) throw new NullReferenceException("Log control not set.");
            if (!enabled) return;
            if (!message.EndsWith("\r\n")) message += "\r\n";
            ctrl.SafeAppendText(message);
        }
        
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
                List<string> redFlags = new List<string>() { "error", "failed", "problem", "skipping", "warning", "unable" };
                if (redFlags.Any(s => message.ContainsIgnorecase(s)))
                {
                    rtfBox.SelectionStart = rtfBox.TextLength;
                    rtfBox.SelectionLength = 0;
                    rtfBox.SelectionColor = Color.Red;
                }
                rtfBox.AppendText(message);
                rtfBox.SelectionColor = rtfBox.ForeColor;
            }
        }

        public static void SafeClearText(this RichTextBox rtfBox)
        {
            rtfBox.BeginInvoke((Action)(() => rtfBox.Clear()));
        }
    }
}
