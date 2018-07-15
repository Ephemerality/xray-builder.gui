using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using HtmlAgilityPack;
using XRayBuilderGUI.Unpack;

namespace XRayBuilderGUI
{
    public static class Functions
    {
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

        // TODO: Clean this up more cause it still sucks
        public static string Clean(this string str)
        {
            (string[] searches, string replace)[] replacements =
            {
                (new[] {"&#169;", "&amp;#169;", "&#174;", "&amp;#174;", "&mdash;", @"</?[a-z]+>" }, ""),
                (new[] { "“", "”", "\"" }, "'"),
                (new[] { "&#133;", "&amp;#133;", @" \. \. \." }, "…"),
                (new[] { " - ", "--" }, "—"),
                (new[] { @"\t|\n|\r|•", @"\s+"}, " "),
                (new[] { @"\. …$"}, "."),
                (new[] {"@", "#", @"\$", "%", "_", }, "")
            };
            foreach (var (s, r) in replacements)
            {
                str = Regex.Replace(str, $"({string.Join("|", s)})", r, RegexOptions.Multiline);
            }
            return str.Trim();
        }

        public static string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static Bitmap Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                return new Bitmap(ms);
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new[]
                {
                    new [] {.3f, .3f, .3f, 0, 0},
                    new [] {.59f, .59f, .59f, 0, 0},
                    new [] {.11f, .11f, .11f, 0, 0},
                    new [] {0f, 0f, 0f, 1f, 0f},
                    new [] {0f, 0f, 0f, 0f, 1f}
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

        public static string GetBookOutputDirectory(string author, string title, bool create)
        {
            var newAuthor = RemoveInvalidFileChars(author);
            var newTitle = RemoveInvalidFileChars(title);
            var path = Path.Combine(Properties.Settings.Default.outDir, $"{newAuthor}\\{newTitle}");
            if (create)
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
            if (File.Exists(location + $"\\AuthorProfile.profile.{asin}.asc") && File.Exists(location + $"\\EndActions.data.{asin}.asc"))
                return true;
            return false;
        }

        public static string GetTempDirectory()
        {
            string path;
            do
            {
                path = Path.Combine(Properties.Settings.Default.tmpDir, Path.GetRandomFileName());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }

        public static string TimeStamp()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var time = String.Format("{0:HH:mm:ss}", DateTime.Now);
            var date = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
            return $"Running X-Ray Builder GUI v{version}. Log started on {date} at {time}.\r\n";
        }

        //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
        public static Metadata GetMetaDataInternal(string mobiFile, string outDir, bool saveRawML, string randomFile = "")
        {
            FileStream fs = new FileStream(mobiFile, FileMode.Open, FileAccess.Read);
            if (fs == null)
                throw new Exception("Unable to open mobi file.");
            Metadata md = new Metadata(fs);
            try
            {
                if (md.mobiHeader.exthHeader == null)
                    throw new Exception("No EXT Header found. Ensure this book was processed with Calibre then try again.");

                if (md.mobiHeader.exthHeader.CDEType != "EBOK")
                    if (md.mobiHeader.exthHeader.CDEType.Length == 4 &&
                        DialogResult.Yes == MessageBox.Show("The document type is not set to EBOK. Would you like this to be updated?\r\n" +
                            "Caution: This feature is experimental and could potentially ruin your book file.", "Incorrect Content Type", MessageBoxButtons.YesNo))
                    {
                        fs = new FileStream(mobiFile, FileMode.Open, FileAccess.ReadWrite);
                        md.mobiHeader.exthHeader.UpdateCDEContentType(fs);
                    }
                    else
                    {
                        throw new Exception("The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\n" +
                            "You must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a MOBI editor (exth 501) to change this.");
                    }

                string ASIN = md.ASIN;
                Match match = Regex.Match(ASIN, "(^B[A-Z0-9]{9})");
                if (!match.Success && DialogResult.No == MessageBox.Show($"Incorrect ASIN detected: {ASIN}!\n" +
                                          "Kindle may not display an X-Ray for this book.\n" +
                                          "Do you wish to continue?", "Incorrect ASIN", MessageBoxButtons.YesNo))
                {
                    throw new Exception($"Incorrect ASIN detected: {ASIN}!\r\n" +
                                      "Kindle may not display an X-Ray for this book.\r\n" +
                                      "You must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) " +
                                      "or a MOBI editor (exth 113 and optionally 504) to change this.");
                }

                if (!Properties.Settings.Default.useNewVersion && md.DBName.Length == 31)
                {
                    MessageBox.Show(
                        $"WARNING: Database Name is the maximum length. If \"{md.DBName}\" is the full book title, this should not be an issue.\r\n" +
                        "If the title is supposed to be longer than that, you may get an error on your Kindle (WG on firmware < 5.6).\r\n" +
                        "This can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n");
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
            }
            finally
            {
                fs.Dispose();
            }
            return md;
        }


        public static string GetPageCount(string rawML, BookInfo bookInfo)
        {
            string output;
            double lineCount = 0;
            if (!File.Exists(rawML) || bookInfo == null)
            {
                output = "Error: RawML could not be found, aborting.\r\nPath: " + rawML;
                return output;
            }
            HtmlAgilityPack.HtmlDocument bookDoc = new HtmlAgilityPack.HtmlDocument { OptionAutoCloseOnEnd = true };
            bookDoc.Load(rawML, Encoding.UTF8);
            var booklineNodes = bookDoc.DocumentNode.SelectNodes("//p") ?? bookDoc.DocumentNode.SelectNodes("//div");
            if (booklineNodes == null)
            {
                output = "An error occurred while estimating page count!";
                return output;
            }
            foreach (HtmlNode line in booklineNodes)
            {
                var lineLength = line.InnerText.Length + 1;
                if (lineLength < 70)
                {
                    lineCount++;
                    continue;
                }
                lineCount += Math.Ceiling((double)lineLength / 70);
            }
            var pageCount = Convert.ToInt32(Math.Ceiling(lineCount / 31));
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
            output = $"Typical time to read: {span.Hours} hours and {span.Minutes} minutes ({bookInfo.pagesInBook} pages)";
            return output;
        }

        public static List<string> GetMetaData(string mobiFile, string outDir, string randomFile, string mobiUnpack)
        {
            if (mobiUnpack == null) throw new ArgumentNullException(nameof(mobiUnpack));
            List<string> output = new List<string>();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = mobiUnpack,
                Arguments = $"-r -d \"{mobiFile}\" \"{randomFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };
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
                throw new Exception(String.Format("An error occurred while running Kindleunpack: {0}\r\n", ex.Message));
            }
            var rawMl = Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
            //Was the unpack successful?
            if (!unpackInfo.Contains("Write opf\r\n") && !unpackInfo.Contains("\r\nCompleted"))
            {
                throw new Exception("Kindleunpack returned: " + unpackInfo + "\r\nAn error occurred during unpack. See above info for details.\r\n");
            }
            //Attempt to find the .rawml unpacked from the mobi
            rawMl = randomFile + @"/mobi8/" + rawMl;
            if (!File.Exists(rawMl))
                rawMl = randomFile + @"/mobi7/" + Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
            if (!File.Exists(rawMl))
            {
                throw new Exception("Error finding .rawml file. Path: " + rawMl);
            }

            string uniqid = "";
            string asin = "";
            string author = "";
            string title = "";
            string image = "";

            DirectoryInfo d = new DirectoryInfo(randomFile + @"/mobi7/Images");
            if (d.Exists)
                image = d.GetFiles("*.jpeg").FirstOrDefault(file => file.Name.Contains("cover"))?.FullName ?? "";

            Match match = Regex.Match(unpackInfo, @"ASIN\s*(.*)");
            if (match.Success && match.Groups.Count > 1)
            {
                var incorrectAsin = match.Groups[1].Value.Replace("\r", "");
                //Improve actual Amazon ASIN matching
                match = Regex.Match(match.Groups[1].Value, "(^B[A-Z0-9]{9})");
                if (!match.Success)
                {
                    if (DialogResult.No == MessageBox.Show($"Incorrect ASIN detected: {incorrectAsin}!\n" +
                                      "Kindle may not display an X-Ray for this book.\n" +
                                      "Do you wish to continue?", "Incorrect ASIN", MessageBoxButtons.YesNo))
                    {
                        throw new Exception($"Incorrect ASIN detected: {incorrectAsin}!\r\n" +
                                          "Kindle may not display an X-Ray for this book.\r\n" +
                                          "You must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) " +
                                          "or a Mobi editor (exth 113 and 504) to change this.");
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
                    throw new Exception("The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\n"
                        + "You must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a Mobi editor (exth 501) to change this.");
                }
            }
            // Find author name in Kindleunpack output
            match = Regex.Match(unpackInfo, @" Creator\s{2,}(.*)");
            if (match.Success && match.Groups.Count > 1)
                author = match.Groups[1].Value.Replace("\r", "");

            // Find book title in Kindleunpack output
            match = Regex.Match(unpackInfo, @"Title in header at offset.*: '(.*)'");
            if (!match.Success || match.Groups.Count <= 1)
                match = Regex.Match(unpackInfo, @" Updated_Title\s*(.*)");
            if (match.Success && match.Groups.Count > 1)
                title = match.Groups[1].Value.Replace("\r", "");

            //Attempt to get database name from the mobi file.
            //If mobi_unpack ran successfully, then hopefully this will always be valid?
            byte[] dbinput = new byte[32];

            string databaseName;
            using (FileStream stream = File.Open(mobiFile, FileMode.Open, FileAccess.Read))
            {
                if (stream.Read(dbinput, 0, 32) != 32)
                    throw new Exception("Error reading from mobi file.");

                databaseName = Encoding.Default.GetString(dbinput).Trim('\0');
            }

            if (databaseName == "" || uniqid == "" || asin == "")
                throw new Exception($"Error: Missing metadata.\r\nDatabase Name: {databaseName}\r\nASIN: {asin}\r\nUniqueID: {uniqid}");

            if (!Properties.Settings.Default.useNewVersion && databaseName.Length == 31)
            {
                MessageBox.Show(
                    $"WARNING: Database Name is the maximum length. If \"{databaseName}\" is the full book title, this should not be an issue.\r\n" +
                    "If the title is supposed to be longer than that, you may get an error WG on your Kindle.\r\n" +
                    "This can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n");
            }

            output.Add(asin);
            output.Add(uniqid);
            output.Add(databaseName);
            output.Add(rawMl);
            output.Add(author);
            output.Add(title);
            output.Add(image);
            return output;
        }

        public static void RunNotepad(string filename)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "notepad",
                Arguments = filename,
                UseShellExecute = false
            };
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error trying to launch notepad: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        //http://stackoverflow.com/questions/4123590/serialize-an-object-to-xml
        public static string Serialize<T>(T value)
        {
            if (value == null)
                return string.Empty;

            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                return stringWriter.ToString();
            }
        }

