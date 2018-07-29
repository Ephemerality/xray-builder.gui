using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilderGUI.DataSources;
using XRayBuilderGUI.Properties;
using Newtonsoft.Json.Linq;
using XRayBuilderGUI.Unpack;

namespace XRayBuilderGUI
{
    public partial class frmMain : Form
    {
        public bool Exiting;
        
        private string EaPath = "";
        private string SaPath = "";
        private string ApPath = "";
        private string XrPath = "";

        public frmMain()
        {
            InitializeComponent();
            _progress = new ProgressBarCtrl(prgBar);
        }

        private readonly frmAbout _frmInfo = new frmAbout();
        private readonly frmCreateXR _frmCreator = new frmCreateXR();
        private readonly ToolTip _tooltip = new ToolTip();
        private readonly Settings _settings = Settings.Default;
        private readonly string _currentLog = $@"{Environment.CurrentDirectory}\log\{DateTime.Now:HH.mm.ss.dd.MM.yyyy}.txt";

        public List<string> openBook = new List<string>();
        
        DataSource dataSource;

        private readonly IProgressBar _progress;

        CancellationTokenSource cancelTokens = new CancellationTokenSource();

        public DialogResult SafeShow(string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return (DialogResult)Invoke(new Func<DialogResult>(() => MessageBox.Show(this, msg, caption, buttons, icon, def)));
        }

        public string OutputDirectory(string author, string title, bool create)
        {
            if (!_settings.useSubDirectories) return _settings.outDir;
            if (!Functions.ValidateFilename(author, title))
                Logger.Log("Warning: The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.");
            return Functions.GetBookOutputDirectory(author, title, create);
        }

