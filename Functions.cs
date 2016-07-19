using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace XRayBuilderGUI
{
    public static class Functions
    {
        private static readonly HashSet<char> badChars = new HashSet<char> { '!', '@', '#', '$', '%', '_', '"' };

        //http://www.levibotelho.com/development/c-remove-diacritics-accents-from-a-string/
        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        public static string GetDir(string defaultFolder)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.SelectedPath = defaultFolder;
            if (f.ShowDialog() == DialogResult.OK)
                return f.SelectedPath;
            else
                return defaultFolder;
        }

        public static string GetFile(string defaultFile, string filter = "All files (*.*)|*.*")
        {
            OpenFileDialog f = new OpenFileDialog();
            if (defaultFile != "") f.InitialDirectory = Path.GetDirectoryName(defaultFile);
            f.Filter = filter;
            f.RestoreDirectory = true;
            if (f.ShowDialog() == DialogResult.OK)
                return f.FileName;
            else
                return defaultFile;
        }

        public static string GetExe(string defaultFile)
        {
            OpenFileDialog f = new OpenFileDialog();
            if (defaultFile != "") f.InitialDirectory = Path.GetDirectoryName(defaultFile);
            f.Title = "Browse for the Kindleunpack executable";
            f.Filter = "Application (*.exe)|*.exe";
            f.RestoreDirectory = true;
            if (f.ShowDialog() == DialogResult.OK)
                return f.FileName;
            else
                return defaultFile;
        }

        //Addition open file dialog for books only
        public static string GetBook(string defaultFile)
        {
            OpenFileDialog f = new OpenFileDialog();
            if (defaultFile != "") f.InitialDirectory = Path.GetDirectoryName(defaultFile);
            f.Title = "Open a Kindle book";
            f.Filter = "Kindle Books (*.azw3, *.mobi)|*.azw3; *.mobi";
            f.RestoreDirectory = true;
            if (f.ShowDialog() == DialogResult.OK)
                return f.FileName;
            else
                return defaultFile;
        }

        public static string CleanString(this string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (!badChars.Contains(s[i]))
                    sb.Append(s[i]);
            }
            string cleanedString = sb.ToString();
            //cleanedString = Regex.Replace(cleanedString, @"([\u0000-\u007F])", string.Empty);
            cleanedString = Regex.Replace(cleanedString, @"(“)|(”)", "'");
            cleanedString = cleanedString.Replace("\"", "'")
                .Replace("<br>", string.Empty)
                .Replace("&#133;", "…")
                .Replace("&amp;#133;", "…")
                .Replace("&#169;", string.Empty)
                .Replace("&amp;#169;", string.Empty)
                .Replace("&#174;", string.Empty)
                .Replace("&amp;#174;", string.Empty);
            cleanedString = Regex.Replace(cleanedString, @"\t|\n|\r|•", " ", RegexOptions.Multiline);
            cleanedString = Regex.Replace(cleanedString, @"\s+", " ", RegexOptions.Multiline);
            cleanedString = Regex.Replace(cleanedString, @"^ | $", string.Empty, RegexOptions.Multiline);
            return cleanedString.Trim();
        }

        public static string ImageToBase64(Image image, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                var imageBytes = ms.ToArray();
                image.Dispose();

                // Convert byte[] to Base64 String
                var base64String = Convert.ToBase64String(imageBytes);
                ms.Flush();
                return base64String;
            }
        }

        public static Bitmap Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
                imageBytes.Length);

            // Convert byte[] to Image , based on @Crulex comment, the below line has no need since MemoryStream already initialized
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            Bitmap bitmap = new Bitmap(image);
            return bitmap;
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static string GetBookOutputDirectory(string author, string title)
        {
            string path, newAuthor, newTitle;
            newAuthor = RemoveInvalidFileChars(author);
            newTitle = RemoveInvalidFileChars(title);
            path = Path.Combine(Properties.Settings.Default.outDir,
                String.Format(@"{0}\{1}", newAuthor, newTitle));
            if (!author.Equals(newAuthor) || !title.Equals(newTitle))
                MessageBox.Show("The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.", "Invalid Characters");
            Directory.CreateDirectory(path);
            return path;
        }

        public static string RemoveInvalidFileChars(string filename)
        {
            char[] fileChars = Path.GetInvalidFileNameChars();
            return new string(filename.Where(x => !fileChars.Contains(x)).ToArray());
        }

        public static bool ExtrasExist(string location, string asin)
        {
            {
                if (File.Exists(location + String.Format(@"\AuthorProfile.profile.{0}.asc", asin)) &&
                    File.Exists(location + String.Format(@"\EndActions.data.{0}.asc", asin)))
                    return true;
            }
            return false;
        }

        public static bool RefreshPreview(string type)
        {
            return true;
        }

        public static string GetTempDirectory()
        {
            string path;
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }

        public static string TimeStamp()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var time = String.Format("{0:H:mm:ss}", DateTime.Now);
            var date = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
            return String.Format("Running X-Ray Builder GUI v{0}. Log started on {1} at {2}.\r\n",
                version, date, time);
        }

        //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
        public static Unpack.Metadata GetMetaDataInternal(string mobiFile, string outDir, bool saveRawML, string randomFile = "")
        {
            List<string> output = new List<string>();
            FileStream fs = new FileStream(mobiFile, FileMode.Open, FileAccess.Read);
            if (fs == null)
                throw new Exception("Unable to open mobi file.");
            Unpack.Metadata md = new Unpack.Metadata(fs);

            if (md.mobiHeader.exthHeader == null)
                throw new Exception("No EXT Header found. Ensure this book was processed with Calibre then try again.");

            if (md.mobiHeader.exthHeader.CDEType != "EBOK")
                if (md.mobiHeader.exthHeader.CDEType.Length == 4 &&
                    DialogResult.Yes == MessageBox.Show("The document type is not set to EBOK. Would you like this to be updated?\r\n" +
                        "Caution: This feature is experimental and could potentially ruin your book file.", "Incorrect Content Type", MessageBoxButtons.YesNo))
                {
                    fs.Close();
                    fs = new FileStream(mobiFile, FileMode.Open, FileAccess.ReadWrite);
                    if (fs == null)
                        throw new Exception("Unable to re-open mobi file for writing.");
                    md.mobiHeader.exthHeader.UpdateCDEContentType(fs);
                }
                else
                {
                    fs.Close();
                    throw new Exception("The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\n" +
                        "You must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a MOBI editor (exth 501) to change this.");
                }

            string ASIN = md.ASIN;
            Match match = Regex.Match(ASIN, "(^B[A-Z0-9]{9})");
            if (!match.Success && DialogResult.No == MessageBox.Show(String.Format("Incorrect ASIN detected: {0}!\n" +
                                      "Kindle may not display an X-Ray for this book.\n" +
                                      "Do you wish to continue?", ASIN), "Incorrect ASIN", MessageBoxButtons.YesNo))
            {
                fs.Close();
                throw new Exception(String.Format("Incorrect ASIN detected: {0}!\r\n" +
                                  "Kindle may not display an X-Ray for this book.\r\n" +
                                  "You must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) " +
                                  "or a MOBI editor (exth 113 and optionally 504) to change this.", ASIN));
            }

            if (!Properties.Settings.Default.useNewVersion && md.DBName.Length == 31)
            {
                MessageBox.Show(String.Format(
                    "WARNING: Database Name is the maximum length. If \"{0}\" is the full book title, this should not be an issue.\r\n" +
                    "If the title is supposed to be longer than that, you may get an error on your Kindle (WG on firmware < 5.6).\r\n" +
                    "This can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n",
                    md.DBName));
            }
            
            if (saveRawML)
            {
                // Everything else checked out, grab rawml and write to the temp file
                md.rawMLPath = randomFile + "\\" + Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
                byte[] rawML = md.getRawML(fs);
                using (FileStream rawMLFile = new FileStream(md.rawMLPath, FileMode.Create, FileAccess.Write))
                {
                    rawMLFile.Write(rawML, 0, rawML.Length);
                }
            }
            fs.Close();
            return md;
        }


        public static string GetPageCount(string rawML, BookInfo bookInfo)
        {
            string output = "";
            int lineLength = 0;
            double lineCount = 0;
            int pageCount = 0;
            if (!File.Exists(rawML) || bookInfo == null)
            {
                output = "Error: RawML could not be found, aborting.\r\nPath: " + rawML;
                return output;
            }
            HtmlAgilityPack.HtmlDocument bookDoc = new HtmlAgilityPack.HtmlDocument { OptionAutoCloseOnEnd = true };
            bookDoc.Load(rawML, Encoding.UTF8);
            HtmlAgilityPack.HtmlNodeCollection booklineNodes = null;
            booklineNodes = bookDoc.DocumentNode.SelectNodes("//p") ?? bookDoc.DocumentNode.SelectNodes("//div");
            if (booklineNodes == null)
            {
                output = "An error occurred while estimating page count!";
                return output;
            }
            foreach (HtmlAgilityPack.HtmlNode line in booklineNodes)
            {
                lineLength = line.InnerText.Length + 1;
                if (lineLength < 70)
                {
                    lineCount++;
                    continue;
                }
                lineCount += Math.Ceiling((double)lineLength / 70);
            }
            pageCount = Convert.ToInt32(Math.Ceiling(lineCount / 31));
            if (pageCount == 0)
            {
                output = "An error occurred while estimating page count!";
                return output;
            }
            double minutes = pageCount * 1.2890625;
            TimeSpan span = TimeSpan.FromMinutes(minutes);
            bookInfo.pagesInBook = pageCount.ToString();
            bookInfo.readingHours = span.Hours.ToString();
            bookInfo.readingMinutes = span.Minutes.ToString();
            output = (String.Format("Typical time to read: {0} hours and {1} minutes ({2} pages)"
                , span.Hours, span.Minutes, bookInfo.pagesInBook));
            return output;
        }

        public static List<string> GetMetaData(string mobiFile, string outDir, string randomFile, string mobiUnpack)
        {
            if (mobiUnpack == null) throw new ArgumentNullException("mobiUnpack");
            List<string> output = new List<string>();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = mobiUnpack;
            startInfo.Arguments = "-r -d \"" + mobiFile + @""" """ + randomFile + @"""";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            startInfo.UseShellExecute = false;
            // Hide console window
            startInfo.CreateNoWindow = true;
            string rawMl = "";
            string unpackInfo = "";
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.BeginErrorReadLine();
                        using (StreamReader reader = process.StandardOutput)
                        {
                            unpackInfo = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                output.Add(String.Format("An error occurred while running Kindleunpack: {0}\r\n", ex.Message));
                MessageBox.Show("Error while running Kindleunpack. See the output log for details.");
                return output;
            }
            rawMl = Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
            //Was the unpack successful?
            if (!unpackInfo.Contains("Write opf\r\n") && !unpackInfo.Contains("\r\nCompleted"))
            {
                output.Add("Kindleunpack returned: " + unpackInfo +
                           "\r\nAn error occurred during unpack. See above info for details.\r\n");
                return output;
            }
            //Attempt to find the .rawml unpacked from the mobi
            rawMl = randomFile + @"/mobi8/" + rawMl;
            if (!File.Exists(rawMl))
                rawMl = randomFile + @"/mobi7/" + Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
            if (!File.Exists(rawMl))
            {
                output.Add("Error finding .rawml file. Path: " + rawMl);
                MessageBox.Show("Error finding .rawml.", "RAWML Error");
                return output;
            }

            string databaseName = "";
            string uniqid = "";
            string asin = "";
            string incorrectAsin = "";
            string author = "";
            string title = "";

            Match match = Regex.Match(unpackInfo, @"ASIN\s*(.*)");
            if (match.Success && match.Groups.Count > 1)
            {
                incorrectAsin = match.Groups[1].Value.Replace("\r", "");
                //Improve actual Amazon ASIN matching
                match = Regex.Match(match.Groups[1].Value, "(^B[A-Z0-9]{9})");
                if (!match.Success)
                {
                    if (DialogResult.No == MessageBox.Show(String.Format("Incorrect ASIN detected: {0}!\n" +
                                      "Kindle may not display an X-Ray for this book.\n" +
                                      "Do you wish to continue?", incorrectAsin), "Incorrect ASIN", MessageBoxButtons.YesNo))
                    {
                        output.Add(
                            String.Format("Incorrect ASIN detected: {0}!\r\n" +
                                          "Kindle may not display an X-Ray for this book.\r\n" +
                                          "You must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) " +
                                          "or a Mobi editor (exth 113 and 504) to change this.", incorrectAsin));
                        return output;
                    }
                }
                asin = incorrectAsin;
            }
            match = Regex.Match(unpackInfo, @"(\d*) unique_id");
            if (match.Success && match.Groups.Count > 1)
                uniqid = match.Groups[1].Value;
            match = Regex.Match(unpackInfo, @"Document Type\s*(\w*)");
            if (match.Success && match.Groups.Count > 1)
            {
                if (match.Groups[1].Value != "EBOK")
                {
                    output.Add(
                        "The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\nYou must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a Mobi editor (exth 501) to change this.");
                    MessageBox.Show(
                        "The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\nYou must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a Mobi editor (exth 501) to change this.");
                    return output;
                }
            }
            // Find author name in Kindleunpack output
            match = Regex.Match(unpackInfo, @" Creator\s{2,}(.*)");
            if (match.Success && match.Groups.Count > 1)
                author = match.Groups[1].Value.Replace("\r", "");

            // Find book title in Kindleunpack output
            match = Regex.Match(unpackInfo, @"Title in header at offset.*'(.*)'");
            if (!match.Success || match.Groups.Count <= 1)
                match = Regex.Match(unpackInfo, @" Updated_Title\s*(.*)");
            if (match.Success && match.Groups.Count > 1)
                title = match.Groups[1].Value.Replace("\r", "");

            //Attempt to get database name from the mobi file.
            //If mobi_unpack ran successfully, then hopefully this will always be valid?
            byte[] dbinput = new byte[32];
            using (FileStream stream = File.Open(mobiFile, FileMode.Open, FileAccess.Read))
            {
                if (stream == null)
                {
                    output.Add("Error opening mobi file (stream error).");
                    MessageBox.Show("Error opening mobi file (stream error).");
                    return output;
                }
                int bytesRead = stream.Read(dbinput, 0, 32);
                if (bytesRead != 32)
                {
                    output.Add("Error reading from mobi file.");
                    MessageBox.Show("Error reading from mobi file.");
                    return output;
                }
                databaseName = Encoding.Default.GetString(dbinput).Trim('\0');
            }

            if (databaseName == "" || uniqid == "" || asin == "")
            {
                output.Add(String.Format(
                    "Error: Missing metadata.\r\nDatabase Name: {0}\r\nASIN: {1}\r\nUniqueID: {2}", databaseName, asin,
                    uniqid));
                MessageBox.Show("Missing metadata. See output log for details.", "Metadata Error");
                return output;
            }
            else if (!Properties.Settings.Default.useNewVersion && databaseName.Length == 31)
            {
                MessageBox.Show(
                    String.Format(
                        "WARNING: Database Name is the maximum length. If \"{0}\" is the full book title, this should not be an issue.\r\n" +
                        "If the title is supposed to be longer than that, you may get an error WG on your Kindle.\r\n" +
                        "This can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n",
                        databaseName));
            }

            output.Add(asin);
            output.Add(uniqid);
            output.Add(databaseName);
            output.Add(rawMl);
            output.Add(author);
            output.Add(title);

            return output;
        }

        public static void RunNotepad(string filename)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "notepad";
            startInfo.Arguments = filename;
            startInfo.UseShellExecute = false;
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error trying to launch notepad: " + e.Message);
            }
        }

        //http://stackoverflow.com/questions/4123590/serialize-an-object-to-xml
        public static string Serialize<T>(T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlserializer = new XmlSerializer(typeof (T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        public static void Save<T>(T output, string fileName)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(typeof (T));
                serializer.Serialize(writer, output);
                writer.Flush();
            }
        }

        //http://stackoverflow.com/questions/14562415/xml-deserialization-generic-method
        public static List<T> DeserializeList<T>(string filePath)
        {
            var itemList = new List<T>();

            if (File.Exists(filePath))
            {
                var serializer = new XmlSerializer(typeof (List<T>));
                TextReader reader = new StreamReader(filePath, Encoding.UTF8);
                try
                {
                    itemList = (List<T>) serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error processing XML file: " + ex.Message +
                        "\r\nIf the error contains a (#, #), the first number is the line the error occurred on.",
                        "XML Error");
                    return null;
                }
                reader.Close();
            }

            return itemList;
        }

        /// <summary>
        /// Fix author name if in last, first format or if multiple authors present (returns first author)
        /// </summary>
        public static string FixAuthor(string author)
        {
            if (author.IndexOf(';') > 0)
                author = author.Split(';')[0];
            if (author.IndexOf(',') > 0)
            {
                string[] parts = author.Split(',');
                author = parts[1].Trim() + " " + parts[0].Trim();
            }
            return author;
        }

        /// <summary>
        /// Trim spaces in author names that contain initials (helps with searching)
        /// </summary>
        public static string TrimAuthor(string author)
        {
            Regex regex = new Regex(@"( [A-Z]\.)|( [a-z]\.)", RegexOptions.Compiled);
            Match match = Regex.Match(author, @"( [A-Z]\.)|( [a-z]\.)", RegexOptions.Compiled);
            if (match.Success)
            {
                foreach (Match m in regex.Matches(author))
                {
                    author = author.Replace(m.Value, m.Value.Trim());
                }
            }
            return author;
        }

        public static string ExpandUnicode(string input)
        {
            StringBuilder output = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > 255)
                {
                    byte[] uniBytes = Encoding.Unicode.GetBytes(input.Substring(i, 1));
                    output.AppendFormat(@"\u{0:X2}{1:X2}", uniBytes[1], uniBytes[0]);
                }
                else
                    output.Append(input[i]);
            }
            return output.ToString();
        }
        
        // Shamelessly stolen from http://www.mobileread.com/forums/showthread.php?t=185565
        public static byte[] CheckBytes(byte[] bytesToCheck)
        {
            byte[] buffer = (byte[])bytesToCheck.Clone();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return buffer;
        }
    }
}