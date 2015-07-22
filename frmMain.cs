using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public partial class frmMain : Form
    {
        public bool CheckInternet = false;
        public bool Exiting = false;
        public bool CheckTimestamp = false;

        private string currentLog = Environment.CurrentDirectory + @"\log\" +
                                    string.Format("{0:yyyy.MM.dd.hh.mm.ss}.txt", DateTime.Now);

        private Properties.Settings settings = Properties.Settings.Default;

        public frmMain()
        {
            InitializeComponent();
        }

        private FrmPreviewAp frmAP = new FrmPreviewAp();
        private FrmPreviewEa frmEA = new FrmPreviewEa();
        private frmPreviewXR frmXR = new frmPreviewXR();

        public void Log(string message)
        {
            if (Exiting) return;
            CheckTimestamp = txtOutput.Text.StartsWith("Running X-Ray Builder GUI");
            if (!CheckTimestamp)
            {
                txtOutput.AppendText(Functions.TimeStamp());
                CheckTimestamp = true;
                txtOutput.AppendText(message + "\r\n");
            }
            else
            {
                txtOutput.AppendText(message + "\r\n");
            }
        }

        private bool previewClear()
        {
            frmAP.lblTitle.Text = "";
            frmAP.pbAuthorImage.Image = Properties.Resources.AI;
            frmAP.lblBio1.Text = "";
            frmAP.lblBio2.Text = "";
            frmAP.lblKindleBooks.Text = "";
            for (var i = 0; i < 4; i++)
            {
                foreach (Control contrl in frmAP.Controls)
                {
                    if (contrl.Name == ("lblBook" + (i + 1)))
                    {
                        contrl.Text = "";
                    }
                }
            }
            frmEA.lblPost.Text = "";
            frmEA.lblMoreBooks.Text = "";
            for (var i = 0; i < 5; i++)
            {
                foreach (Control contrl in frmEA.Controls)
                {
                    if (contrl.Name == ("lblBook" + (i + 1)))
                    {
                        contrl.Text = "";
                    }
                }
            }
            frmEA.lblBook6.Text = "";
            frmEA.lblAuthor1.Text = "";
            frmEA.lblBook7.Text = "";
            frmEA.lblAuthor2.Text = "";
            return true;
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = Functions.GetBook(txtMobi.Text);
            //if (!_previewClear)
            //{
            //    //Log("Clearing previews...");
            //    frmAP.lblTitle.Text = "";
            //    frmAP.pbAuthorImage.Image = null;
            //    frmAP.lblBio1.Text = "";
            //    frmAP.lblBio2.Text = "";
            //    frmAP.lblKindleBooks.Text = "";
            //    for (var i = 0; i < 4; i++)
            //    {
            //        foreach (Control contrl in frmAP.Controls)
            //        {
            //            if (contrl.Name == ("lblBook" + (i + 1)))
            //            {
            //                contrl.Text = "";
            //            }
            //        }
            //    }
            //    frmEA.lblPost.Text = "";
            //    frmEA.lblMoreBooks.Text = "";
            //    for (var i = 0; i < 5; i++)
            //    {
            //        foreach (Control contrl in frmEA.Controls)
            //        {
            //            if (contrl.Name == ("lblBook" + (i + 1)))
            //            {
            //                contrl.Text = "";
            //            }
            //        }
            //    }
            //    frmEA.lblBook6.Text = "";
            //    frmEA.lblAuthor1.Text = "";
            //    frmEA.lblBook7.Text = "";
            //    frmEA.lblAuthor2.Text = "";
            //    //Log("Previews updated!");
            //    _previewClear = true;
            //}
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist. Please review the settings page.",
                    @"Output Directory Not found");
                return;
            }
            else
                Process.Start(settings.outDir);
            //if (!cbAuthorProfile.Checked &&
            //    !cbEndAction.Checked &&
            //    !cbXray.Checked)
            //    if (MessageBox.Show("This book has not been processed yet.\r\nDo you want to open the Output directory?", "Unprocessed book", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //        Process.Start(settings.outDir);
            //    else
            //        return;
            //Process.Start(settings.outDir);
        }

        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = Functions.GetFile(txtXMLFile.Text,
                "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt");
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(@"Specified book was not found.", @"Book Not Found");
                return;
            }
            if (rdoShelfari.Checked && txtShelfari.Text == "")
            {
                MessageBox.Show(@"No Shelfari link was specified.", @"Missing Shelfari Link");
                return;
            }
            if (!File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show(@"Kindleunpack was not found.\r\nPlease review the settings page.",
                    @"Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist.\r\nPlease review the settings page.",
                    @"Output Directory Not found");
                return;
            }
            if (Properties.Settings.Default.realName.Trim().Length == 0 |
                Properties.Settings.Default.penName.Trim().Length == 0)
            {
                MessageBox.Show(
                    @"Both Real and Pen names are required for End Action\r\n" +
                    @"file creation. This information allows you to rate this\r\n" +
                    @"book on Amazon. Please review the settings page.",
                    @"Amazon Customer Details Not found");
                return;
            }
            if (Properties.Settings.Default.sendtoKindle &&
                Properties.Settings.Default.docDir.Trim().Length == 0)
            {
                MessageBox.Show(
                    @"Automatically send to Kindle is enabled but no Kindle Documents\r\nDirectory has been specified. Please review the settings page.",
                    @"Kindle Documents Directory Not found");
                return;
            }
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show(@"Temporary path not accessible for some reason.", @"Temporary Directory Error");
                return;
            }

            //this.Width = 559;
            //btnPreview.Enabled = false;
            //cmsPreview.Items[0].Enabled = false;
            //cmsPreview.Items[1].Enabled = false;
            //cmsPreview.Items[2].Enabled = false;
            //txtOutput.Clear();
            //txtOutput.Refresh();
            prgBar.Value = 0;

            Log("Running Kindleunpack to get metadata...");

            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            //this.TopMost = true;
            List<string> results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            if (settings.saverawml)
            {
                Log("Saving rawML to output directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp",
                    Path.GetFileName(results[3])), true);
            }

            // Added author name to log output
            Log(
                String.Format(
                    "Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                    results[2], results[0], results[4], results[5], results[1]));

            Log(String.Format("Attempting to build X-Ray...\r\nSpoilers: {0}",
                settings.spoilers ? "Enabled" : "Disabled"));
            Log("Offset: " + settings.offset.ToString());

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay ss;
            try
            {
                if (rdoShelfari.Checked)
                    ss = new XRay(txtShelfari.Text, results[2], results[1], results[0], this, settings.spoilers,
                        settings.offset, "", false);
                else
                    ss = new XRay(txtXMLFile.Text, results[2], results[1], results[0], this, settings.spoilers,
                        settings.offset, "");
                if (ss.CreateXray() > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if (ss.ExpandFromRawMl(results[3], settings.ignoresofthyphen, !settings.useNewVersion) > 0)
                {
                    Log("Error while processing locations and chapters.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\n" +
                    ex.Message);
                return;
            }

            Log("Saving X-Ray to file...");
            string outFolder = "";
            string _newPath = "";
            try
            {
                outFolder = settings.useSubDirectories ? Functions.GetBookOutputDirectory(results[4], Path.GetFileNameWithoutExtension(txtMobi.Text)) : settings.outDir;
            }
            catch (Exception ex)
            {
                Log("Failed to create output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outFolder = settings.outDir;
            }
            _newPath = outFolder + "\\" + ss.GetXRayName(settings.android);

            if (settings.useNewVersion)
            {
                try
                {
                    SQLiteConnection.CreateFile(_newPath);
                }
                catch (Exception ex)
                {
                    Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\n" +
                        ex.Message);
                    return;
                }
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection(("Data Source=" + _newPath + ";Version=3;"));
                m_dbConnection.Open();
                string sql;
                try
                {
                    using (StreamReader streamReader = new StreamReader("BaseDB.sql", Encoding.UTF8))
                    {
                        sql = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Log(
                        "An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\n" +
                        ex.Message);
                    m_dbConnection.Close();
                    return;
                }
                SQLiteCommand command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", m_dbConnection);
                Log("\nBuilding new X-Ray database. May take a few minutes...");
                command.ExecuteNonQuery();
                command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", m_dbConnection);
                command.ExecuteNonQuery();
                Console.WriteLine("Done building initial database. Populating with info from source X-Ray...");
                try
                {
                    ss.PopulateDb(m_dbConnection);
                }
                catch (Exception ex)
                {
                    Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\n" +
                        ex.Message);
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
                using (
                    StreamWriter streamWriter = new StreamWriter(_newPath, false,
                        settings.utf8 ? Encoding.UTF8 : Encoding.Default))
                {
                    streamWriter.Write(ss.ToString());
                }
            }
            Log("X-Ray file created successfully!\r\nSaved to " + _newPath);

            for (var i = 0; i < 8; i++)
            {
                foreach (Control contrl in frmXR.Controls)
                {
                    if (contrl.Name == ("lblTerm" + (i + 1)))
                    {
                        contrl.Visible = true;
                        contrl.Text = "Term " + i + " ...Waiting...";
                    }
                    if (contrl.Name == ("pbTerm" + (i + 1)))
                    {
                        contrl.Visible = true;
                    }
                }
            }
            frmXR.lblBookTitle.Text = results[5];
            frmXR.lblXrayTermsAll.Text = String.Format("All {0}", ss.Terms.Count);
            frmXR.lblXrayTermsRest.Text = String.Format("|  People {0}  |  Terms {1}",
                ss.Terms.Count(t => t.Type.Equals("character")),
                ss.Terms.Count(t => t.Type.Equals("topic")));
            if (ss.Terms.Count != 0)
            {
                var numberOfLabels = 0;
                if (ss.Terms.Count > 8)
                    numberOfLabels = 8;
                else
                    numberOfLabels = ss.Terms.Count;

                for (var i = 0; i < numberOfLabels; i++)
                {
                    foreach (Control contrl in frmXR.Controls)
                    {
                        if (contrl.Name == ("lblTerm" + (i + 1)))
                        {
                            contrl.Text = ss.Terms[i].TermName;
                        }
                        if (contrl.Name == ("pbTerm" + (i + 1)))
                        {
                            contrl.Visible = true;
                        }
                    }
                }
                if (ss.Terms.Count < 8)
                {
                    for (var i = ss.Terms.Count + 1; i < 9; i++)
                    {
                        foreach (Control contrl in frmXR.Controls)
                        {
                            if (contrl.Name == ("lblTerm" + (i)))
                            {
                                contrl.Visible = false;
                            }
                            if (contrl.Name == ("pbTerm" + (i)))
                            {
                                contrl.Visible = false;
                            }
                        }
                    }
                }
            }
            btnPreview.Enabled = true;
            cmsPreview.Items[2].Enabled = true;

            try
            {
                Directory.Delete(randomFile, true);
            }
            catch (Exception)
            {
                Log("An error occurred while trying to delete temporary files.\r\nTry deleting these files manually.");
            }

            //this.TopMost = false;
        }

        private void btnKindleExtras_Click(object sender, EventArgs e)
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
            if (Properties.Settings.Default.realName.Trim().Length == 0 |
                Properties.Settings.Default.penName.Trim().Length == 0)
            {
                MessageBox.Show(
                    "Both Real and Pen names are required for End Action\r\n" +
                    "file creation. This information allows you to rate this\r\n" +
                    "book on Amazon. Please review the settings page.",
                    "Amazon Customer Details Not found");
                return;
            }
            if (Properties.Settings.Default.sendtoKindle &&
                Properties.Settings.Default.docDir.Trim().Length == 0)
            {
                MessageBox.Show(
                    "Automatically send to Kindle is enabled but no Kindle Documents\r\nDirectory has been specified. Please review the settings page.",
                    "Kindle Documents Directory Not found");
                return;
            }
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show("Temporary path not accessible for some reason.", "Temporary Directory Error");
                return;
            }

            //this.Width = 559;
            //btnPreview.Enabled = false;
            //txtOutput.Clear();
            //txtOutput.Refresh();

            Log("Running Kindleunpack to get metadata...");

            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            //this.TopMost = true;
            List<string> results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            if (settings.saverawml)
            {
                Log("Saving rawML to output directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp",
                    Path.GetFileName(results[3])), true);
            }

            // Added author name to log output
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                            results[2], results[0], results[4], results[5], results[1]));
            try
            {
                BookInfo bookInfo = new BookInfo(results[5], results[4], results[0], results[1], results[2],
                                                randomFile, Path.GetFileNameWithoutExtension(txtMobi.Text));
                AuthorProfile ap = new AuthorProfile(bookInfo, this);
                if (!ap.complete) return;
                EndActions ea = new EndActions(ap, bookInfo, this);
                if (!ea.complete) return;
                if (settings.useNewVersion)
                    ea.GenerateNew();
                else
                    ea.GenerateOld();
                previewClear();

                frmAP.lblTitle.Text = ap.ApTitle;
                frmAP.pbAuthorImage.Image = ap.ApAuthorImage;

                var g = Graphics.FromHwnd(frmAP.lblBio1.Handle);
                int charFitted, linesFitted;
                g.MeasureString(ap.BioTrimmed, frmAP.lblBio1.Font, frmAP.lblBio1.Size,
                    StringFormat.GenericTypographic, out charFitted, out linesFitted);

                if (ap.BioTrimmed != "")
                {
                    if (ap.BioTrimmed.Length > charFitted)
                    {
                        string bio1Trim = ap.BioTrimmed.Substring(0, Math.Min(ap.BioTrimmed.Length, charFitted - 10));
                        frmAP.lblBio1.Text = bio1Trim.Substring(0, bio1Trim.LastIndexOf(" "));
                        frmAP.lblBio2.Text = ap.BioTrimmed.Substring(bio1Trim.LastIndexOf(" ") + 1);
                    }
                    else
                    {
                        frmAP.lblBio1.Text = ap.BioTrimmed;
                    }
                }

                frmAP.lblKindleBooks.Text = ap.ApSubTitle;
                for (var i = 0; i < Math.Min(ap.otherBooks.Count, 4); i++)
                {
                    foreach (Control contrl in frmAP.Controls)
                    {
                        if (contrl.Name == ("lblBook" + (i + 1)))
                            contrl.Text = ap.otherBooks[i].title;
                    }
                }
                frmEA.lblPost.Text = String.Format("Post on Amazon (as {0})",
                    Properties.Settings.Default.penName);
                frmEA.lblMoreBooks.Text = ap.EaSubTitle;
                for (var i = 0; i < Math.Min(ap.otherBooks.Count, 5); i++)
                {
                    foreach (Control contrl in frmEA.Controls)
                    {
                        if (contrl.Name == ("lblBook" + (i + 1)))
                            contrl.Text = ap.otherBooks[i].title;
}
                }
                if (ea.custAlsoBought.Count > 1)
                {
                    frmEA.lblBook6.Text = ea.custAlsoBought[0].title;
                    frmEA.lblAuthor1.Text = ea.custAlsoBought[0].author;
                    frmEA.lblBook7.Text = ea.custAlsoBought[1].title;
                    frmEA.lblAuthor2.Text = ea.custAlsoBought[1].author;
                }
            }
            catch (Exception ex)
            {
                Log("An error occurred while creating the new Author Profile and/or End Action files: " + ex.Message);
            }
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            if (txtShelfari.Text.Trim().Length == 0)
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
            else
                Process.Start(txtShelfari.Text);
        }

        private void btnSaveShelfari_Click(object sender, EventArgs e)
        {
            if (txtShelfari.Text == "")
            {
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
                return;
            }
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
            string path = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(txtMobi.Text) +
                          ".xml";
            try
            {
                txtXMLFile.Text = path;

                //txtOutput.Clear();
                //txtOutput.Refresh();

                XRay xray = new XRay(txtShelfari.Text, this, settings.spoilers);
                if (xray.SaveXml(path) > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Shelfari info has been saved to: " + path);
            }
            catch (Exception)
            {
                Log("Error while saving Shelfari data to XML. Path was: " + path);
                return;
            }
        }

        private void btnSearchShelfari_Click(object sender, EventArgs e)
        {
            //Check internet connection
            //Log("Checking internet connection...");
            //if (Functions.CheckForInternetConnection() != true)
            //    Log("No Internet connection detected!");
            //else
            //    Log("Internet connection ok!");

            //Check current settings
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
            //txtOutput.Clear();
            //txtOutput.Refresh();
            Log("Running Kindleunpack to get metadata...");

            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            //this.TopMost = true;
            List<string> results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            if (settings.saverawml)
            {
                Log("Saving rawML to output directory...");
                File.Copy(results[3], Path.Combine(settings.outDir, Path.GetFileName(results[3])), true);
            }
            // Added author name to log output
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                results[2], results[0], results[4], results[5], results[1]));

            Directory.Delete(randomFile, true);

            //Get Shelfari Search URL
            Log("Searching for book on Shelfari...");
            string shelfariSearchUrlBase = @"http://www.shelfari.com/search/books?Author={0}&Title={1}&Binding={2}";
            string[] bindingTypes = {"Hardcover", "Kindle", "Paperback"};

            // Search book on Shelfari
            bool bookFound = false;
            string shelfariBookUrl = "";
            try
            {
                HtmlAgilityPack.HtmlDocument shelfariHtmlDoc = new HtmlAgilityPack.HtmlDocument();
                for (int j = 0; j <= 1; j++)
                {
                    for (int i = 0; i < bindingTypes.Length; i++)
                    {
                        Log("Searching for " + bindingTypes[i] + " edition...");
                        // Insert parameters (mainly for searching with removed diacritics). Seems to work fine without replacing spaces?
                        shelfariHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(string.Format(shelfariSearchUrlBase, results[4], results[5], bindingTypes[i])));
                        if (!shelfariHtmlDoc.DocumentNode.InnerText.Contains("Your search did not return any results"))
                        {
                            shelfariBookUrl = FindShelfariURL(shelfariHtmlDoc, results[4], results[5]);
                            if (shelfariBookUrl != "")
                            {
                                bookFound = true;
                                break;
                            }
                        }
                        if (!bookFound)
                        {
                            Log("Unable to find a " + bindingTypes[i] + " edition of this book on Shelfari!");
                        }
                    }
                    if (bookFound) break;
                    // Attempt to remove diacritics (accented characters) from author & title for searching
                    string newAuthor = results[4].RemoveDiacritics();
                    string newTitle = results[5].RemoveDiacritics();
                    if (!results[4].Equals(newAuthor) || !results[5].Equals(newTitle))
                    {
                        results[4] = newAuthor;
                        results[5] = newTitle;
                        Log("Accented characters detected. Attempting to search without them.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
                return;
            }

            if (bookFound)
            {
                Log("Book found on Shelfari!");
                Log(results[5] + " by " + results[4]);

                //Find series and next book
                FindSeriesInfo(shelfariBookUrl);

                txtShelfari.Text = shelfariBookUrl;
                txtShelfari.Refresh();
                Log("Shelfari URL updated!\r\nYou may want to visit the URL to ensure it is correct and add/modify terms if necessary.");
            }
            else
            {
                Log("Unable to find this book on Shelfari! You may have to search manually.");
            }
        }

        private string FindShelfariURL(HtmlAgilityPack.HtmlDocument shelfariHtmlDoc, string author, string title)
        {
            // Try to find book's page from Shelfari search
            string shelfariBookUrl = "";
            int index = 0;
            List<string> listofthings = new List<string>();
            List<string> listoflinks = new List<string>();
            Dictionary<string, string> retData = new Dictionary<string, string>();

            foreach (HtmlAgilityPack.HtmlNode bookItems in shelfariHtmlDoc.DocumentNode.SelectNodes("//li[@class='item']/div[@class='text']"))
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
                    // Search for author with spaces removed to avoid situations like "J.R.R. Tolkien" / "J. R. R. Tolkien"
                    // May cause false matches, we'll see.
                    // Also remove diacritics from titles when matching just in case...
                    // Searching for Children of Húrin will give a false match on the first pass before diacritics are removed from the search URL
                    if ((listofthings.Contains("(Author)") || listofthings.Contains("(Author),")) &&
                        line.RemoveDiacritics().StartsWith(title.RemoveDiacritics(), StringComparison.OrdinalIgnoreCase) &&
                        (listofthings.Contains(author) || listofthings.Exists(r => r.Replace(" ", "") == author.Replace(" ", ""))))
                    {
                        shelfariBookUrl = listoflinks[index].ToString();
                        shelfariBookUrl = Regex.Replace(shelfariBookUrl, "<a href=\"", "", RegexOptions.None);
                        shelfariBookUrl = Regex.Replace(shelfariBookUrl, "\">.*?</a>", "", RegexOptions.None);
                        return shelfariBookUrl;
                    }
                    index++;
                }
            }
            return "";
        }

        private string FindSeriesInfo(string searchUrl)
        {
            bool hasSeries = false;
            string series = "";
            string seriesShort = "";
            string seriesURL = "";
            int currentSeriesIndex = 0;
            int currentSeriesCount = 0;
            string nextTitle = "";
            //Check if book's Shelfari page contains series info
            HtmlAgilityPack.HtmlDocument searchHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            searchHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(string.Format(searchUrl)));
            HtmlAgilityPack.HtmlNode node = searchHtmlDoc.DocumentNode.SelectSingleNode("//span[@class='series']");
            if (node != null)
            {
                //Series name and book number
                series = node.InnerText.Trim();
                //Convert book number string to integer
                Int32.TryParse(series.Substring(series.LastIndexOf(" ") + 1), out currentSeriesIndex);
                //Parse series Shelfari URL
                seriesURL = node.SelectSingleNode("//span[@class='series']/a[@href]")
                    .GetAttributeValue("href", "");
                seriesShort = node.FirstChild.InnerText.Trim();
                //Add series name and book number to log, if found
                searchHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(string.Format(seriesURL)));
                //Parse number of books in series and convert to integer
                node = searchHtmlDoc.DocumentNode.SelectSingleNode("//h2[@class='f_m']");
                string test = node.FirstChild.InnerText.Trim();
                Match match = Regex.Match(test, @"\d+");
                if (match.Success)
                    Int32.TryParse(match.Value, out currentSeriesCount);
                hasSeries = true;
                //Check if there is a next book
                if (currentSeriesIndex < currentSeriesCount)
                {
                    //Add series name and book number to log, if found
                    Log(String.Format("This is book {0} of {1} in the {2} Series...",
                        currentSeriesIndex, currentSeriesCount, seriesShort));
                    foreach (HtmlAgilityPack.HtmlNode seriesItem in
                        searchHtmlDoc.DocumentNode.SelectNodes(".//ol/li"))
                    {
                        node = seriesItem.SelectSingleNode(".//div/span[@class='series bold']");
                        if (node != null)
                            if (node.InnerText.Contains((currentSeriesIndex + 1).ToString()))
                            {
                                node = seriesItem.SelectSingleNode(".//h3/a");
                                //Parse title of the next book
                                nextTitle = node.InnerText.Trim();
                                //Add next book in series to log, if found
                                Log(String.Format("The next book in this series is {0}!", nextTitle));
                                return "";
                            }
                    }
                }
                if (hasSeries)
                    Log(String.Format("Unable to find the next book in this series!\r\n" +
                                      "This is the last title in the {0} series...", seriesShort));
                return "";
            }
            return "";
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmSettings frmSet = new frmSettings();
            frmSet.ShowDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.mobiFile = txtMobi.Text;
            Properties.Settings.Default.xmlFile = txtXMLFile.Text;
            Properties.Settings.Default.shelfari = txtShelfari.Text;
            if (rdoShelfari.Checked)
                Properties.Settings.Default.buildSource = "Shelfari";
            else
                Properties.Settings.Default.buildSource = "XML";
            Properties.Settings.Default.Save();
            if (txtOutput.Text.Trim().Length != 0)
                File.WriteAllText(currentLog, txtOutput.Text);
            Exiting = true;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Width = 559;
            //pnlAuthorProfile.Visible = false;
            this.ActiveControl = lblShelfari;
            ToolTip toolTip1 = new ToolTip();
            //toolTip1.IsBalloon = true;
            toolTip1.SetToolTip(btnBrowseMobi, "Open a Kindle book.");
            toolTip1.SetToolTip(btnBrowseOutput, "Open the default output directory.");
            toolTip1.SetToolTip(btnLink, "Open the Shelfari link in your default web browser.");
            toolTip1.SetToolTip(btnBrowseXML, "Open a supported alias file containg Characters and Topics.");
            toolTip1.SetToolTip(btnSearchShelfari, "Try to search for this book on Shelfari.");
            toolTip1.SetToolTip(btnSaveShelfari, "Save Shelfari info to an XML file.");
            toolTip1.SetToolTip(btnKindleExtras,
                "Try to build the Author Profile and\r\nEnd Action files for this book.");
            toolTip1.SetToolTip(btnBuild,
                "Try to build the X-Ray file for this book.");
            toolTip1.SetToolTip(btnSettings, "Configure X-Ray Builder GUI.");
            toolTip1.SetToolTip(btnPreview, "View a preview of the generated files.");
            this.DragEnter += frmMain_DragEnter;
            this.DragDrop += frmMain_DragDrop;

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

            if (txtMobi.Text == "") txtMobi.Text = Properties.Settings.Default.mobiFile;

            if (txtXMLFile.Text == "")
            {
                txtXMLFile.Text = Properties.Settings.Default.xmlFile;
            }
            if (!Directory.Exists(Environment.CurrentDirectory + @"\out"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\out");
            if (Properties.Settings.Default.outDir == "")
            {
                Properties.Settings.Default.outDir = Environment.CurrentDirectory + @"\out";
            }
            if (!Directory.Exists(Environment.CurrentDirectory + @"\log"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\log");
            if (!Directory.Exists(Environment.CurrentDirectory + @"\dmp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\dmp");
            if (Properties.Settings.Default.mobi_unpack == "")
            {
                Properties.Settings.Default.mobi_unpack = Environment.CurrentDirectory +
                                                          @"\dist\kindleunpack.exe";
            }
            txtShelfari.Text = Properties.Settings.Default.shelfari;
            if (Properties.Settings.Default.buildSource == "Shelfari")
                rdoShelfari.Checked = true;
            else
                rdoFile.Checked = true;
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[]) (e.Data.GetData(DataFormats.FileDrop));
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

        private void rdoSource_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton) sender).Text == "Shelfari")
            {
                lblShelfari.Visible = !lblShelfari.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                lblXMLFile.Visible = !lblXMLFile.Visible;
                txtXMLFile.Visible = !txtXMLFile.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                btnBrowseXML.Visible = !btnBrowseXML.Visible;
                btnSaveShelfari.Enabled = !btnSaveShelfari.Enabled;
                btnLink.Visible = !btnLink.Visible;
            }
        }

        private void txtMobi_TextChanged(object sender, EventArgs e)
        {
            txtShelfari.Text = "";
            //this.Width = 559;
            btnPreview.Enabled = false;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (cmsPreview.Visible)
            {
                cmsPreview.Hide();
            }
            else
            {
                cmsPreview.Show(btnPreview, new Point(2, btnPreview.Height));
            }
        }

        private void tmiAuthorProfile_Click(object sender, EventArgs e)
        {
            frmAP.ShowDialog();
        }

        private void tmiEndAction_Click(object sender, EventArgs e)
        {
            frmEA.ShowDialog();
        }

        private void tmiXray_Click(object sender, EventArgs e)
        {
            frmXR.ShowDialog();
        }
    }
}