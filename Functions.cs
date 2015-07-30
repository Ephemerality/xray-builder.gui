using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using HAP = HtmlAgilityPack;

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

        public static string GetPageCount(string shelfariBookUrl)
        {
            HAP.HtmlDocument shelfariHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            shelfariHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(shelfariBookUrl));
            HAP.HtmlNode pageNode = shelfariHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_FirstEdition']");
            if (pageNode == null)
                return "";
            HAP.HtmlNode node = pageNode.SelectSingleNode(".//div/div");
            if (node == null)
                return "";
            Match match = Regex.Match(node.InnerText, @"Page Count: (\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "";
        }

        public static string GetQuotes(string shelfariBookUrl)
        {
            HAP.HtmlDocument shelfariHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            shelfariHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(shelfariBookUrl));
            HAP.HtmlNodeCollection quoteNodes = shelfariHtmlDoc.DocumentNode.SelectNodes("//div[@id='WikiModule_Quotations']/div/ul[@class='li_6']/li");
            int highlights = 0;
            string passages = "";
            if (quoteNodes != null)
            {
                foreach (HAP.HtmlNode quoteNode in quoteNodes)
                {
                    HAP.HtmlNode node = quoteNode.SelectSingleNode(".//blockquote");
                    if (node == null) continue;
                    passages = quoteNodes.Count.ToString();
                    node = quoteNode.SelectSingleNode(".//cite");
                    if (node == null) continue;
                       Match match = Regex.Match(node.InnerText, @"Highlighted by (\d+) Kindle customers");
                    if (match.Success)
                        highlights += int.Parse(match.Groups[1].Value);
                }
                return String.Format(@"{0} passages have been highlighted {1} times", passages, highlights); ;
            }
            return "No highlighted passages were found for this book";
        }

        public static string CleanString(this string s)
        {
            StringBuilder sb = new StringBuilder(s);

            Regex.Replace(sb.ToString(), @"\t|\n|\r|\s+", String.Empty);
            Regex.Replace(sb.ToString(), "\"", "'");
            sb.Replace(@"&quot;", "'");
            sb.Replace(@"<br>", String.Empty);
            sb.Replace(@"&#133;", "…");
            sb.Replace(@"&amp;#133;", "…");
            sb.Replace(@"&#169;", String.Empty);
            sb.Replace(@"&amp;#169;", String.Empty);
            sb.Replace(@"&#174;", String.Empty);
            sb.Replace(@"&amp;#174;", String.Empty);

            return sb.ToString().Trim();
        }


        /*public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }*/

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

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
                imageBytes.Length);

            // Convert byte[] to Image , based on @Crulex comment, the below line has no need since MemoryStream already initialized
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
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
            char[] fileChars = Path.GetInvalidFileNameChars();
            newAuthor = new string(author.Where(x => !fileChars.Contains(x)).ToArray());
            newTitle = new string(title.Where(x => !fileChars.Contains(x)).ToArray());
            path = Path.Combine(Properties.Settings.Default.outDir,
                String.Format(@"{0}\{1}", newAuthor, newTitle));
            if (!author.Equals(newAuthor) || !title.Equals(newTitle))
                MessageBox.Show("The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.", "Invalid Characters");
            Directory.CreateDirectory(path);
            return path;
        }

        public static bool ExtrasExist(string location, string asin)
        {
            {
                if (File.Exists(location + string.Format(@"\AuthorProfile.profile.{0}.asc", asin)) &&
                    File.Exists(location + string.Format(@"\EndActions.data.{0}.asc", asin)))
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

        public static string AppVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return "X-Ray Builder GUI v" + version.ToString();
        }

        public static string TimeStamp()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var time = string.Format("{0:H:mm:ss}", DateTime.Now);
            var date = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
            return string.Format("Running X-Ray Builder GUI v{0}. Log started on {1} at {2}.\r\n",
                version, date, time);
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
                    process.BeginErrorReadLine();
                    using (StreamReader reader = process.StandardOutput)
                    {
                        unpackInfo = reader.ReadToEnd();
                    }
                    process.Close();
                }
            }
            catch (Exception ex)
            {
                output.Add(String.Format("An error occurred while running Kindleunpack: {0} | {1}\r\n", ex.Message,
                    ex.Data));
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
                if (match.Groups[1].Value.Contains("-"))
                {
                    if (DialogResult.No == MessageBox.Show(String.Format("Incorrect ASIN detected: {0}!\n" +
                                      "Kindle may not display an X-Ray for this book.\n" +
                                      "Do you wish to continue?", incorrectAsin), "Incorrect ASIN", MessageBoxButtons.YesNo))
                    {
                        incorrectAsin = match.Groups[1].Value.Replace("\r", "");
                        output.Add(
                            String.Format("Incorrect ASIN detected: {0}!\r\n" +
                                          "Kindle may not display an X-Ray for this book.\r\n" +
                                          "You must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) " +
                                          "or a Mobi editor (exth 113 and 504) to change this.", incorrectAsin));
                        return output;
                    }
                }
                asin = match.Groups[1].Value.Replace("\r", "");
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
            FileStream stream = File.Open(mobiFile, FileMode.Open, FileAccess.Read);
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

            if (databaseName == "" || uniqid == "" || asin == "")
            {
                output.Add(String.Format(
                    "Error: Missing metadata.\r\nDatabase Name: {0}\r\nASIN: {1}\r\nUniqueID: {2}", databaseName, asin,
                    uniqid));
                MessageBox.Show("Missing metadata. See output log for details.", "Metadata Error");
                return output;
            }
            else if (databaseName.Length == 31)
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

            // Add author name to MetaData output
            output.Add(author);
            // Add book title to MetaData output
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

        public static BookInfo AmazonSearchBook(string title, string author)
        {
            BookInfo result = null;
            string searchUrl = @"http://www.amazon.com/s/?url=search-alias%3Ddigital-text&field-keywords=" + 
                Uri.EscapeDataString(title + " " + author);
            HAP.HtmlDocument searchDoc = new HAP.HtmlDocument();
            searchDoc.LoadHtml(HttpDownloader.GetPageHtml(searchUrl));
            HAP.HtmlNode node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_0']");
            //At least attempt to verify it might be the same book?
            if (node != null && node.InnerText.Contains(title))
            {
                string foundASIN = node.GetAttributeValue("data-asin", "");
                node = node.SelectSingleNode(".//div/div/div/div[@class='a-fixed-left-grid-col a-col-right']/div/a");
                if (node != null)
                {
                    result = new BookInfo(node.InnerText, author, foundASIN);
                    result.amazonUrl = node.GetAttributeValue("href", ""); // Grab the true link for good measure
                }
            }

            return result;
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
    }
}