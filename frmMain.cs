﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
                                    String.Format("{0:dd.MM.yyyy.H.mm.ss}.txt", DateTime.Now);

        private Properties.Settings settings = Properties.Settings.Default;

        public frmMain()
        {
            InitializeComponent();
        }

        private FrmPreviewAp frmAP = new FrmPreviewAp();
        private FrmPreviewEa frmEA = new FrmPreviewEa();
        private frmPreviewXR frmXR = new frmPreviewXR();
        private frmPreviewXRN frmXRN = new frmPreviewXRN();
        private frmPreviewSA frmSA = new frmPreviewSA();

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
                if (message.ContainsIgnorecase("successfully"))
                {
                    txtOutput.SelectionStart = txtOutput.TextLength;
                    txtOutput.SelectionLength = 0;
                    txtOutput.SelectionColor = Color.FromArgb(20, 102, 20);
                }
                if (message.ContainsIgnorecase("error") || message.ContainsIgnorecase("failed") || message.ContainsIgnorecase("problem"))
                {
                    txtOutput.SelectionStart = txtOutput.TextLength;
                    txtOutput.SelectionLength = 0;
                    txtOutput.SelectionColor = Color.FromArgb(102, 20, 20);
                }
                txtOutput.AppendText(message + "\r\n");
                txtOutput.SelectionColor = txtOutput.ForeColor;
            }
            //txtOutput.Refresh();
        }

        private bool ClearPreviews()
        {
            frmAP.lblTitle.Text = "";
            frmAP.pbAuthorImage.Image = Properties.Resources.AI;
            frmAP.lblBio1.Text = "";
            frmAP.lblBio2.Text = "";
            frmAP.lblKindleBooks.Text = "";
            for (int i = 0; i < 4; i++)
            {
                frmAP.Controls["lblbook" + (i + 1)].Text = "";
            }
            frmEA.lblPost.Text = "";
            frmEA.lblMoreBooks.Text = "";
            for (int i = 0; i < 5; i++)
            {
                frmEA.Controls["lblbook" + (i + 1)].Text = "";
            }
            frmEA.lblBook6.Text = "";
            frmEA.lblAuthor1.Text = "";
            frmEA.lblBook7.Text = "";
            frmEA.lblAuthor2.Text = "";
            foreach (Control contrl in frmXRN.Controls)
            {
                contrl.Visible = false;
            }
            return true;
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = Functions.GetBook(txtMobi.Text);
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist. Please review the settings page.", @"Output Directory Not found");
                return;
            }
            else
                Process.Start(settings.outDir);
        }

        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = Functions.GetFile(txtXMLFile.Text, "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt");
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
            if (settings.useKindleUnpack && !File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show(@"Kindleunpack was not found.\r\nPlease review the settings page.", @"Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist.\r\nPlease review the settings page.", @"Output Directory Not found");
                return;
            }
            if (Properties.Settings.Default.realName.Trim().Length == 0
                || Properties.Settings.Default.penName.Trim().Length == 0)
            {
                MessageBox.Show(
                    @"Both Real and Pen names are required for End Action\r\n" +
                    @"file creation. This information allows you to rate this\r\n" +
                    @"book on Amazon. Please review the settings page.",
                    @"Amazon Customer Details Not found");
                return;
            }
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show(@"Temporary path not accessible for some reason.", @"Temporary Directory Error");
                return;
            }

            prgBar.Value = 0;
            
            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            List<string> results;
            if (settings.useKindleUnpack)
            {
                Log("Running Kindleunpack to get metadata...");
                results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            }
            else
            {
                Log("Extracting metadata...");
                try
                {
                    results = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, true, randomFile).getResults();
                }
                catch (Exception ex)
                {
                    Log("Error getting metadata: " + ex.Message);
                    return;
                }
            }
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            if (settings.saverawml)
            {
                Log("Saving rawML to dmp directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp", Path.GetFileName(results[3])), true);
            }

            // Added author name to log output
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                results[2], results[0], results[4], results[5], results[1]));

            Log(String.Format("Attempting to build X-Ray...\r\nSpoilers: {0}", settings.spoilers ? "Enabled" : "Disabled"));

            //If AZW3 file use AZW3 offset, if checked. Checked by default.
            bool AZW3 = Path.GetExtension(txtMobi.Text) == ".azw3" && settings.overrideOffset;
            Log("Offset: " + (AZW3 ? settings.offsetAZW3.ToString() + " (AZW3)" : settings.offset.ToString()));

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            try
            {
                if (rdoShelfari.Checked)
                    xray = new XRay(txtShelfari.Text, results[2], results[1], results[0], this, settings.spoilers,
                        (AZW3 ? settings.offsetAZW3 : settings.offset), "", false);
                else
                    xray = new XRay(txtXMLFile.Text, results[2], results[1], results[0], this, settings.spoilers,
                        (AZW3 ? settings.offsetAZW3 : settings.offset), "");
                if (xray.CreateXray() > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if (xray.ExpandFromRawMl(results[3], settings.ignoresofthyphen, !settings.useNewVersion) > 0)
                {
                    Log("Error while processing locations and chapters.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log("An error occurred while creating the new X-Ray database.\r\n" + ex.Message);
                return;
            }

            Log("Saving X-Ray to file...");
            string outFolder = "";
            string _newPath = "";
            try
            {
                if (settings.android)
                {
                    outFolder = settings.outDir + @"\Android\" + results[0];
                    Directory.CreateDirectory(outFolder);
                }
                else
                {
                    outFolder = settings.useSubDirectories
                        ? Functions.GetBookOutputDirectory(results[4], Functions.RemoveInvalidFileChars(results[5]))
                        : settings.outDir;
                }
            }
            catch (Exception ex)
            {
                Log("Failed to create output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outFolder = settings.outDir;
            }
            _newPath = outFolder + "\\" + xray.GetXRayName(settings.android);

            if (settings.useNewVersion)
            {
                try
                {
                    SQLiteConnection.CreateFile(_newPath);
                }
                catch (Exception ex)
                {
                    Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\r\n" + ex.Message);
                    return;
                }
                using (SQLiteConnection m_dbConnection = new SQLiteConnection(("Data Source=" + _newPath + ";Version=3;")))
                {
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
                        Log("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\n"
                            + ex.Message);
                        m_dbConnection.Dispose();
                        return;
                    }
                    SQLiteCommand command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", m_dbConnection);
                    Log("Building new X-Ray database. May take a few minutes...");
                    command.ExecuteNonQuery();
                    command.Dispose();
                    command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", m_dbConnection);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    Log("Done building initial database. Populating with info from source X-Ray...");
                    try
                    {
                        xray.PopulateDb(m_dbConnection);
                    }
                    catch (Exception ex)
                    {
                        Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\r\n" + ex.Message);
                        command.Dispose();
                        m_dbConnection.Close();
                        return;
                    }
                    Log("Updating indices...");
                    sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                          + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                          + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC); COMMIT;";
                    command = new SQLiteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    m_dbConnection.Close();
                }
                
                //Save the new XRAY.ASIN.previewData file
                try
                {
                    string PdPath = outFolder + @"\XRAY." + results[0] + ".previewData";
                    using (StreamWriter streamWriter = new StreamWriter(PdPath, false,
                        settings.utf8 ? Encoding.UTF8 : Encoding.Default))
                    {
                        streamWriter.Write(xray.getPreviewData());
                    }
                    Log("X-Ray previewData file created successfully!\r\nSaved to " + PdPath);
                }
                catch (Exception ex)
                {
                    Log(String.Format("An error occurred saving the previewData file: {0}", ex.Message));
                }
            }
            else
            {
                using (StreamWriter streamWriter = new StreamWriter(_newPath, false, settings.utf8 ? Encoding.UTF8 : Encoding.Default))
                {
                    streamWriter.Write(xray.ToString());
                }
            }

            Log("X-Ray file created successfully!\r\nSaved to " + _newPath);

            if (Properties.Settings.Default.playSound)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
                player.Play();
            }

            try
            {
                PopulateXRayPreviews(results[5], xray);
                btnPreview.Enabled = true;
                cmsPreview.Items[2].Enabled = true;
            }
            catch (Exception ex)
            {
                Log("Error populating X-Ray preview windows: " + ex.Message);
            }

            try
            {
                Directory.Delete(randomFile, true);
            }
            catch (Exception)
            {
                Log("An error occurred while trying to delete temporary files.\r\nTry deleting these files manually.");
            }
        }

        private void PopulateXRayPreviews(string bookTitle, XRay xray)
        {
            //Old X-ray Preview
            for (int i = 0; i < 8; i++)
            {
                frmXR.Controls["lblTerm" + (i + 1)].Visible = true;
                frmXR.Controls["lblTerm" + (i + 1)].Text = "Term " + i + " ...Waiting...";
                frmXR.Controls["pbTerm" + (i + 1)].Visible = true;
            }
            frmXR.lblBookTitle.Text = bookTitle;
            frmXR.lblXrayTermsAll.Text = String.Format("All {0}", xray.Terms.Count);
            frmXR.lblXrayTermsRest.Text = String.Format("|  People {0}  |  Terms {1}",
                xray.Terms.Count(t => t.Type.Equals("character")),
                xray.Terms.Count(t => t.Type.Equals("topic")));

            if (xray.Terms.Count != 0)
            {
                int numLabels = Math.Min(8, xray.Terms.Count);
                for (int i = 0; i < numLabels; i++)
                {
                    frmXR.Controls["lblTerm" + (i + 1)].Text = xray.Terms[i].TermName;
                    frmXR.Controls["pbTerm" + (i + 1)].Visible = true;
                }
                if (xray.Terms.Count < 8)
                {
                    for (int i = xray.Terms.Count + 1; i < 9; i++)
                    {
                        frmXR.Controls["lblTerm" + i].Visible = false;
                        frmXR.Controls["pbTerm" + i].Visible = false;
                    }
                }
            }

            //New X-ray Preview
            for (int i = 1; i <= 4; i++)
            {
                frmXRN.Controls["lblTermName" + i].Visible = true;
                frmXRN.Controls["lblTermMentions" + i].Visible = true;
                frmXRN.Controls["lblTermDescription" + i].Visible = true;
            }
            frmXRN.lblTitle.Text = "X-Ray — " + bookTitle;
            if (xray.Terms.Count != 0)
            {
                int numLabels = Math.Min(4, xray.Terms.Count);
                for (int i = 0; i < numLabels; i++)
                {
                    frmXRN.Controls["lblTermName" + (i + 1)].Text = xray.Terms[i].TermName;
                    frmXRN.Controls["lblTermMentions" + (i + 1)].Text = xray.Terms[i].Locs.Count + " Mentions";
                    frmXRN.Controls["lblTermDescription" + (i + 1)].Text = xray.Terms[i].Desc;
                }
                if (xray.Terms.Count < 4)
                {
                    for (int i = xray.Terms.Count + 1; i < 5; i++)
                    {
                        frmXRN.Controls["lblTermName" + i].Visible = false;
                        frmXRN.Controls["lblTermMentions" + i].Visible = false;
                        frmXRN.Controls["lblTermDescription" + i].Visible = false;
                    }
                }
            }
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
            
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show("Temporary path not accessible for some reason.", "Temporary Directory Error");
                return;
            }

            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            List<string> results;
            long rawMLSize = 0;
            if (settings.useKindleUnpack)
            {
                Log("Running Kindleunpack to get metadata...");
                results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
                if (!File.Exists(results[3]))
                {
                    Log("Error: RawML could not be found, aborting.\r\nPath: " + results[3]);
                    return;
                }
                rawMLSize = new FileInfo(results[3]).Length;
            }
            else
            {
                Log("Extracting metadata...");
                try
                {
                    Unpack.Metadata md = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, false);
                    rawMLSize = md.PDH.TextLength;
                    results = md.getResults();

                }
                catch (Exception ex)
                {
                    Log("Error getting metadata: " + ex.Message);
                    return;
                }
            }
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            if (settings.saverawml && settings.useKindleUnpack)
            {
                Log("Saving rawML to dmp directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp",
                    Path.GetFileName(results[3])), true);
            }

            // Added author name to log output
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                results[2], results[0], results[4], results[5], results[1]));
            try
            {
                BookInfo bookInfo = new BookInfo(results[5], results[4], results[0], results[1], results[2],
                                                randomFile, Functions.RemoveInvalidFileChars(results[5]), txtShelfari.Text);
                Log("Attempting to build Author Profile...");
                AuthorProfile ap = new AuthorProfile(bookInfo, this);
                if (!ap.complete) return;
                Log("Attempting to build Start Actions and End Actions...");
                EndActions ea = new EndActions(ap, bookInfo, rawMLSize, this);
                if (!ea.complete) return;
                if (settings.useNewVersion)
                {
                    ea.GenerateNew();
                    Log("Attempting to build Start Actions...");
                    ea.GenerateStartActions();
                }
                else
                    ea.GenerateOld();

                if (Properties.Settings.Default.playSound)
                {
                    System.Media.SoundPlayer player =
                        new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
                    player.Play();
                }

                try
                {
                    PopulateAPEAPreviews(ap, ea);
                }
                catch (Exception ex)
                {
                    Log("Error populating extras preview windows: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Log("An error occurred while creating the new Author Profile, Start Actions, and/or End Actions files: " + ex.Message);
            }

        }

        private void PopulateAPEAPreviews(AuthorProfile ap, EndActions ea)
        {
            ClearPreviews();

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
                    frmAP.lblBio1.Text = ap.BioTrimmed;
            }

            frmAP.lblKindleBooks.Text = ap.ApSubTitle;
            int numLabels = Math.Min(4, ap.otherBooks.Count);
            for (int i = 0; i < numLabels; i++)
            {
                frmAP.Controls["lblBook" + (i + 1)].Text = ap.otherBooks[i].title;
            }
            frmEA.lblPost.Text = String.Format("Post on Amazon (as {0}) and Goodreads",
                Properties.Settings.Default.penName);
            frmEA.lblMoreBooks.Text = ap.EaSubTitle;
            numLabels = Math.Min(5, ap.otherBooks.Count);
            for (int i = 0; i < numLabels; i++)
            {
                frmEA.Controls["lblBook" + (i + 1)].Text = ap.otherBooks[i].title;
            }
            if (ea.custAlsoBought.Count > 1)
            {
                frmEA.lblBook6.Text = ea.custAlsoBought[0].title;
                frmEA.lblAuthor1.Text = ea.custAlsoBought[0].author;
                frmEA.lblBook7.Text = ea.custAlsoBought[1].title;
                frmEA.lblAuthor2.Text = ea.custAlsoBought[1].author;
            }

            // StartActions preview
            if (ea.curBook.seriesName == "")
            {
                frmSA.lblBookTitle.Text = ea.curBook.title;
            }
            else
            {
                frmSA.lblBookTitle.Text = string.Format("{0} ({1} Book {2})", ea.curBook.title, ea.curBook.seriesName,
                    ea.curBook.seriesPosition);
            }
            frmSA.lblBookAuthor.Text = ea.curBook.author;
            //Convert rating to equivalent Star image
            string starNum = string.Format("STAR{0}",
                Math.Floor(ea.curBook.amazonRating).ToString());
            //Return an object from the star image in the project,
            //set the Image property of pbRating to the returned object as Image
            frmSA.pbRating.Image = (Image)Properties.Resources.ResourceManager.GetObject(starNum);
            frmSA.lblBookDesc.Text = ea.curBook.desc;
            frmSA.lblRead.Text = string.Format("{0} hours and {1} minutes", ea.curBook.readingHours, ea.curBook.readingMinutes);
            frmSA.lblPages.Text = string.Format("{0} pages", ea.curBook.pagesInBook);
            if (ea.curBook.seriesPosition != "")
                frmSA.lblSeries.Text = string.Format("This is book {0} of {1} in {2}",
                    ea.curBook.seriesPosition, ea.curBook.totalInSeries, ea.curBook.seriesName);
            else
                frmSA.lblSeries.Text = "This book is not part of a series.";
            frmSA.pbAuthorImage.Image = ap.ApAuthorImage;
            frmSA.lblAboutAuthor.Text = ea.curBook.author;
            frmSA.lblAuthorBio.Text = ap.BioTrimmed;
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
            string path = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(txtMobi.Text) + ".xml";
            try
            {
                txtXMLFile.Text = path;

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

            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
            //this.TopMost = true;
            List<string> results;
            if (settings.useKindleUnpack)
            {
                Log("Running Kindleunpack to get metadata...");
                results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            }
            else
            {
                Log("Extracting metadata...");
                try
                {
                    results = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, false).getResults();
                }
                catch (Exception ex)
                {
                    Log("Error getting metadata: " + ex.Message);
                    return;
                }
            }
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }

            // Added author name to log output
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nAuthor: {2}\r\nTitle: {3}\r\nUniqueID: {4}",
                results[2], results[0], results[4], results[5], results[1]));
            
            //Get Shelfari Search URL
            Log("Searching for book on Shelfari...");
            string shelfariSearchUrlBase = @"http://www.shelfari.com/search/books?Author={0}&Title={1}&Binding={2}";
            string[] bindingTypes = {"Hardcover", "Kindle", "Paperback"};

            // Search book on Shelfari
            bool bookFound = false;
            string shelfariBookUrl = "";
            results[4] = Functions.FixAuthor(results[4]);

            try
            {
                HtmlAgilityPack.HtmlDocument shelfariHtmlDoc = new HtmlAgilityPack.HtmlDocument();
                for (int j = 0; j <= 1; j++)
                {
                    for (int i = 0; i < bindingTypes.Length; i++)
                    {
                        Log("Searching for " + bindingTypes[i] + " edition...");
                        // Insert parameters (mainly for searching with removed diacritics). Seems to work fine without replacing spaces?
                        shelfariHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(String.Format(shelfariSearchUrlBase, results[4], results[5], bindingTypes[i])));
                        if (!shelfariHtmlDoc.DocumentNode.InnerText.Contains("Your search did not return any results"))
                        {
                            shelfariBookUrl = FindShelfariURL(shelfariHtmlDoc, results[4], results[5]);
                            if (shelfariBookUrl != "")
                            {
                                bookFound = true;
                                if (Properties.Settings.Default.saveHtml)
                                {
                                    try
                                    {
                                        Log("Saving book's Shelfari webpage...");
                                        shelfariHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(shelfariBookUrl));
                                        File.WriteAllText(Environment.CurrentDirectory +
                                                          String.Format(@"\dmp\{0}.shelfaripageHtml.txt", results[0]),
                                            shelfariHtmlDoc.DocumentNode.InnerHtml);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log(String.Format("An error ocurred saving shelfaripageHtml.txt: {0}", ex.Message));
                                    }
                                }
                                break;
                            }
                        }
                        if (!bookFound)
                            Log("Unable to find a " + bindingTypes[i] + " edition of this book on Shelfari!");
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

                txtShelfari.Text = shelfariBookUrl;
                txtShelfari.Refresh();
                Log(String.Format("Shelfari URL updated: {0}\r\nYou may want to visit the URL to ensure it is correct and add/modify terms if necessary.", shelfariBookUrl));
            }
            else
                Log("Unable to find this book on Shelfari! You may have to search manually.");

            try
            {
                Directory.Delete(randomFile, true);
            }
            catch (Exception)
            {
                Log("An error occurred while trying to delete temporary files.\r\nTry deleting these files manually.");
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
                    if (bookItems.ChildNodes[i].GetAttributeValue("class", "") == "series") continue;
                    listofthings.Add(bookItems.ChildNodes[i].InnerText.Trim());
                    listoflinks.Add(bookItems.ChildNodes[i].InnerHtml);
                }
                index = 0;
                foreach (string line in listofthings)
                {
                    // Search for author with spaces removed to avoid situations like "J.R.R. Tolkien" / "J. R. R. Tolkien"
                    // Ignore Collective Work search result.
                    // May cause false matches, we'll see.
                    // Also remove diacritics from titles when matching just in case...
                    // Searching for Children of Húrin will give a false match on the first pass before diacritics are removed from the search URL
                    if ((listofthings.Contains("(Author)") || listofthings.Contains("(Author),")) &&
                        line.RemoveDiacritics().StartsWith(title.RemoveDiacritics(), StringComparison.OrdinalIgnoreCase) &&
                        (listofthings.Contains(author) || listofthings.Exists(r => r.Replace(" ", "") == author.Replace(" ", ""))))
                        if (!listoflinks.Any(c => c.Contains("(collective work)")))
                        {
                            shelfariBookUrl = listoflinks[index].ToString();
                            shelfariBookUrl = Regex.Replace(shelfariBookUrl, "<a href=\"", "", RegexOptions.None);
                            shelfariBookUrl = Regex.Replace(shelfariBookUrl, "\".*?</a>.*", "", RegexOptions.None);
                            if (shelfariBookUrl.ToLower().StartsWith("http://"))
                                return shelfariBookUrl;
                        }
                    index++;
                }
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
                File.WriteAllText(currentLog, txtOutput.Text.ToString());
            Exiting = true;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ActiveControl = lblShelfari;
            ToolTip toolTip1 = new ToolTip();
            toolTip1.SetToolTip(btnBrowseMobi, "Open a Kindle book.");
            toolTip1.SetToolTip(btnBrowseOutput, "Open the default output directory.");
            toolTip1.SetToolTip(btnLink, "Open the Shelfari link in your default web browser.");
            toolTip1.SetToolTip(btnBrowseXML, "Open a supported XML or TXT file containing characters and topics.");
            toolTip1.SetToolTip(btnSearchShelfari, "Try to search for this book on Shelfari.");
            toolTip1.SetToolTip(btnSaveShelfari, "Save Shelfari info to an XML file.");
            toolTip1.SetToolTip(btnKindleExtras,
                "Try to build the Start Action, Author Profile,\r\nand End Action files for this book.");
            toolTip1.SetToolTip(btnBuild,
                "Try to build the X-Ray file for this book.");
            toolTip1.SetToolTip(btnSettings, "Configure X-Ray Builder GUI.");
            toolTip1.SetToolTip(btnPreview, "View a preview of the generated files.");
            toolTip1.SetToolTip(btnUnpack, "Save the rawML (raw markup) of the book\r\nin the output directory so you can review it.");
            this.DragEnter += frmMain_DragEnter;
            this.DragDrop += frmMain_DragDrop;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (File.Exists(args[i]))
                        txtMobi.Text = Path.GetFullPath(args[i]);
                }
            }

            if (txtMobi.Text == "") txtMobi.Text = Properties.Settings.Default.mobiFile;

            if (txtXMLFile.Text == "") txtXMLFile.Text = Properties.Settings.Default.xmlFile;
            if (!Directory.Exists(Environment.CurrentDirectory + @"\out"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\out");
            if (Properties.Settings.Default.outDir == "")
                Properties.Settings.Default.outDir = Environment.CurrentDirectory + @"\out";
            if (!Directory.Exists(Environment.CurrentDirectory + @"\log"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\log");
            if (!Directory.Exists(Environment.CurrentDirectory + @"\dmp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\dmp");
            
            if (Properties.Settings.Default.mobi_unpack == "")
                Properties.Settings.Default.mobi_unpack = Environment.CurrentDirectory + @"\dist\kindleunpack.exe";

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
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void rdoSource_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Text == "Shelfari")
            {
                lblShelfari.Visible = !lblShelfari.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                lblXMLFile.Visible = !lblXMLFile.Visible;
                txtXMLFile.Visible = !txtXMLFile.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                btnBrowseXML.Visible = !btnBrowseXML.Visible;
                btnSaveShelfari.Enabled = !btnSaveShelfari.Enabled;
                btnSearchShelfari.Visible = !btnSearchShelfari.Visible;
            }
        }

        private void txtMobi_TextChanged(object sender, EventArgs e)
        {
            txtShelfari.Text = "";
            btnPreview.Enabled = false;
            prgBar.Value = 0;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (cmsPreview.Visible)
                cmsPreview.Hide();
            else
                cmsPreview.Show(btnPreview, new Point(2, btnPreview.Height));
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
            if (Properties.Settings.Default.useNewVersion)
                frmXRN.ShowDialog();
            else
                frmXR.ShowDialog();
        }

        private void tmiStartAction_Click(object sender, EventArgs e)
        {
            frmSA.ShowDialog();
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (!settings.newMessage)
            {
                MessageBox.Show("Metadata is now gathered internally rather than with KindleUnpack. " +
                    "If you run into any metadata extraction errors, there is a setting to turn KindleUnpack back on. " +
                    "Please report any such errors on the MobileRead thread to help improve the program.\r\n\r\n" +
                    "There is also a new feature that allows you to download pre-made aliases if they exist on our server. " +
                    "If the setting is checked, aliases will be downloaded automatically during the build process.\r\n\r\n" +
                    "- Thanks for using X-Ray Builder GUI!\r\n- Ephemerality and darrenmcg","New for X-Ray Builder GUI v2.0.10.0");
                settings.newMessage = true;
                settings.Save();
            }
        }

        private void btnUnpack_Click(object sender, EventArgs e)
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(@"Specified book was not found.", @"Book Not Found");
                return;
            }
            if (settings.useKindleUnpack && !File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show(@"Kindleunpack was not found.\r\nPlease review the settings page.", @"Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist.\r\nPlease review the settings page.", @"Output Directory Not found");
                return;
            }
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show(@"Temporary path not accessible for some reason.", @"Temporary Directory Error");
                return;
            }
            List<string> results;
            if (settings.useKindleUnpack)
            {
                Log("Running Kindleunpack to extract rawML...");
                results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            }
            else
            {
                Log("Extracting rawML...");
                try
                {
                    results = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, true, randomFile).getResults();
                }
                catch (Exception ex)
                {
                    Log("Error getting rawML: " + ex.Message);
                    return;
                }
            }
            if (results.Count != 6)
            {
                Log(results[0]);
                return;
            }
            string rawmlPath = Path.Combine(Environment.CurrentDirectory + @"\dmp", Path.GetFileName(results[3]));
            File.Copy(results[3], rawmlPath, true);
            Log("Extracted rawml successfully!\r\nSaved to " + rawmlPath);
        }

        private void txtOutput_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}