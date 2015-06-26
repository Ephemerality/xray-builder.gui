using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using ExceptionReporting;
using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    public partial class frmMain : Form
    {
        private Properties.Settings settings = XRayBuilderGUI.Properties.Settings.Default;
        public bool exiting = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                foreach (string fileLoc in filePaths)
                {
                    if (File.Exists(fileLoc))
                    {
                        txtMobi.Text = fileLoc;
                        return;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToolTip ToolTip1 = new ToolTip();
            ToolTip1.SetToolTip(btnBrowseMobi, "Open a Kindle book.");
            ToolTip1.SetToolTip(btnBrowseOutput, "Open the default output directory.");
            ToolTip1.SetToolTip(btnLink, "Open the Shelfari link in your default web browser.");
            ToolTip1.SetToolTip(btnBrowseXML, "Open a supported alias file containg Characters and Topics.");
            ToolTip1.SetToolTip(btnSearchShelfari, "Try to search for this book on Shelfari.");
            ToolTip1.SetToolTip(btnShelfari, "Save Shelfari info to an XML file.");
            ToolTip1.SetToolTip(btnKindleExtras, "Try to build the Author Profile and End Action files for this book.");

            this.DragEnter += frmMain_DragEnter;
            this.DragDrop += frmMain_DragDrop;
            Version dd = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string xrayversion = dd.Major.ToString() + "." + dd.Minor.ToString() + dd.Build.ToString();
            this.Text = "X-Ray Builder GUI v" + xrayversion;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    
                    if (File.Exists(args[i]))
                    {
                        txtMobi.Text = Path.GetFullPath(args[i]);
                    }
                }
            }
            if (txtMobi.Text == "") txtMobi.Text = XRayBuilderGUI.Properties.Settings.Default.mobiFile;
            if (txtXMLFile.Text == "") txtXMLFile.Text = XRayBuilderGUI.Properties.Settings.Default.xmlFile;
            if (XRayBuilderGUI.Properties.Settings.Default.outDir == "")
            {
                XRayBuilderGUI.Properties.Settings.Default.outDir = Environment.CurrentDirectory + "/out";
                if (!Directory.Exists(XRayBuilderGUI.Properties.Settings.Default.outDir)) Directory.CreateDirectory(XRayBuilderGUI.Properties.Settings.Default.outDir);
            }
            if (XRayBuilderGUI.Properties.Settings.Default.mobi_unpack == "")
            {
                XRayBuilderGUI.Properties.Settings.Default.mobi_unpack = Environment.CurrentDirectory + "/dist/kindleunpack.exe";
            }
            txtShelfari.Text = XRayBuilderGUI.Properties.Settings.Default.shelfari;
            if (XRayBuilderGUI.Properties.Settings.Default.buildSource == "Shelfari")
                rdoShelfari.Checked = true;
            else
                rdoFile.Checked = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            XRayBuilderGUI.Properties.Settings.Default.mobiFile = txtMobi.Text;
            XRayBuilderGUI.Properties.Settings.Default.xmlFile = txtXMLFile.Text;
            XRayBuilderGUI.Properties.Settings.Default.shelfari = txtShelfari.Text;
            if (rdoShelfari.Checked)
                XRayBuilderGUI.Properties.Settings.Default.buildSource = "Shelfari";
            else
                XRayBuilderGUI.Properties.Settings.Default.buildSource = "XML";
            XRayBuilderGUI.Properties.Settings.Default.Save();
            exiting = true;
            Application.Exit();
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = Functions.getFile(txtMobi.Text);
        }
        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = Functions.getFile(txtXMLFile.Text, "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt|All files (*.*)|*.*");
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            Form frm = new frmSettings();
            frm.ShowDialog();
            this.TopMost = true;
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (rdoShelfari.Checked && txtShelfari.Text == "")
            {
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
                return;
            }
            if (!File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show("Kindleunpack was not found. Please review the settings page.", "Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show("Specified output directory does not exist. Please review the settings page.", "Output Directory Not found");
                return;
            }
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show("Temporary path not accessible for some reason.", "Temporary Directory Error");
                return;
            }
            Log("Running kindleunpack to get metadata...\r\n");
            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML
            this.TopMost = true;
            List<string> results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }
            if (settings.saverawml)
            {
                Log("Saving rawML to output directory.");
                File.Copy(results[3], Path.Combine(settings.outDir, Path.GetFileName(results[3])), true);
            }
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nUniqueID: {2}\r\nAttempting to build X-Ray...", results[2], results[0], results[1]));
            Log(String.Format("Spoilers: {0}", settings.spoilers ? "Enabled" : "Disabled"));
            Log("Offset: " + settings.offset.ToString());
            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay ss;
            try
            {
                if (rdoShelfari.Checked)
                    ss = new XRay(txtShelfari.Text, results[2], results[1], results[0], this, settings.spoilers, settings.offset, "", false);
                else
                    ss = new XRay(txtXMLFile.Text, results[2], results[1], results[0], this, settings.spoilers, settings.offset, "");
                if (ss.createXRAY() > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Initial X-Ray built, adding locs and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if (ss.expandFromRawML(results[3], settings.ignoresofthyphen, !settings.useNewVersion) > 0)
                {
                    Log("Error while processing locations and chapters.");
                    return;
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler(exception);
                return;
            }
            Log("Saving X-Ray to file...");
            string newPath = settings.outDir + "\\" + ss.getXRayName(settings.android);
            if (settings.useNewVersion)
            {
                try
                {
                    SQLiteConnection.CreateFile(newPath);
                } catch (Exception ex) {
                    Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\n" + ex.Message);
                    return;
                }
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection(("Data Source=" + newPath + ";Version=3;"));
                m_dbConnection.Open();
                string sql;
                try
                {
                    using (StreamReader streamReader = new StreamReader("BaseDB.sql", Encoding.UTF8))
                    {
                        sql = streamReader.ReadToEnd();
                    }
                } catch (Exception ex) {
                    Log("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\n" + ex.Message);
                    m_dbConnection.Close();
                    return;
                }
                SQLiteCommand command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", m_dbConnection);
                Log("\nBuilding new X-ray database. May take a few minutes...");
                command.ExecuteNonQuery();
                command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", m_dbConnection);
                command.ExecuteNonQuery();
                Console.WriteLine("Done building initial database. Populating with info from source X-Ray...");
                try
                {
                    ss.PopulateDB(m_dbConnection);
                } catch (Exception ex) {
                    ExceptionHandler(ex);
                    m_dbConnection.Close();
                    return;
                }
                Console.WriteLine("Updating indices...");
                sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                    + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                    + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC); COMMIT;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                m_dbConnection.Close();
            }
            else
            {
                using (StreamWriter streamWriter = new StreamWriter(newPath, false, settings.utf8 ? Encoding.UTF8 :  Encoding.Default))
                {
                    streamWriter.Write(ss.ToString());
                }
            }
            Log("XRay file created successfully!\r\nSaved to " + newPath);
            this.TopMost = false;
            try
            {
                Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                Log("Error deleting temp directory: " + ex.Message);
            }
            //frmXRay frm = new frmXRay(ss, results[3]);
            //frm.Show(this);
        }
        public void Log(string message)
        {
            if (exiting) return;
            txtOutput.AppendText(message + "\r\n");
        }

        private void btnShelfari_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (txtShelfari.Text == "")
            {
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
                return;
            }
            if (!Directory.Exists(Environment.CurrentDirectory + "/ext/"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "/ext/");
            string path = Environment.CurrentDirectory + "/ext/" + Path.GetFileNameWithoutExtension(txtMobi.Text) + ".xml";
            try
            {
                txtXMLFile.Text = path;
                XRay xray = new XRay(txtShelfari.Text, this, settings.spoilers);
                if (xray.saveXML(path) > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Shelfari info has been saved to: " + path);
            }
            catch (Exception exception)
            {
                Log("Error while saving Shelfari data to XML. Path was: " + path);
                ExceptionHandler(exception);
                return;
            }
        }

        private void rdoSource_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton source = (RadioButton)sender;
            if (source.Text == "Shelfari")
            {
                lblShelfari.Visible = !lblShelfari.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                lblXMLFile.Visible = !lblXMLFile.Visible;
                txtXMLFile.Visible = !txtXMLFile.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                btnBrowseXML.Visible = !btnBrowseXML.Visible;
                btnShelfari.Enabled = !btnShelfari.Enabled;
                btnLink.Visible = !btnLink.Visible;
            }
        }
        private void btnLink_Click(object sender, EventArgs e)
        {
            if (txtShelfari.Text.Trim().Length == 0)
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
            else
                Process.Start(txtShelfari.Text);
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show("Specified output directory does not exist. Please review the settings page.",
                    "Output Directory Not found");
                return;
            }
            else
                Process.Start(settings.outDir);
        }

        private void btnSearchShelfari_Click(object sender, EventArgs e)
        {
            bool bookFoundShelfari = false;

            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (!File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show("Kindleunpack was not found. Please review the settings page.", "Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show("Specified output directory does not exist. Please review the settings page.",
                    "Output Directory Not found");
                return;
            }

            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show("Temporary path not accessible for some reason.", "Temporary Directory Error");
                return;
            }

            Log("Running Kindleunpack to get metadata...");

            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            //this.TopMost = true;
            List<string> results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            try
            {
                Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                Log("Error deleting temp directory: " + ex.Message);
            }
            // Added author name to log output
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                    results[2], results[0], results[4], results[5], results[1]));

            //Get Shelfari Search URL
            var shelfariSearchUrl = @"http://www.shelfari.com/search/books?Author=" + results[4].Replace(" ", "%20") +
                                    "&Title=" + results[5].Replace(" ", "%20"); // +"&Binding=Hardcover";

            // Search book on Shelfari
            Log("Searching for book on Shelfari...");
            HtmlAgilityPack.HtmlDocument shelfariHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            CookieContainer jar = new CookieContainer();
            using (var webClient = new WebClientEx(jar))
            {
                try
                {
                    using (var stream = webClient.OpenRead(shelfariSearchUrl))
                    {
                        shelfariHtmlDoc.Load(stream);
                    }
                    webClient.Dispose();
                }
                catch (Exception ex)
                {
                    Log("Error searching Shelfari: " + ex.Message);
                    return;
                }
            }

            // Try to find book's page from Shelfari search
            string shelfariBookUrl = "";
            string bookSeries = "";
            int index = 0;
            List<string> listofthings = new List<string>();
            List<string> listoflinks = new List<string>();
            foreach (HtmlNode bookItems in shelfariHtmlDoc.DocumentNode.SelectNodes("//li[@class='item']/div[@class='text']"))
            {
                if (bookItems == null) continue;
                listofthings.Clear();
                listoflinks.Clear();
                for (var i = 1; i < bookItems.ChildNodes.Count; i++)
                {
                    listofthings.Add(bookItems.ChildNodes[i].InnerText.Trim());
                    listoflinks.Add(bookItems.ChildNodes[i].InnerHtml);
                }
                index = 0;
                foreach (string line in listofthings)
                {
                    if (listofthings.Contains("(Author)") &&
                        line.StartsWith(results[5], StringComparison.OrdinalIgnoreCase) &&
                        listofthings.Contains(results[4]))
                    {
                        shelfariBookUrl = listoflinks[index].ToString();
                        bookSeries = listofthings[0].ToString();
                        shelfariBookUrl = Regex.Replace(shelfariBookUrl, "<a href=\"", "", RegexOptions.None);
                        shelfariBookUrl = Regex.Replace(shelfariBookUrl, "\">.*", "", RegexOptions.None);
                        Log("Book found on Shelfari!");
                        Log(results[5] + " by " + results[4] + " (" + bookSeries + ")");
                        txtShelfari.Text = shelfariBookUrl;
                        txtShelfari.Refresh();
                        Log("Shelfari URL updated!");
                        bookFoundShelfari = true;
                        break;
                    }
                    index++;
                }
            }
            if (!bookFoundShelfari)
                Log("Unable to find this book on Shelfari!");
        }

        public void ExceptionHandler(Exception exception)
        {
            ExceptionReporter reporter = new ExceptionReporter();
            reporter.ReadConfig();
            reporter.Config.ShowSysInfoTab = false;
            reporter.Config.EmailReportAddress = "revensoftware+xraybuilder@gmail.com";
            reporter.Config.ShowAssembliesTab = false;
            reporter.Config.ShowConfigTab = false;

            reporter.Config.UserExplanationLabel = "No description required, but you can enter one if you like:";
            this.TopMost = false;
            reporter.Show(exception);
            this.TopMost = true;
            Log("Unhandled error occurred while processing this book, please report it.");
        }
    }
}
