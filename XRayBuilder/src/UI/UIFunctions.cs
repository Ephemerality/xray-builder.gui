﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Prompt;

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
                Functions.ShellExecute(dir);
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

        public static PromptResultYesNo PromptHandlerYesNo(string title, string message, PromptType type, Form owner)
        {
            return owner.MessageBoxShowSafe(message, title, MessageBoxButtons.YesNo, type.ToMessageBoxIcon(), MessageBoxDefaultButton.Button2) switch
            {
                DialogResult.Yes => PromptResultYesNo.Yes,
                DialogResult.No => PromptResultYesNo.No,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static PromptResultYesNoCancel PromptHandlerYesNoCancel(string title, string message, PromptType type, Form owner)
        {
            return owner.MessageBoxShowSafe(message, title, MessageBoxButtons.YesNoCancel, type.ToMessageBoxIcon(), MessageBoxDefaultButton.Button3) switch
            {
                DialogResult.Cancel => PromptResultYesNoCancel.Cancel,
                DialogResult.Yes => PromptResultYesNoCancel.Yes,
                DialogResult.No => PromptResultYesNoCancel.No,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static string ShortcutText(Keys keys)
        {
            var result = new StringBuilder();
            if ((keys & Keys.Control) == Keys.Control)
                result.Append("Ctrl");

            if ((keys & Keys.Alt) == Keys.Alt)
                result.Append("Alt");

            if ((keys & Keys.Shift) == Keys.Shift)
                result.Append("Shift");

            if (result.Length > 0)
                result.Append('+');

            keys = keys & ~Keys.Control & ~Keys.Alt;
            result.Append(keys.ToString());

            return result.ToString();
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class UIExtensionMethods
    {
        public static MessageBoxIcon ToMessageBoxIcon(this PromptType promptType)
            => promptType switch
            {
                PromptType.Info => MessageBoxIcon.Information,
                PromptType.Warning => MessageBoxIcon.Warning,
                PromptType.Error => MessageBoxIcon.Error,
                PromptType.Question => MessageBoxIcon.Question,
                _ => throw new ArgumentOutOfRangeException(nameof(promptType), promptType, null)
            };

        public static void SetPropertyThreadSafe(this Control ctrl, string name, object value)
        {
            if (ctrl.InvokeRequired)
                ctrl.BeginInvoke(() => SetPropertyThreadSafe(ctrl, name, value));
            else
                ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, ctrl, new[] { value });
        }

        public static object GetPropertyThreadSafe(this Control ctrl, string name)
        {
            return ctrl.InvokeRequired
                ? ctrl.Invoke(() => ctrl.GetPropertyThreadSafe(name))
                : ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, ctrl, null);
        }

        /// <summary>
        /// Thread-safe MessageBox.Show - invokes the messagebox on <paramref name="form"/>
        /// </summary>
        public static DialogResult MessageBoxShowSafe(this Form form, [Localizable(true)] string msg, [Localizable(true)] string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return form.Invoke(() => MessageBox.Show(form, msg, caption, buttons, icon, def));
        }
    }
}
