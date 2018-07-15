using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    // ReSharper disable once InconsistentNaming
    public static class UIFunctions
    {
        public static string GetDir(string defaultFolder)
        {
            FolderBrowserDialog f = new FolderBrowserDialog { SelectedPath = defaultFolder };
            return f.ShowDialog() == DialogResult.OK ? f.SelectedPath : defaultFolder;
        }

        public static string GetFile(string title, string defaultFile, string filter = "All files (*.*)|*.*", string initialDir = "")
        {
            OpenFileDialog f = new OpenFileDialog();
            if (!string.IsNullOrEmpty(initialDir))
                f.InitialDirectory = initialDir;
            else if (!string.IsNullOrEmpty(defaultFile))
                f.InitialDirectory = Path.GetDirectoryName(defaultFile);
            f.Filter = filter;
            f.RestoreDirectory = true;
            return f.ShowDialog() == DialogResult.OK ? f.FileName : defaultFile;
        }

        public static string GetExe(string defaultFile) => GetFile("Browse for the Kindleunpack executable", defaultFile, "Application (*.exe)|*.exe");

        public static string GetBook(string defaultFile) => GetFile("Open a Kindle book", defaultFile, "Kindle Books (*.azw3, *.mobi)|*.azw3; *.mobi");

        public static void ValidateFilename(string author, string title)
        {
            var newAuthor = RemoveInvalidFileChars(author);
            var newTitle = RemoveInvalidFileChars(title);
            if (!author.Equals(newAuthor) || !title.Equals(newTitle))
                MessageBox.Show("The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.", "Invalid Characters");
        }

        public static string RemoveInvalidFileChars(string filename)
        {
            char[] fileChars = Path.GetInvalidFileNameChars();
            return new string(filename.Where(x => !fileChars.Contains(x)).ToArray());
        }
    }
}
