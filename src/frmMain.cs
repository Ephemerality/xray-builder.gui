using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilderGUI.DataSources;
using XRayBuilderGUI.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XRayBuilderGUI
{
    public partial class frmMain : Form
    {
        public bool Exiting;

        private string currentLog = Environment.CurrentDirectory + @"\log\" +
                                    String.Format("{0:HH.mm.ss.dd.MM.yyyy}.txt", DateTime.Now);

        private string EaPath = "";
        private string SaPath = "";
        private string ApPath = "";
        private string XrPath = "";

        private Settings settings = Settings.Default;

        public frmMain()
        {
            InitializeComponent();
        }

        private frmAbout frmInfo = new frmAbout();
        private frmCreateXR frmCreator = new frmCreateXR();

        public List<string> openBook = new List<string>();

        ToolTip toolTip1 = new ToolTip();

        DataSource dataSource;

        CancellationTokenSource cancelTokens = new CancellationTokenSource();

        public void UpdateProgressBar(Tuple<int, int> vals)
        {
            Functions.SetPropertyThreadSafe(prgBar, "Maximum", vals.Item2);
            Functions.SetPropertyThreadSafe(prgBar, "Value", vals.Item1);
        }

        public DialogResult SafeShow(string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return (DialogResult)this.Invoke(new Func<DialogResult>(() => { return MessageBox.Show(this, msg, caption, buttons, icon, def); }));
        }

        private void ToggleInterface(bool enabled)
        {
            foreach (Control c in Controls)
            {
                if (c is Button)
                    c.Enabled = enabled;
            }
            txtMobi.Enabled = enabled;
            txtXMLFile.Enabled = enabled;
            txtGoodreads.Enabled = enabled;
            rdoFile.Enabled = enabled;
            rdoGoodreads.Enabled = enabled;
            btnCancel.Enabled = !enabled;
            // If process was canceled and we're disabling the interface for another time, reset token source
            if (enabled == false && cancelTokens.IsCancellationRequested)
            {
                cancelTokens.Dispose();
                cancelTokens = new CancellationTokenSource();
            }
            else if (enabled == true)
                UpdateProgressBar(new Tuple<int, int>(0, 0));
        }

        public static bool checkInternet()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead("https://www.google.com"))
                        return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = "";
            txtMobi.Text = Functions.GetBook(txtMobi.Text);
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(settings.outDir))
                MessageBox.Show(@"Specified output directory does not exist. Please review the settings page.", @"Output Directory Not found");
            else
                Process.Start(settings.outDir);
        }

        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = Functions.GetFile(txtXMLFile.Text, "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt");
        }

        private async void btnBuild_Click(object sender, EventArgs e)
        {
            ToggleInterface(false);
            await btnBuild_Run();
            ToggleInterface(true);
        }

        private async Task btnBuild_Run()
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(@"Specified book was not found.", @"Book Not Found");
                return;
            }
            if (rdoGoodreads.Checked && txtGoodreads.Text == "")
            {
                MessageBox.Show("No " + dataSource.Name + " link was specified.", "Missing " + dataSource.Name + " Link");
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
            if (settings.realName.Trim().Length == 0
                || settings.penName.Trim().Length == 0)
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
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = await Functions.GetMetaDataAsync(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }
            else
            {
                Logger.Log("Extracting metadata...");
                try
                {
                    results = (await Functions.GetMetaDataInternalAsync(txtMobi.Text, settings.outDir, true, randomFile)).getResults();
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }

            if (settings.saverawml)
            {
                Logger.Log("Saving rawML to dmp directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp", Path.GetFileName(results[3])), true);
            }

            // Added author name to log output
            Logger.Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nUniqueID: {1}",
                 results[2], results[1]));
            Logger.Log(String.Format("Book's {0} URL: {1}", dataSource.Name, txtGoodreads.Text));
            if (cancelTokens.IsCancellationRequested) return;
            Logger.Log("Attempting to build X-Ray...");

            //If AZW3 file use AZW3 offset, if checked. Checked by default.
            bool AZW3 = Path.GetExtension(txtMobi.Text) == ".azw3" && settings.overrideOffset;
            Logger.Log("Offset: " + (AZW3 ? settings.offsetAZW3.ToString() + " (AZW3)" : settings.offset.ToString()));

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            try
            {
                if (rdoGoodreads.Checked)
                    xray = new XRay(txtGoodreads.Text, results[2], results[1], results[0], this, dataSource,
                        (AZW3 ? settings.offsetAZW3 : settings.offset), "", false);
                else
                    xray = new XRay(txtXMLFile.Text, results[2], results[1], results[0], this, dataSource,
                        (AZW3 ? settings.offsetAZW3 : settings.offset), "");
                Progress<Tuple<int, int>> progress = new Progress<Tuple<int, int>>(UpdateProgressBar);

                if ((await Task.Run(() => xray.CreateXray(progress, cancelTokens.Token))) > 0)
                {
                    Logger.Log("Build canceled or error while processing.");
                    return;
                }
                Logger.Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if ((await Task.Run(() => xray.ExpandFromRawMl(results[3], progress, cancelTokens.Token, settings.ignoresofthyphen, !settings.useNewVersion))) > 0)
                {
                    Logger.Log("Build canceled or error occurred while processing locations and chapters.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while building the X-Ray:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return;
            }

            Logger.Log("Saving X-Ray to file...");
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
                Logger.Log("Failed to create output directory: " + ex.Message + "\r\n" + ex.StackTrace + "\r\nFiles will be placed in the default output directory.");
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
                    Logger.Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
                using (SQLiteConnection m_dbConnection = new SQLiteConnection(("Data Source=" + _newPath + ";Version=3;")))
                {
                    m_dbConnection.Open();
                    string sql;
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + @"\dist\BaseDB.sql", Encoding.UTF8))
                        {
                            sql = streamReader.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\n"
                             + ex.Message + "\r\n" + ex.StackTrace);
                        m_dbConnection.Dispose();
                        return;
                    }
                    SQLiteCommand command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", m_dbConnection);
                    Logger.Log("Building new X-Ray database. May take a few minutes...");
                    command.ExecuteNonQuery();
                    command.Dispose();
                    command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", m_dbConnection);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    Logger.Log("Done building initial database. Populating with info from source X-Ray...");
                    CancellationToken token = cancelTokens.Token;
                    try
                    {
                        await Task.Run(() =>
                        {
                            xray.PopulateDb(m_dbConnection, new Progress<Tuple<int, int>>(UpdateProgressBar), token);
                        }, token);
                    }
                    catch (Exception ex)
                    {
                        command.Dispose();
                        m_dbConnection.Close();
                        if (ex is OperationCanceledException)
                            Logger.Log("Building canceled.");
                        else
                            Logger.Log("An error occurred while populating the X-Ray database.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        return;
                    }
                    Logger.Log("Updating indices...");
                    sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                          + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                          + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC); COMMIT;";
                    command = new SQLiteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    m_dbConnection.Close();
                    XrPath = outFolder + @"\XRAY.entities." + results[0];
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
                    Logger.Log("X-Ray previewData file created successfully!\r\nSaved to " + PdPath);
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("An error occurred saving the previewData file: {0}\r\n{1}", ex.Message, ex.StackTrace));
                }
            }
            else
            {
                using (StreamWriter streamWriter = new StreamWriter(_newPath, false, settings.utf8 ? Encoding.UTF8 : Encoding.Default))
                {
                    streamWriter.Write(xray.ToString());
                }
            }
            Logger.Log("X-Ray file created successfully!\r\nSaved to " + _newPath);

            checkFiles(results[4], results[5], results[0]);

            if (settings.playSound)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
                player.Play();
            }

            try
            {
                if (settings.deleteTemp)
                    Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                Logger.Log(String.Format("An error occurred while trying to delete temporary files: {0}\r\n{1}\r\nTry deleting these files manually.", ex.Message, ex.StackTrace));
            }
        }

        private async void btnKindleExtras_Click(object sender, EventArgs e)
        {
            ToggleInterface(false);
            await btnKindleExtras_Run();
            ToggleInterface(true);
        }

        private async Task btnKindleExtras_Run()
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (rdoGoodreads.Checked)
            {
                if (txtGoodreads.Text == "")
                {
                    MessageBox.Show("No " + dataSource.Name + " link was specified.", "Missing " + dataSource.Name + " Link");
                    return;
                }
                else if (!txtGoodreads.Text.ToLower().Contains(settings.dataSource.ToLower()))
                {
                    MessageBox.Show(String.Format("Invalid {0} link was specified.\r\n"
                        + "If you do not want to use {0}, you can change the data source in Settings.", dataSource.Name)
                        , "Invalid " + dataSource.Name + " Link");
                    return;
                }
            }
            if (!File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show("Kindleunpack was not found. Please review the settings page.", "Kindleunpack Not Found");
                return;
            }
            if (settings.realName.Trim().Length == 0 |
                settings.penName.Trim().Length == 0)
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
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = await Functions.GetMetaDataAsync(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
                    if (!File.Exists(results[3]))
                    {
                        Logger.Log("Error: RawML could not be found, aborting.\r\nPath: " + results[3]);
                        return;
                    }
                    rawMLSize = new FileInfo(results[3]).Length;
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }
            else
            {
                Logger.Log("Extracting metadata...");
                try
                {
                    //Same results with addition of rawML filename
                    results = (await Functions.GetMetaDataInternalAsync(txtMobi.Text, settings.outDir, true, randomFile)).getResults();
                    rawMLSize = new FileInfo(results[3]).Length;
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }

            if (settings.saverawml && settings.useKindleUnpack)
            {
                Logger.Log("Saving rawML to dmp directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp",
                    Path.GetFileName(results[3])), true);
            }

            Logger.Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nUniqueID: {1}",
                 results[2], results[1]));
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            Logger.Log(String.Format("Book's {0} URL: {1}", dataSource.Name, txtGoodreads.Text));
            try
            {
                BookInfo bookInfo = new BookInfo(results[5], results[4], results[0], results[1], results[2],
                                                randomFile, Functions.RemoveInvalidFileChars(results[5]), txtGoodreads.Text, results[3]);

                string outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectory(bookInfo.author, bookInfo.sidecarName) : settings.outDir;

                Logger.Log("Attempting to build Author Profile...");
                AuthorProfile ap = new AuthorProfile(bookInfo, settings.amazonTLD);
                if (!(await ap.Generate())) return;
                SaPath = outputDir + @"\StartActions.data." + bookInfo.asin + ".asc";
                ApPath = outputDir + @"\AuthorProfile.profile." + bookInfo.asin + ".asc";
                Logger.Log("Attempting to build Start Actions and End Actions...");
                EndActions ea = new EndActions(ap, bookInfo, rawMLSize, dataSource, this);
                if (!(await ea.Generate())) return;

                if (settings.useNewVersion)
                {
                    await ea.GenerateEndActions(cancelTokens.Token);
                    ea.GenerateStartActions();
                    EaPath = outputDir + @"\EndActions.data." + bookInfo.asin + ".asc";
                }
                else
                    ea.GenerateOld();

                checkFiles(bookInfo.author, bookInfo.title, bookInfo.asin);
                if (settings.playSound)
                {
                    System.Media.SoundPlayer player =
                        new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while creating the new Author Profile, Start Actions, and/or End Actions files: " + ex.Message + "\r\n" + ex.StackTrace);
            }

        }

        private async void btnDownloadTerms_Click(object sender, EventArgs e)
        {
            if (txtGoodreads.Text == "")
            {
                MessageBox.Show("No link was specified.", "Missing Link");
                return;
            }
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            ToggleInterface(false);
            if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
            string path = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(txtMobi.Text) + ".xml";
            try
            {
                txtXMLFile.Text = path;

                XRay xray = new XRay(txtGoodreads.Text, this, dataSource);
                int result = await Task.Run(() => xray.SaveXml(path, new Progress<Tuple<int, int>>(UpdateProgressBar), cancelTokens.Token));
                if (result == 1)
                    Logger.Log("Warning: Unable to download character data as no character data found on Goodreads.");
                else if (result == 2)
                    Logger.Log("Download cancelled.");
                else
                    Logger.Log("Character data has been successfully saved to: " + path);
            }
            catch (Exception ex)
            {
                Logger.Log(String.Format("An error occurred while saving character data to XML: {0}\r\nPath was: {1}", ex.Message, path));
            }
            finally
            {
                ToggleInterface(true);
            }
        }

        private async void btnSearchGoodreads_Click(object sender, EventArgs e)
        {
            ToggleInterface(false);
            await btnSearchGoodreads_Run();
            ToggleInterface(true);
        }

        private async Task btnSearchGoodreads_Run()
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
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }
            else
            {
                Logger.Log("Extracting metadata...");
                try
                {
                    results = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, false).getResults();
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }

            Logger.Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nUniqueID: {1}",
                 results[2], results[1]));
            try
            {
                string bookUrl = await dataSource.SearchBook(results[4], results[5]);
                if (bookUrl != "")
                {
                    txtGoodreads.Text = bookUrl;
                    txtGoodreads.Refresh();
                    Logger.Log(String.Format("Book found on {3}!\r\n{0} by {1}\r\n{3} URL: {2}\r\nYou may want to visit the URL to ensure it is correct.",
                         results[5], results[4], bookUrl, dataSource.Name));
                }
                else
                    Logger.Log("Unable to find this book on " + dataSource.Name + "!");
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while searching: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            try
            {
                if (settings.deleteTemp)
                    Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                Logger.Log(
                     String.Format(
                         "An error occurred while trying to delete temporary files: {0}\r\n{1}\r\nTry deleting these files manually.",
                         ex.Message, ex.StackTrace));
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmSettings frmSet = new frmSettings();
            frmSet.ShowDialog();
            SetDatasourceLabels();

            if (!txtGoodreads.Text.ToLower().Contains(settings.dataSource.ToLower()))
                txtGoodreads.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.mobiFile = txtMobi.Text;
            settings.xmlFile = txtXMLFile.Text;
            settings.Goodreads = txtGoodreads.Text;
            if (rdoGoodreads.Checked)
                settings.buildSource = "Goodreads";
            else
                settings.buildSource = "XML";
            settings.Save();
            if (txtOutput.Text.Trim().Length != 0)
                File.WriteAllText(currentLog, txtOutput.Text.ToString());
            if (settings.deleteTemp)
            {
                try
                {
                    Functions.CleanUp(Environment.CurrentDirectory + @"\tmp\");
                }
                catch (Exception ex)
                {
                    Logger.Log(
                     String.Format(
                         "An error occurred while trying to delete temporary files: {0}\r\n{1}\r\nTry deleting these files manually.",
                         ex.Message, ex.StackTrace));
                }
            }
            Exiting = true;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Logger.ctrl = txtOutput;
            //this.WindowState = FormWindowState.Maximized;
            this.ActiveControl = lblGoodreads;
            toolTip1.SetToolTip(btnBrowseMobi, "Open a Kindle book.");
            toolTip1.SetToolTip(btnBrowseOutput, "Open the default output directory.");
            toolTip1.SetToolTip(btnOneClick, "One Click to try to build the Start\r\nAction, Author Profile, End Action\r\nand X-Ray files for this book.");
            toolTip1.SetToolTip(btnBrowseXML, "Open a supported XML or TXT file containing characters and topics.");
            toolTip1.SetToolTip(btnKindleExtras,
                "Try to build the Start Action, Author Profile,\r\nand End Action files for this book.");
            toolTip1.SetToolTip(btnBuild,
                "Try to build the X-Ray file for this book.");
            toolTip1.SetToolTip(btnSettings, "Configure X-Ray Builder GUI.");
            toolTip1.SetToolTip(btnPreview, "View a preview of the generated files.");
            toolTip1.SetToolTip(btnUnpack, "Save the rawML (raw markup) of the book\r\nin the output directory so you can review it.");
            toolTip1.SetToolTip(btnExtractTerms,
                "Extract an existing X-Ray file to an XML file.\r\nThis can be useful if you have characters and\r\nterms you want to reuse.");
            toolTip1.SetToolTip(btnCreate, "Create an XML file containing characters\r\nand settings, or edit an existing XML file.");

            toolTip1.SetToolTip(pbFile1, "Start Actions");
            toolTip1.SetToolTip(pbFile2, "Author Profile");
            toolTip1.SetToolTip(pbFile3, "End Actions");
            toolTip1.SetToolTip(pbFile4, "X-Ray");

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

            if (txtMobi.Text == "") txtMobi.Text = settings.mobiFile;

            if (txtXMLFile.Text == "") txtXMLFile.Text = settings.xmlFile;
            if (!Directory.Exists(Environment.CurrentDirectory + @"\out"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\out");
            if (settings.outDir == "")
                settings.outDir = Environment.CurrentDirectory + @"\out";
            if (!Directory.Exists(Environment.CurrentDirectory + @"\log"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\log");
            if (!Directory.Exists(Environment.CurrentDirectory + @"\dmp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\dmp");
            if (!Directory.Exists(Environment.CurrentDirectory + @"\tmp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\tmp");
            if (settings.tmpDir == "")
                settings.tmpDir = Environment.CurrentDirectory + @"\tmp";
            if (settings.mobi_unpack == "")
                settings.mobi_unpack = Environment.CurrentDirectory + @"\dist\kindleunpack.exe";

            txtGoodreads.Text = settings.Goodreads;
            if (settings.buildSource == "Goodreads")
                rdoGoodreads.Checked = true;
            else
                rdoFile.Checked = true;
            SetDatasourceLabels();
        }

        private void SetDatasourceLabels()
        {
            if (settings.dataSource == "Goodreads")
            {
                btnSearchGoodreads.Enabled = true;
                dataSource = new Goodreads();
                rdoGoodreads.Text = "Goodreads";
                lblGoodreads.Text = "Goodreads URL:";
                lblGoodreads.Left = 134;
                toolTip1.SetToolTip(btnDownloadTerms, "Save Goodreads info to an XML file.");
                toolTip1.SetToolTip(btnSearchGoodreads, "Try to search for this book on Goodreads.");
            }
            else
            {
                btnSearchGoodreads.Enabled = false;
                dataSource = new Shelfari();
                rdoGoodreads.Text = "Shelfari";
                lblGoodreads.Text = "Shelfari URL:";
                lblGoodreads.Left = 150;
                toolTip1.SetToolTip(btnDownloadTerms, "Save Shelfari info to an XML file.");
                toolTip1.SetToolTip(btnSearchGoodreads, "Search is disabled when Shelfari is selected as a data source.");
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

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void rdoSource_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Text != "File")
            {
                lblGoodreads.Visible = !lblGoodreads.Visible;
                txtGoodreads.Visible = !txtGoodreads.Visible;
                lblXMLFile.Visible = !lblXMLFile.Visible;
                txtXMLFile.Visible = !txtXMLFile.Visible;
                txtGoodreads.Visible = !txtGoodreads.Visible;
                btnBrowseXML.Visible = !btnBrowseXML.Visible;
                btnSearchGoodreads.Visible = !btnSearchGoodreads.Visible;
            }
            if (((RadioButton)sender).Text == "Shelfari")
                lblGoodreads.Left = 150;
            else if (((RadioButton)sender).Text == "Goodreads")
                lblGoodreads.Left = 134;
        }

        private void txtMobi_TextChanged(object sender, EventArgs e)
        {
            if (txtMobi.Text == "" || !File.Exists(txtMobi.Text)) return;
            txtGoodreads.Text = "";
            prgBar.Value = 0;

            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show("Temporary path not accessible for some reason.", "Temporary Directory Error");
                return;
            }
            List<string> results;
            if (settings.useKindleUnpack)
            {
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
                    if (results.Count == 7)
                        pbCover.Image = new Bitmap(results[6]);
                    else
                        pbCover.Image = null;
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    txtMobi.Text = "";
                    return;
                }
            }
            else
            {
                try
                {
                    using (Unpack.Metadata md = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, false))
                    {
                        results = md.getResults();
                        pbCover.Image = (Image)md.coverImage.Clone();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    txtMobi.Text = "";
                    return;
                }
            }

            string outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectoryOnly(results[4], results[5]) : settings.outDir;

            lblTitle.Visible = true;
            lblAuthor.Visible = true;
            lblAsin.Visible = true;
            txtTitle.Visible = true;
            txtAuthor.Visible = true;
            txtAsin.Visible = true;

            txtAuthor.Text = results[4];
            txtTitle.Text = results[5];
            txtAsin.Text = results[0];
            toolTip1.SetToolTip(txtAsin, String.Format(@"https://www.amazon.{0}/dp/{1}", settings.amazonTLD, txtAsin.Text));

            openBook.Clear();
            openBook.Add(results[4]);
            openBook.Add(results[5]);
            openBook.Add(results[0]);

            checkFiles(results[4], results[5], results[0]);

            try
            {
                // Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                Logger.Log(
                     String.Format(
                         "An error occurred while trying to delete temporary files: {0}\r\n{1}\r\n" +
                         "Try deleting these files manually.",
                         ex.Message, ex.StackTrace));
            }
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
            string selPath = "";
            if (File.Exists(ApPath))
                selPath = ApPath;
            else
            {
                OpenFileDialog openFile = new OpenFileDialog
                {
                    Title = "Open a Kindle AuthorProfile file...",
                    Filter = "ASC files|*.asc",
                    InitialDirectory = settings.outDir
                };
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    if (openFile.FileName.Contains("AuthorProfile"))
                        selPath = openFile.FileName;
                    else
                    {
                        Logger.Log("Invalid Author Profile file.");
                        return;
                    }
                }
            }
            if (selPath != "")
            {
                try
                {
                    frmPreviewAP frmAuthorProfile = new frmPreviewAP();
                    frmAuthorProfile.populateAuthorProfile(selPath);
                    frmAuthorProfile.Location = new Point(this.Left, this.Top);
                    frmAuthorProfile.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        private async void tmiStartAction_Click(object sender, EventArgs e)
        {
            string selPath = "";
            if (File.Exists(SaPath))
                selPath = SaPath;
            else
            {
                OpenFileDialog openFile = new OpenFileDialog
                {
                    Title = "Open a Kindle StartActions file...",
                    Filter = "ASC files|*.asc",
                    InitialDirectory = settings.outDir
                };
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    if (openFile.FileName.Contains("StartActions"))
                        selPath = openFile.FileName;
                    else
                    {
                        Logger.Log("Invalid Start Actions file.");
                        return;
                    }
                }
            }
            if (selPath != "")
            {
                try
                {
                    frmPreviewSA frmStartAction = new frmPreviewSA();
                    await frmStartAction.populateStartActions(selPath);
                    frmStartAction.Location = new Point(this.Left, this.Top);
                    frmStartAction.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        private async void tmiEndAction_Click(object sender, EventArgs e)
        {
            string selPath = "";
            if (File.Exists(EaPath))
                selPath = EaPath;
            else
            {
                OpenFileDialog openFile = new OpenFileDialog
                {
                    Title = "Open a Kindle EndActions file...",
                    Filter = "ASC files|*.asc",
                    InitialDirectory = settings.outDir
                };
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    if (openFile.FileName.Contains("EndActions"))
                        selPath = openFile.FileName;
                    else
                    {
                        Logger.Log("Invalid End Actions file.");
                        return;
                    }
                }
            }
            if (selPath != "")
            {
                try
                {
                    frmPreviewEA frmEndAction = new frmPreviewEA();
                    await frmEndAction.populateEndActions(selPath);
                    frmEndAction.Location = new Point(this.Left, this.Top);
                    frmEndAction.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        private void tmiXray_Click(object sender, EventArgs e)
        {
            string selPath = "";
            if (File.Exists(XrPath))
                selPath = XrPath;
            else
            {
                OpenFileDialog openFile = new OpenFileDialog
                {
                    Title = "Open a Kindle X-Ray file...",
                    Filter = "ASC files|*.asc",
                    InitialDirectory = settings.outDir
                };
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    if (openFile.FileName.Contains("XRAY.entities"))
                        selPath = openFile.FileName;
                    else
                    {
                        Logger.Log("Invalid X-Ray file.");
                        return;
                    }
                }
            }
            if (selPath != "")
            {

                try
                {
                    int ver = CheckXRayVersion(selPath);
                    if (ver == 0)
                    {
                        Logger.Log("Invalid X-Ray file.");
                        return;
                    }
                    List<XRay.Term> terms = ver == 2 ? ExtractTermsNew(selPath) : ExtractTermsOld(selPath);
                    
                    frmPreviewXR frmXraPreview = new frmPreviewXR();
                    frmXraPreview.PopulateXRay(terms);
                    frmXraPreview.ShowDialog();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
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
                Logger.Log("Running Kindleunpack to extract rawML...");
                try
                {
                    results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting rawML: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }
            else
            {
                Logger.Log("Extracting rawML...");
                try
                {
                    results = Functions.GetMetaDataInternal(txtMobi.Text, settings.outDir, true, randomFile).getResults();
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting rawML: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }
            string rawmlPath = Path.Combine(Environment.CurrentDirectory + @"\dmp", Path.GetFileName(results[3]));
            File.Copy(results[3], rawmlPath, true);
            Logger.Log("Extracted rawml successfully!\r\nSaved to " + rawmlPath);
        }

        private void txtOutput_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private async void btnOneClick_Click(object sender, EventArgs e)
        {
            ToggleInterface(false);
            await btnKindleExtras_Run();
            await btnBuild_Run();
            ToggleInterface(true);
        }

        private void txtAsin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = String.Format(@"https://www.amazon.{0}/dp/{1}", settings.amazonTLD, txtAsin.Text);
            Process.Start(link);
        }

        private void btnExtractTerms_Click(object sender, EventArgs e)
        {
            string selPath = "";
            OpenFileDialog openFile = new OpenFileDialog
            {
                Title = "Open a Kindle X-Ray file...",
                Filter = "ASC files|*.asc",
                InitialDirectory = settings.outDir
            };
            if (openFile.ShowDialog() == DialogResult.OK)
                selPath = openFile.FileName;
            if (selPath == "" || !selPath.Contains("XRAY.entities"))
            {
                Logger.Log("Invalid or no file selected.");
                return;
            }
            int newVer = CheckXRayVersion(selPath);
            if (newVer == 0)
            {
                Logger.Log("Invalid X-Ray file.");
                return;
            }
            try
            {
                List<XRay.Term> terms = newVer == 2 ? ExtractTermsNew(selPath) : ExtractTermsOld(selPath);
                if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                    Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
                string outfile = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(selPath) + ".xml";
                Functions.Save<List<XRay.Term>>(terms, outfile);
                Logger.Log("Character data has been saved to: " + outfile);
            }
            catch (Exception ex)
            {
                Logger.Log("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        // 0 = invalid, 1 = old, 2 = new
        public int CheckXRayVersion(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int c = fs.ReadByte();
                if (c == 'S')
                    return 2;
                else if (c == '{')
                    return 1;
                return 0;
            }
        }

        private List<XRay.Term> ExtractTermsNew(string path)
        {
            List<XRay.Term> terms = new List<XRay.Term>(100);

            string xrayDB = "Data Source=" + path + ";Version=3;";
            SQLiteConnection m_dbConnection = new SQLiteConnection(xrayDB);
            m_dbConnection.Open();

            string sql = "SELECT * FROM entity WHERE has_info_card = '1'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                XRay.Term newTerm = new XRay.Term
                {
                    Id = reader.GetInt32(0),
                    TermName = reader.GetString(1),
                    Type = reader.GetInt32(3) == 1 ? "character" : "topic"
                };
                //if (newTerm.Type == "character")
                //{
                //    newTerm.DescSrc = "Kindle Store";
                //    newTerm.DescUrl = String.Format(@"http://www.amazon.{0}/s/ref=nb_sb_ss_i_5_4?url=search-alias%3Ddigital-text&field-keywords={1}",
                //        settings.amazonTLD, newTerm.TermName.Replace(" ", "+"));
                //}
                //else
                //{
                // Actual location aren't needed for extracting terms for preview or XML saving, but need count
                int i = reader.GetInt32(4);
                for (; i > 0; i--)
                    newTerm.Locs.Add(null);
                newTerm.DescSrc = "Wikipedia";
                newTerm.DescUrl = String.Format(@"http://en.wikipedia.org/wiki/{0}", newTerm.TermName.Replace(" ", "_"));
                //newTerm.DescSrc = Convert.ToString(reader.GetInt32(4));
                //}
                terms.Add(newTerm);
            }

            command.Dispose();

            for (int i = 1; i < terms.Count + 1; i++)
            {
                sql = String.Format("SELECT * FROM entity_description WHERE entity = '{0}'", i);

                command = new SQLiteCommand(sql, m_dbConnection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    terms[i - 1].Desc = reader.GetString(0);
                command.Dispose();
            }
            m_dbConnection.Close();
            return terms;
        }

        private List<XRay.Term> ExtractTermsOld(string path)
        {
            List<XRay.Term> terms;
            string readContents;
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
                readContents = streamReader.ReadToEnd();

            JObject xray = JObject.Parse(readContents);
            var termsjson = xray["terms"].Children().ToList();
            terms = new List<XRay.Term>(termsjson.Count);
            foreach (var term in termsjson)
                terms.Add(term.ToObject<XRay.Term>());
            return terms;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Environment.CurrentDirectory + @"\doc\help.pdf");
            }
            catch
            {
                MessageBox.Show(@"Unable to open the supplied help document.", @"Help Document Not found");
            }

        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            frmInfo.ShowDialog();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (openBook.Count == 3)
            {
                frmCreator.txtAuthor.Text = openBook[0];
                frmCreator.txtTitle.Text = openBook[1];
                frmCreator.txtAsin.Text = openBook[2];
                frmCreator.ShowDialog();
            }
            else
                frmCreator.ShowDialog();
        }

        private void checkFiles(string author, string title, string asin)
        {
            string bookOutputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectoryOnly(author, Functions.RemoveInvalidFileChars(title)) : settings.outDir;

            if (File.Exists(bookOutputDir + @"\StartActions.data." + asin + ".asc"))
                pbFile1.Image = Resources.file_on;
            else
                pbFile1.Image = Resources.file_off;
            if (File.Exists(bookOutputDir + @"\AuthorProfile.profile." + asin + ".asc"))
                pbFile2.Image = Resources.file_on;
            else
                pbFile2.Image = Resources.file_off;
            if (File.Exists(bookOutputDir + @"\EndActions.data." + asin + ".asc"))
                pbFile3.Image = Resources.file_on;
            else
                pbFile3.Image = Resources.file_off;
            XrPath = bookOutputDir + @"\XRAY.entities." + asin + ".asc";
            if (File.Exists(XrPath))
                pbFile4.Image = Resources.file_on;
            else
                pbFile4.Image = Resources.file_off;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!cancelTokens.IsCancellationRequested)
            {
                Logger.Log("Canceling...");
                cancelTokens.Cancel();
            }
        }
    }
}