        public static void Save<T>(T output, string fileName)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(typeof(T));
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
                var serializer = new XmlSerializer(typeof(List<T>));
                TextReader reader = new StreamReader(filePath, Encoding.UTF8);
                try
                {
                    itemList = (List<T>)serializer.Deserialize(reader);
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
            if (author.Contains(';'))
                author = author.Split(';')[0];
            if (author.Contains(','))
            {
                string[] parts = author.Split(',');
                author = parts[1].Trim() + " " + parts[0].Trim();
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
            if (bytesToCheck == null) return null;
            byte[] buffer = (byte[])bytesToCheck.Clone();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return buffer;
        }

        public static bool CleanUp(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return false;

            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] dirs = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                CleanUp(dir);
            }
            Thread.Sleep(1);
            Directory.Delete(folderPath, false);
            return true;
        }

        /// <summary>
        /// Process GUID. If in decimal form, convert to hex.
        /// </summary>
        public static string ConvertGuid(string guid)
        {
            if (Regex.IsMatch(guid, "/[a-zA-Z]/", RegexOptions.Compiled))
                guid = guid.ToUpper();
            else
            {
                long.TryParse(guid, out var guidDec);
                guid = guidDec.ToString("X");
            }

            if (guid == "0")
                throw new ArgumentException("An error occurred while converting the GUID.");

            return guid;
        }

        public static void SetPropertyThreadSafe(this Control ctrl, string name, object value)
        {
            if (ctrl.InvokeRequired)
                ctrl.BeginInvoke(new Action(() => SetPropertyThreadSafe(ctrl, name, value)));
            else
                ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, ctrl, new [] { value });
        }

        public static object GetPropertyTS(this Control ctrl, string name)
        {
            return ctrl.InvokeRequired
                ? ctrl.Invoke(new Func<object>(() => ctrl.GetPropertyTS(name)))
                : ctrl.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, ctrl, null);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }

    public static partial class ExtensionMethods
    {
        public static void AddNotNull<T>(this IList<T> list, T value)
        {
            if (value != null) list.Add(value);
        }

        public static void AddNotNull<T>(this ConcurrentBag<T> list, T value)
        {
            if (value != null) list.Add(value);
        }
    }
}