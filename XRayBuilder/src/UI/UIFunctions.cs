using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XRayBuilder.Core.Model;

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
                MessageBox.Show("Specified directory does not exist.", "Directory Not Found...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        public static PromptResultYesNoCancel PromptHandlerYesNoCancel(string title, string message, PromptType type)
        {
            var icon = type switch
            {
                PromptType.Info => MessageBoxIcon.Information,
                PromptType.Warning => MessageBoxIcon.Warning,
                PromptType.Error => MessageBoxIcon.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, icon, MessageBoxDefaultButton.Button3) switch
            {
                DialogResult.Cancel => PromptResultYesNoCancel.Cancel,
                DialogResult.Yes => PromptResultYesNoCancel.Yes,
                DialogResult.No => PromptResultYesNoCancel.No,
                _ => throw new ArgumentOutOfRangeException()
            };
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