        public string AmazonUrl(string asin) => $"https://www.amazon.{_settings.amazonTLD}/dp/{asin}";

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
            else if (enabled)
                _progress.Set(0, 0);
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = "";
            txtMobi.Text = UIFunctions.GetBook(txtMobi.Text);
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_settings.outDir))
                MessageBox.Show(@"Specified output directory does not exist. Please review the settings page.", @"Output Directory Not found");
            else
                Process.Start(_settings.outDir);
        }

        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = UIFunctions.GetFile(txtXMLFile.Text, "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt");
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
            if (_settings.useKindleUnpack && !File.Exists(_settings.mobi_unpack))
            {
                MessageBox.Show(@"Kindleunpack was not found.\r\nPlease review the settings page.", @"Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(_settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist.\r\nPlease review the settings page.", @"Output Directory Not found");
                return;
            }
            if (_settings.realName.Trim().Length == 0 || _settings.penName.Trim().Length == 0)
            {
                MessageBox.Show(
                    @"Both Real and Pen names are required for End Action\r\n" +
                    @"file creation. This information allows you to rate this\r\n" +
                    "book on Amazon. Please review the settings page.",
                    "Amazon Customer Details Not found");
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
            if (_settings.useKindleUnpack)
            {
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = await Task.Run(() => Functions.GetMetaData(txtMobi.Text, _settings.outDir, randomFile, _settings.mobi_unpack));
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
                    results = (await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.outDir, true, randomFile))).getResults();
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }

            if (_settings.saverawml)
            {
                Logger.Log("Saving rawML to dmp directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp", Path.GetFileName(results[3])), true);
            }

            // Added author name to log output
            Logger.Log($"Got metadata!\r\nDatabase Name: {results[2]}\r\nUniqueID: {results[1]}");
            Logger.Log($"Book's {dataSource.Name} URL: {txtGoodreads.Text}");
            if (cancelTokens.IsCancellationRequested) return;
            Logger.Log("Attempting to build X-Ray...");

            //If AZW3 file use AZW3 offset, if checked. Checked by default.
            bool AZW3 = Path.GetExtension(txtMobi.Text) == ".azw3" && _settings.overrideOffset;
            Logger.Log("Offset: " + (AZW3 ? $"{_settings.offsetAZW3} (AZW3)" : _settings.offset.ToString()));

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            try
            {
                if (rdoGoodreads.Checked)
                    xray = new XRay(txtGoodreads.Text, results[2], results[1], results[0], dataSource,
                        AZW3 ? _settings.offsetAZW3 : _settings.offset, "", false);
                else
                    xray = new XRay(txtXMLFile.Text, results[2], results[1], results[0], dataSource,
                        AZW3 ? _settings.offsetAZW3 : _settings.offset, "");

                await Task.Run(() => xray.CreateXray(_progress, cancelTokens.Token)).ConfigureAwait(false);

                xray.ExportAndDisplayTerms();

                if (_settings.enableEdit && DialogResult.Yes ==
                    MessageBox.Show(
                        "Terms have been exported to an alias file or already exist in that file. Would you like to open the file in notepad for editing?\r\n"
                        + "See the MobileRead forum thread (link in Settings) for more information on building aliases.",
                        "Aliases",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(xray.AliasPath);
                }
                if (!File.Exists(xray.AliasPath))
                    Logger.Log("Aliases file not found.");
                else
                {
                    xray.LoadAliases();
                    Logger.Log($"Character aliases read from {xray.AliasPath}.");
                }

                Logger.Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if (await Task.Run(() => xray.ExpandFromRawMl(results[3], SafeShow, _progress, cancelTokens.Token, _settings.ignoresofthyphen, !_settings.useNewVersion)).ConfigureAwait(false) > 0)
                {
                    Logger.Log("Build canceled or error occurred while processing locations and chapters.");
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Build canceled.");
                return;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while building the X-Ray:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return;
            }

            Logger.Log("Saving X-Ray to file...");
            string outFolder;
            try
            {
                if (_settings.android)
                {
                    outFolder = _settings.outDir + @"\Android\" + results[0];
                    Directory.CreateDirectory(outFolder);
                }
                else
                {
                    outFolder = OutputDirectory(results[4], results[5], true);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to create output directory: " + ex.Message + "\r\n" + ex.StackTrace + "\r\nFiles will be placed in the default output directory.");
                outFolder = _settings.outDir;
            }
            var newPath = outFolder + "\\" + xray.XRayName(_settings.android);

            if (_settings.useNewVersion)
            {
                try
                {
                    xray.SaveToFileNew(newPath, _progress, cancelTokens.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.Log("Building canceled.");
                    return;
                }
                catch (Exception ex)
                {
                    // TODO: Add option to retry maybe?
                    Logger.Log($"An error occurred while creating the new X-Ray database. Is it opened in another program?\r\n{ex.Message}");
                    return;
                }
                XrPath = outFolder + @"\XRAY.entities." + results[0];

                //Save the new XRAY.ASIN.previewData file
                try
                {
                    string PdPath = outFolder + @"\XRAY." + results[0] + ".previewData";
                    xray.SavePreviewToFile(PdPath);
                    Logger.Log($"X-Ray previewData file created successfully!\r\nSaved to {PdPath}");
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("An error occurred saving the previewData file: {0}\r\n{1}", ex.Message, ex.StackTrace));
                }
            }
            else
            {
                xray.SaveToFileOld(newPath);
            }
            Logger.Log($"X-Ray file created successfully!\r\nSaved to {newPath}");

            checkFiles(results[4], results[5], results[0]);

            if (_settings.playSound)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
                player.Play();
            }

            try
            {
                if (_settings.deleteTemp)
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
                    MessageBox.Show($"No {dataSource.Name} link was specified.", $"Missing {dataSource.Name} Link");
                    return;
                }
                if (!txtGoodreads.Text.ToLower().Contains(_settings.dataSource.ToLower()))
                {
                    MessageBox.Show($"Invalid {dataSource.Name} link was specified.\r\n"
                        + $"If you do not want to use {dataSource.Name}, you can change the data source in Settings."
                        , $"Invalid {dataSource.Name} Link");
                    return;
                }
            }
            if (!File.Exists(_settings.mobi_unpack))
            {
                MessageBox.Show("Kindleunpack was not found. Please review the settings page.", "Kindleunpack Not Found");
                return;
            }
            if (_settings.realName.Trim().Length == 0 |
                _settings.penName.Trim().Length == 0)
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
            long rawMLSize;
            if (_settings.useKindleUnpack)
            {
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = await Task.Run(() => Functions.GetMetaData(txtMobi.Text, _settings.outDir, randomFile, _settings.mobi_unpack));
                    if (!File.Exists(results[3]))
                    {
                        Logger.Log($"Error: RawML could not be found, aborting.\r\nPath: {results[3]}");
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
                    results = (await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.outDir, true, randomFile))).getResults();
                    rawMLSize = new FileInfo(results[3]).Length;
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }

            if (_settings.saverawml && _settings.useKindleUnpack)
            {
                Logger.Log("Saving rawML to dmp directory...");
                File.Copy(results[3], Path.Combine(Environment.CurrentDirectory + @"\dmp", Path.GetFileName(results[3])), true);
            }

            Logger.Log($"Got metadata!\r\nDatabase Name: {results[2]}\r\nUniqueID: {results[1]}");
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            Logger.Log($"Book's {dataSource.Name} URL: {txtGoodreads.Text}");
            try
            {
                BookInfo bookInfo = new BookInfo(results[5], results[4], results[0], results[1], results[2],
                                                randomFile, Functions.RemoveInvalidFileChars(results[5]), txtGoodreads.Text, results[3]);

                string outputDir = OutputDirectory(bookInfo.author, bookInfo.sidecarName, true);

                Logger.Log("Attempting to build Author Profile...");
                AuthorProfile ap = new AuthorProfile(bookInfo, new AuthorProfile.Settings
                {
                    AmazonTld = _settings.amazonTLD,
                    Android = _settings.android,
                    OutDir = _settings.outDir,
                    SaveBio = _settings.saveBio,
                    UseNewVersion = _settings.useNewVersion,
                    UseSubDirectories = _settings.useSubDirectories
                });
                if (!await ap.Generate()) return;
                SaPath = $@"{outputDir}\StartActions.data.{bookInfo.asin}.asc";
                ApPath = $@"{outputDir}\AuthorProfile.profile.{bookInfo.asin}.asc";
                Logger.Log("Attempting to build Start Actions and End Actions...");
                EndActions ea = new EndActions(ap, bookInfo, rawMLSize, dataSource, new EndActions.Settings
                {
                    AmazonTld = _settings.amazonTLD,
                    Android = _settings.android,
                    OutDir = _settings.outDir,
                    PenName = _settings.penName,
                    RealName = _settings.realName,
                    UseNewVersion = _settings.useNewVersion,
                    UseSubDirectories = _settings.useSubDirectories
                });
                if (!await ea.Generate()) return;

                if (_settings.useNewVersion)
                {
                    await ea.GenerateNewFormatData(_progress, cancelTokens.Token);
                    await ea.GenerateEndActions(_progress, cancelTokens.Token);
                    ea.GenerateStartActions();
                    cmsPreview.Items[3].Enabled = true;
                    EaPath = $@"{outputDir}\EndActions.data.{bookInfo.asin}.asc";
                }
                else
                    ea.GenerateOld();

                cmsPreview.Items[1].Enabled = true;

                checkFiles(bookInfo.author, bookInfo.title, bookInfo.asin);
                if (_settings.playSound)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
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

                XRay xray = new XRay(txtGoodreads.Text, dataSource);
                int result = await Task.Run(() => xray.SaveXml(path, _progress, cancelTokens.Token));
                if (result == 1)
                    Logger.Log("Warning: Unable to download character data as no character data found on Goodreads.");
                else if (result == 2)
                    Logger.Log("Download cancelled.");
                else
                    Logger.Log("Character data has been successfully saved to: " + path);
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred while saving character data to XML: {ex.Message}\r\nPath was: {path}");
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
            if (!File.Exists(_settings.mobi_unpack))
            {
                MessageBox.Show("Kindleunpack was not found. Please review the settings page.", "Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(_settings.outDir))
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
            if (_settings.useKindleUnpack)
            {
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = await Task.Run(() => Functions.GetMetaData(txtMobi.Text, _settings.outDir, randomFile, _settings.mobi_unpack));
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
                    results = (await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.outDir, false))).getResults();
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
            }

            Logger.Log($"Got metadata!\r\nDatabase Name: {results[2]}\r\nUniqueID: {results[1]}");
            try
            {
                List<BookInfo> books = await dataSource.SearchBook(results[4], results[5]);
                string bookUrl = books?.Count == 1 ? books[0].dataUrl : "";
                if (books?.Count > 1)
                {
                    Logger.Log($"Warning: Multiple results returned from {dataSource.Name}...");
                    var frmG = new frmGR {BookList = books};
                    frmG.ShowDialog();
                    bookUrl = books[frmG.cbResults.SelectedIndex].dataUrl;
                }
                else if (books?.Count == 1)
                    bookUrl = books[0].dataUrl;
                else
                    Logger.Log($"Unable to find this book on {dataSource.Name}!");

                if (bookUrl != "")
                {
                    txtGoodreads.Text = bookUrl;
                    txtGoodreads.Refresh();
                    Logger.Log($"Book found on {dataSource.Name}!\r\n{results[5]} by {results[4]}\r\n{dataSource.Name} URL: {bookUrl}\r\n"
                        + "You may want to visit the URL to ensure it is correct.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while searching: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            try
            {
                if (_settings.deleteTemp)
                    Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred while trying to delete temporary files: {ex.Message}\r\n{ex.StackTrace}\r\n"
                    + "Try deleting these files manually.");
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmSettings frmSet = new frmSettings();
            frmSet.ShowDialog();
            SetDatasourceLabels();

            if (!txtGoodreads.Text.ToLower().Contains(_settings.dataSource.ToLower()))
                txtGoodreads.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settings.mobiFile = txtMobi.Text;
            _settings.xmlFile = txtXMLFile.Text;
            _settings.Goodreads = txtGoodreads.Text;
            _settings.buildSource = rdoGoodreads.Checked ? "Goodreads" : "XML";
            _settings.Save();
            if (txtOutput.Text.Trim().Length != 0)
                File.WriteAllText(_currentLog, txtOutput.Text);
            if (_settings.deleteTemp)
            {
                try
                {
                    Functions.CleanUp(Environment.CurrentDirectory + @"\tmp\");
                }
                catch (Exception ex)
                {
                    Logger.Log($"An error occurred while trying to delete temporary files: {ex.Message}\r\n{ex.StackTrace}\r\nTry deleting these files manually.");
                }
            }
            Exiting = true;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Logger.ctrl = txtOutput;
            //this.WindowState = FormWindowState.Maximized;
            ActiveControl = lblGoodreads;
            _tooltip.SetToolTip(btnBrowseMobi, "Open a Kindle book.");
            _tooltip.SetToolTip(btnBrowseOutput, "Open the default output directory.");
            _tooltip.SetToolTip(btnOneClick, "One Click to try to build the Start\r\nAction, Author Profile, End Action\r\nand X-Ray files for this book.");
            _tooltip.SetToolTip(btnBrowseXML, "Open a supported XML or TXT file containing characters and topics.");
            _tooltip.SetToolTip(btnKindleExtras,
                "Try to build the Start Action, Author Profile,\r\nand End Action files for this book.");
            _tooltip.SetToolTip(btnBuild,
                "Try to build the X-Ray file for this book.");
            _tooltip.SetToolTip(btnSettings, "Configure X-Ray Builder GUI.");
            _tooltip.SetToolTip(btnPreview, "View a preview of the generated files.");
            _tooltip.SetToolTip(btnUnpack, "Save the rawML (raw markup) of the book\r\nin the output directory so you can review it.");
            _tooltip.SetToolTip(btnExtractTerms,
                "Extract an existing X-Ray file to an XML file.\r\nThis can be useful if you have characters and\r\nterms you want to reuse.");
            _tooltip.SetToolTip(btnCreate, "Create an XML file containing characters\r\nand settings, or edit an existing XML file.");

            _tooltip.SetToolTip(pbFile1, "Start Actions");
            _tooltip.SetToolTip(pbFile2, "Author Profile");
            _tooltip.SetToolTip(pbFile3, "End Actions");
            _tooltip.SetToolTip(pbFile4, "X-Ray");

            DragEnter += frmMain_DragEnter;
            DragDrop += frmMain_DragDrop;

            string[] args = Environment.GetCommandLineArgs();

            txtMobi.Text = args.Skip(1).Where(File.Exists).Select(Path.GetFullPath).FirstOrDefault()
                           ?? _settings.mobiFile;

            if (txtXMLFile.Text == "") txtXMLFile.Text = _settings.xmlFile;
            if (!Directory.Exists(Environment.CurrentDirectory + @"\out"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\out");
            if (_settings.outDir == "")
                _settings.outDir = Environment.CurrentDirectory + @"\out";
            if (!Directory.Exists(Environment.CurrentDirectory + @"\log"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\log");
            if (!Directory.Exists(Environment.CurrentDirectory + @"\dmp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\dmp");
            if (!Directory.Exists(Environment.CurrentDirectory + @"\tmp"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\tmp");
            if (_settings.tmpDir == "")
                _settings.tmpDir = Environment.CurrentDirectory + @"\tmp";
            if (_settings.mobi_unpack == "")
                _settings.mobi_unpack = Environment.CurrentDirectory + @"\dist\kindleunpack.exe";

            txtGoodreads.Text = _settings.Goodreads;
            if (_settings.buildSource == "Goodreads")
                rdoGoodreads.Checked = true;
            else
                rdoFile.Checked = true;
            SetDatasourceLabels();
        }

        private void SetDatasourceLabels()
        {
            if (_settings.dataSource == "Goodreads")
            {
                btnSearchGoodreads.Enabled = true;
                dataSource = new Goodreads();
                rdoGoodreads.Text = "Goodreads";
                lblGoodreads.Text = "Goodreads URL:";
                lblGoodreads.Left = 134;
                _tooltip.SetToolTip(btnDownloadTerms, "Save Goodreads info to an XML file.");
                _tooltip.SetToolTip(btnSearchGoodreads, "Try to search for this book on Goodreads.");
            }
            else
            {
                btnSearchGoodreads.Enabled = false;
                dataSource = new Shelfari();
                rdoGoodreads.Text = "Shelfari";
                lblGoodreads.Text = "Shelfari URL:";
                lblGoodreads.Left = 150;
                _tooltip.SetToolTip(btnDownloadTerms, "Save Shelfari info to an XML file.");
                _tooltip.SetToolTip(btnSearchGoodreads, "Search is disabled when Shelfari is selected as a data source.");
            }
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
            foreach (string fileLoc in filePaths)
            {
                if (!File.Exists(fileLoc)) continue;
                txtMobi.Text = fileLoc;
                return;
            }
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
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

        private async void txtMobi_TextChanged(object sender, EventArgs e)
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
            
            if (_settings.useKindleUnpack)
            {
                Logger.Log("Running Kindleunpack to get metadata...");
                try
                {
                    results = await Task.Run(() => Functions.GetMetaData(txtMobi.Text, _settings.outDir, randomFile, _settings.mobi_unpack));
                    pbCover.Image = results.Count == 7 ? new Bitmap(results[6]) : null;
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
                    var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.outDir, false));
                    metadata.CheckDRM();
                    results = metadata.getResults();
                    pbCover.Image = (Image) metadata.coverImage?.Clone();
                }
                catch (Exception ex)
                {
                    Logger.Log("An error occurred extracting metadata: " + ex.Message + "\r\n" + ex.StackTrace);
                    txtMobi.Text = "";
                    return;
                }
            }

            lblTitle.Visible = true;
            lblAuthor.Visible = true;
            lblAsin.Visible = true;
            txtTitle.Visible = true;
            txtAuthor.Visible = true;
            txtAsin.Visible = true;

            txtAuthor.Text = results[4];
            txtTitle.Text = results[5];
            txtAsin.Text = results[0];
            _tooltip.SetToolTip(txtAsin, AmazonUrl(txtAsin.Text));

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
                Logger.Log($"An error occurred while trying to delete temporary files: {ex.Message}\r\n{ex.StackTrace}\r\n"
                    + "Try deleting these files manually.");
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (cmsPreview.Visible)
                cmsPreview.Hide();
            else
                cmsPreview.Show(btnPreview, new Point(2, btnPreview.Height));
        }

        // TODO: Clean up these preview handlers
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
                    InitialDirectory = _settings.outDir
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
                    frmAuthorProfile.PopulateAuthorProfile(selPath);
                    frmAuthorProfile.Location = new Point(Left, Top);
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
                    InitialDirectory = _settings.outDir
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
                    frmStartAction.Location = new Point(Left, Top);
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
                    InitialDirectory = _settings.outDir
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
                    await frmEndAction.PopulateEndActions(selPath);
                    frmEndAction.Location = new Point(Left, Top);
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
            string selPath;
            if (File.Exists(XrPath))
                selPath = XrPath;
            else
            {
                selPath = UIFunctions.GetFile("Open a Kindle X-Ray file...", null, "ASC files|*.asc", _settings.outDir);
                if (selPath.Contains("XRAY.entities"))
                {
                    Logger.Log("Invalid X-Ray file.");
                    return;
                }
            }
            try
            {
                var ver = XRayUtil.CheckXRayVersion(selPath);
                if (ver == XRayUtil.XRayVersion.Invalid)
                {
                    Logger.Log("Invalid X-Ray file.");
                    return;
                }
                var terms = ver == XRayUtil.XRayVersion.New
                    ? XRayUtil.ExtractTermsNew(new SQLiteConnection($"Data Source={selPath}; Version=3;"), true)
                    : XRayUtil.ExtractTermsOld(selPath);
                
                frmPreviewXR frmXraPreview = new frmPreviewXR();
                frmXraPreview.PopulateXRay(terms.ToList());
                frmXraPreview.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private async void btnUnpack_Click(object sender, EventArgs e)
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(@"Specified book was not found.", @"Book Not Found");
                return;
            }
            if (_settings.useKindleUnpack && !File.Exists(_settings.mobi_unpack))
            {
                MessageBox.Show(@"Kindleunpack was not found.\r\nPlease review the settings page.", @"Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(_settings.outDir))
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
            if (_settings.useKindleUnpack)
            {
                Logger.Log("Running Kindleunpack to extract rawML...");
                try
                {
                    results = await Task.Run(() => Functions.GetMetaData(txtMobi.Text, _settings.outDir, randomFile, _settings.mobi_unpack)).ConfigureAwait(false);
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
                    results = await Task.Run(() => Functions.GetMetaDataInternal(txtMobi.Text, _settings.outDir, true, randomFile).getResults()).ConfigureAwait(false);
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
            Process.Start(AmazonUrl(txtAsin.Text));
        }

        private void btnExtractTerms_Click(object sender, EventArgs e)
        {
            string selPath = UIFunctions.GetFile("Open a Kindle X-Ray file...", "", "ASC files|*.asc", _settings.outDir);
            if (selPath == "" || !selPath.Contains("XRAY.entities"))
            {
                Logger.Log("Invalid or no file selected.");
                return;
            }
            var newVer = XRayUtil.CheckXRayVersion(selPath);
            if (newVer == XRayUtil.XRayVersion.Invalid)
            {
                Logger.Log("Invalid X-Ray file.");
                return;
            }
            try
            {
                var terms = newVer == XRayUtil.XRayVersion.New
                    ? XRayUtil.ExtractTermsNew(new SQLiteConnection($"Data Source={selPath}; Version=3;"), true)
                    : XRayUtil.ExtractTermsOld(selPath);
                if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                    Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
                string outfile = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(selPath) + ".xml";
                Functions.Save(terms.ToList(), outfile);
                Logger.Log("Character data has been successfully extracted and saved to: " + outfile);
            }
            catch (Exception ex)
            {
                Logger.Log("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
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
            _frmInfo.ShowDialog();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (openBook.Count == 3)
            {
                _frmCreator.txtAuthor.Text = openBook[0];
                _frmCreator.txtTitle.Text = openBook[1];
                _frmCreator.txtAsin.Text = openBook[2];
                _frmCreator.ShowDialog();
            }
            else
                _frmCreator.ShowDialog();
        }

        // TODO: Fix this mess
        private void checkFiles(string author, string title, string asin)
        {
            string bookOutputDir = OutputDirectory(author, Functions.RemoveInvalidFileChars(title), false);

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