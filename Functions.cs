using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace XRayBuilderGUI
{
    public static class Functions
    {
        public static string getDir(string defaultFolder)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.SelectedPath = defaultFolder;
            if (f.ShowDialog() == DialogResult.OK)
                return f.SelectedPath;
            else
                return defaultFolder;
        }

        public static string getFile(string defaultFile, string filter = "All files (*.*)|*.*")
        {
            OpenFileDialog f = new OpenFileDialog();
            if (defaultFile != "") f.InitialDirectory = Path.GetDirectoryName(defaultFile);
            f.Filter = filter;
            //ff.FilterIndex = 2;
            f.RestoreDirectory = true;
            if (f.ShowDialog() == DialogResult.OK)
                return f.FileName;
            else
                return defaultFile;
        }
        public static string GetTempDirectory()
        {
            string path;
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }

        public static List<string> GetMetaData (string mobiFile, string outDir, string randomFile, string mobi_unpack)
        {
            List<string> output = new List<string>();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = mobi_unpack;
            startInfo.Arguments = "-r -d \"" + mobiFile + @""" """ + randomFile + @"""";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            string rawML = "";
            string unpackInfo = "";
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.BeginErrorReadLine();
                    using (StreamReader reader1 = process.StandardOutput)
                    {
                        unpackInfo = reader1.ReadToEnd();
                    }
                    process.Close();
                }
            }
            catch (Exception ex)
            {
                output.Add(String.Format("An error occurred while running Kindleunpack: {0} | {1}\r\n", ex.Message, ex.Data));
                MessageBox.Show("Error while running Kindleunpack. See the output log for details.");
                return output;
            }
            rawML = Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
            //Was the unpack successful?
            if (!unpackInfo.Contains("Write opf\r\n") && !unpackInfo.Contains("\r\nCompleted"))
            {
                output.Add("Kindleunpack returned: " + unpackInfo + "\r\nAn error occurred during unpack. See above info for details.\r\n");
                return output;
            }
            //Attempt to find the .rawml unpacked from the mobi
            rawML = randomFile + @"/mobi8/" + rawML;
            if (!File.Exists(rawML))
                rawML = randomFile + @"/mobi7/" + Path.GetFileNameWithoutExtension(mobiFile) + ".rawml";
            if (!File.Exists(rawML))
            {
                output.Add("Error finding .rawml file. Path: " + rawML);
                MessageBox.Show("Error finding .rawml.", "RAWML Error");
                return output;
            }
            /*Log("RawML found at " + rawML + ".");
            if (settings.saverawml)
            {
                Log("Saving rawML to output directory.");
                File.Copy(rawML, Path.Combine(settings.outDir, Path.GetFileName(rawML)), true);
            }
            Log("Grabbing metadata...");*/
            string databaseName = "";
            string uniqid = "";
            string asin = "";
            Match match = Regex.Match(unpackInfo, @"ASIN\s*(.*)");
            if (match.Success && match.Groups.Count > 1)
                asin = match.Groups[1].Value.Replace("\r", "");
            match = Regex.Match(unpackInfo, @"(\d*) unique_id");
            if (match.Success && match.Groups.Count > 1)
                uniqid = match.Groups[1].Value;
            match = Regex.Match(unpackInfo, @"Document Type\s*(\w*)");
            if (match.Success && match.Groups.Count > 1)
            {
                if (match.Groups[1].Value != "EBOK")
                {
                    output.Add("The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\nYou must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a Mobi editor (exth 501) to change this.");
                    MessageBox.Show("The document type is not set to EBOK; Kindle will not display an X-Ray for this book.\r\nYou must either use Calibre's convert feature (Personal Doc tag under MOBI Output) or a Mobi editor (exth 501) to change this.");
                    return output;
                }
            }
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
                output.Add(String.Format("Error: Missing metadata.\r\nDatabase Name: {0}\r\nASIN: {1}\r\nUniqueID: {2}", databaseName, asin, uniqid));
                MessageBox.Show("Missing metadata. See output log for details.", "Metadata Error");
                return output;
            }
            else if (databaseName.Length == 31)
            {
                MessageBox.Show(String.Format("WARNING: Database Name is the maximum length. If \"{0}\" is the full book title, this should not be an issue.\r\n" +
                    "If the title is supposed to be longer than that, you may get an error WG on your Kindle.\r\n" +
                    "This can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n", databaseName));
            }
            output.Add(asin);
            output.Add(uniqid);
            output.Add(databaseName);
            output.Add(rawML);
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
                var xmlserializer = new XmlSerializer(typeof(T));
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

        public static void Save<T>(T output, string FileName)
        {
            using (var writer = new System.IO.StreamWriter(FileName, false, Encoding.UTF8))
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
                    MessageBox.Show("Error processing XML file: " + ex.Message + "\r\nIf the error contains a (#, #), the first number is the line the error occurred on.", "XML Error");
                    return null;
                }
                reader.Close();
            }

            return itemList;
        }
    }


    // Taken from http://stackoverflow.com/a/2700707
    // Downloads an HTML page using appropriate encoding
    public class HttpDownloader
    {
        private readonly string _referer;
        private readonly string _userAgent;
        private readonly CookieContainer _cookiejar = new CookieContainer();
        private bool encodingFoundInHeader = false;

        public Encoding Encoding { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public Uri Url { get; set; }

        public HttpDownloader(string url, CookieContainer jar, string referer, string userAgent)
        {
            Encoding = Encoding.GetEncoding("ISO-8859-1");
            Url = new Uri(url); // verify the uri
            _userAgent = userAgent;
            _referer = referer;
            _cookiejar = jar;
        }

        public string GetPage()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (!string.IsNullOrEmpty(_referer))
                request.Referer = _referer;
            if (!string.IsNullOrEmpty(_userAgent))
                request.UserAgent = _userAgent;

            request.CookieContainer = _cookiejar;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Headers = response.Headers;
                Url = response.ResponseUri;
                return ProcessContent(response);
            }

        }

        private string ProcessContent(HttpWebResponse response)
        {
            SetEncodingFromHeader(response);

            Stream s = response.GetResponseStream();
            if (response.ContentEncoding.ToLower().Contains("gzip"))
                s = new GZipStream(s, CompressionMode.Decompress);
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
                s = new DeflateStream(s, CompressionMode.Decompress);

            MemoryStream memStream = new MemoryStream();
            int bytesRead;
            byte[] buffer = new byte[0x1000];
            for (bytesRead = s.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = s.Read(buffer, 0, buffer.Length))
            {
                memStream.Write(buffer, 0, bytesRead);
            }
            s.Close();
            string html;
            memStream.Position = 0;
            using (StreamReader r = new StreamReader(memStream, Encoding))
            {
                html = r.ReadToEnd().Trim();
                if (!encodingFoundInHeader)
                    html = CheckMetaCharSetAndReEncode(memStream, html);
            }

            return html;
        }

        private void SetEncodingFromHeader(HttpWebResponse response)
        {
            string charset = null;
            if (string.IsNullOrEmpty(response.CharacterSet))
            {
                Match m = Regex.Match(response.ContentType, @";\s*charset\s*=\s*(?<charset>.*)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    charset = m.Groups["charset"].Value.Trim(new[] { '\'', '"' });
                    encodingFoundInHeader = true;
                }
            }
            else
            {
                charset = response.CharacterSet;
            }
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    Encoding = Encoding.GetEncoding(charset);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
        {
            Match m = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);
            if (m.Success)
            {
                string charset = m.Groups["charset"].Value.ToLower() ?? "iso-8859-1";
                if ((charset == "unicode") || (charset == "utf-16"))
                {
                    charset = "utf-8";
                }

                try
                {
                    Encoding metaEncoding = Encoding.GetEncoding(charset);
                    if (Encoding != metaEncoding)
                    {
                        memStream.Position = 0L;
                        StreamReader recodeReader = new StreamReader(memStream, metaEncoding);
                        html = recodeReader.ReadToEnd().Trim();
                        recodeReader.Close();
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return html;
        }
    }
}
