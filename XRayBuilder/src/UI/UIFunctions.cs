using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Logging;
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

        public static string GetFile(string title, string defaultFile, string filter = "All files (*.*)|*.*", string initialDir = "")
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
            if (DialogResult.Yes == MessageBox.Show("The document type is not set to EBOK. Would you like this to be updated?\r\nCaution: This feature is experimental and could potentially ruin your book file.", "Incorrect Content Type", MessageBoxButtons.YesNo))
            {
                try
                {
                    using var fs = new FileStream(bookPath, FileMode.Create);
                    md.UpdateCdeContentType();
                    md.Save(fs);
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to update Content Type, could not open with write access. Is the book open in another application?");
                }
            }
        }

        public static string RawMlPath(string filename) => Path.Combine(Environment.CurrentDirectory, "dmp", $"{filename}.rawml");

        public static IMetadata GetAndValidateMetadata(string mobiFile, bool saveRawMl, ILogger logger)
        {
            logger.Log("Extracting metadata...");
            try
            {
                var metadata = MetadataLoader.Load(mobiFile);
                EbokTagPromptOrThrow(metadata, mobiFile);
                IncorrectAsinPromptOrThrow(metadata.Asin);
                if (!Properties.Settings.Default.useNewVersion && metadata.DbName.Length == 31)
                {
                    MessageBox.Show($"WARNING: Database Name is the maximum length. If \"{metadata.DbName}\" is the full book title, this should not be an issue.\r\nIf the title is supposed to be longer than that, you may get an error on your Kindle (WG on firmware < 5.6).\r\nThis can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n");
                }

                if (saveRawMl && metadata.RawMlSupported)
                {
                    logger.Log("Saving rawML to dmp directory...");
                    metadata.SaveRawMl(RawMlPath(Path.GetFileNameWithoutExtension(mobiFile)));
                }
                logger.Log($"Got metadata!\r\nDatabase Name: {metadata.DbName}\r\nUniqueID: {metadata.UniqueId}\r\nASIN: {metadata.Asin}");

                return metadata;
            }
            catch (Exception ex)
            {
                logger.Log($"An error occurred extracting metadata: {ex.Message}\r\n{ex.StackTrace}");
            }

            return null;
        }

        public static void IncorrectAsinPromptOrThrow(string asin)
        {
            if (!AmazonClient.IsAsin(asin)
                && DialogResult.No == MessageBox.Show($"Incorrect ASIN detected: {asin}!\nKindle may not display an X-Ray for this book.\nDo you wish to continue?", "Incorrect ASIN", MessageBoxButtons.YesNo))
            {
                throw new Exception($"Incorrect ASIN detected: {asin}!\r\nKindle may not display an X-Ray for this book.\r\nYou must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) or a MOBI editor (exth 113 and optionally 504) to change this.");
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

        public static object GetPropertyTS(this Control ctrl, string name)
        {
            return ctrl.InvokeRequired
                ? ctrl.Invoke(new Func<object>(() => ctrl.GetPropertyTS(name)))
                : ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, ctrl, null);
        }
    }
}
