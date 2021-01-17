using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XRayBuilder.Core.Unpack;

namespace XRayBuilderGUI.UI
{
    // ReSharper disable once InconsistentNaming
    public static class UIFunctions
    {
        public static string GetDir(string defaultFolder)
        {
            using var f = new FolderBrowserDialog { SelectedPath = defaultFolder };
            return f.ShowDialog() == DialogResult.OK ? f.SelectedPath : defaultFolder;
        }

        public static void OpenDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                MessageBox.Show(
                    @"Specified directory does not exist.",
                    @"Directory Not Found…",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            else
                Process.Start(dir);
        }

        public static string GetFile([Localizable(true)] string title, string defaultFile, string filter = "All files (*.*)|*.*", string initialDir = "")
        {
            using var f = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                RestoreDirectory = true
            };

            if (!string.IsNullOrEmpty(initialDir))
                f.InitialDirectory = initialDir;
            else if (!string.IsNullOrEmpty(defaultFile))
                f.InitialDirectory = Path.GetDirectoryName(defaultFile);

            return f.ShowDialog() == DialogResult.OK ? f.FileName : defaultFile;
        }

        public static string GetBook(string defaultFile) => GetFile("Open a Kindle book", defaultFile, "Kindle Books (*.azw3, *.mobi, *.kfx)|*.azw3; *.mobi; *.kfx");

        public static string RemoveInvalidFileChars(string filename)
        {
            var fileChars = Path.GetInvalidFileNameChars();
            return new string(filename.Where(x => !fileChars.Contains(x)).ToArray());
        }

        public static void EbokTagPromptOrThrow(IMetadata md, string bookPath)
        {
            if (md.CdeContentType == "EBOK")
                return;
            if (DialogResult.Yes != MessageBox.Show(
                @"The document type is not set to EBOK." +
                Environment.NewLine + Environment.NewLine +
                @"Would you like this to be updated?" +
                Environment.NewLine +
                @"Caution: This feature is experimental" +
                Environment.NewLine +
                @"and could potentially ruin your book file.",
                @"Incorrect Content Type",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)) return;
            try
            {
                using var fs = new FileStream(bookPath, FileMode.Create);
                md.UpdateCdeContentType();
                md.Save(fs);
            }
            catch (IOException)
            {
                MessageBox.Show(@"Failed to update Content Type, could not open with write access." +
                                Environment.NewLine +
                                @"Is the book open in another application?",
                    @"Access Denied…",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class UIExtensionMethods
    {
        public static void SetPropertyThreadSafe(this Control ctrl, string name, object value)
        {
            if (ctrl.InvokeRequired)
                ctrl.BeginInvoke(new Action(() => SetPropertyThreadSafe(ctrl, name, value)));
            else
                ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, ctrl, new[] { value });
        }

        public static object GetPropertyThreadSafe(this Control ctrl, string name)
        {
            return ctrl.InvokeRequired
                ? ctrl.Invoke(new Func<object>(() => ctrl.GetPropertyThreadSafe(name)))
                : ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, ctrl, null);
        }
    }
}
