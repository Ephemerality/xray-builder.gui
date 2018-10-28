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
using Newtonsoft.Json;
using SimpleInjector;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.Model;
using XRayBuilderGUI.Properties;
using XRayBuilderGUI.Unpack;

namespace XRayBuilderGUI.UI
{
    public partial class frmMain : Form
    {
        // TODO: Remove logging from classes that shouldn't have it
        private readonly Logger _logger;
        // TODO: Find a better way to create new forms w/o requiring container outside composition root
        private readonly Container _diContainer;

        public bool Exiting;

        private string EaPath = "";
        private string SaPath = "";
        private string ApPath = "";
        private string XrPath = "";

        public frmMain(Logger logger, Container diContainer)
        {
            InitializeComponent();
            _progress = new ProgressBarCtrl(prgBar);
            var rtfLogger = new RtfLogger(txtOutput);
            _logger = logger;
            _diContainer = diContainer;
            _logger.LogEvent += rtfLogger.Log;
        }

        private readonly ToolTip _tooltip = new ToolTip();
        private readonly Settings _settings = Settings.Default;
        private readonly string _currentLog = $@"{Environment.CurrentDirectory}\log\{DateTime.Now:HH.mm.ss.dd.MM.yyyy}.txt";

        // TODO: Do something else for this
        public List<string> openBook = new List<string>();

        private readonly IProgressBar _progress;

        private CancellationTokenSource _cancelTokens = new CancellationTokenSource();
        private ISecondarySource _dataSource;

        public DialogResult SafeShow(string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return (DialogResult)Invoke(new Func<DialogResult>(() => MessageBox.Show(this, msg, caption, buttons, icon, def)));
        }

        public string OutputDirectory(string author, string title, bool create)
        {
            if (!_settings.useSubDirectories) return _settings.outDir;
            if (!Functions.ValidateFilename(author, title))
                _logger.Log("Warning: The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.");
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
            if (enabled == false && _cancelTokens.IsCancellationRequested)
            {
                _cancelTokens.Dispose();
                _cancelTokens = new CancellationTokenSource();
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
            txtXMLFile.Text = UIFunctions.GetFile(txtXMLFile.Text, "",  "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt");
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
                MessageBox.Show("No " + _dataSource.Name + " link was specified.", "Missing " + _dataSource.Name + " Link");
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

            prgBar.Value = 0;

            var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.saverawml, _logger));
            if (metadata == null)
                return;

            // Added author name to log output
            _logger.Log($"Book's {_dataSource.Name} URL: {txtGoodreads.Text}");
            if (_cancelTokens.IsCancellationRequested) return;
            _logger.Log("Attempting to build X-Ray...");

            //If AZW3 file use AZW3 offset, if checked. Checked by default.
            bool AZW3 = Path.GetExtension(txtMobi.Text) == ".azw3" && _settings.overrideOffset;
            _logger.Log("Offset: " + (AZW3 ? $"{_settings.offsetAZW3} (AZW3)" : _settings.offset.ToString()));

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            try
            {
                if (rdoGoodreads.Checked)
                    xray = new XRay(txtGoodreads.Text, metadata.DBName, metadata.UniqueID, metadata.ASIN, _dataSource, _logger,
                        AZW3 ? _settings.offsetAZW3 : _settings.offset, "", false);
                else
                    xray = new XRay(txtXMLFile.Text, metadata.DBName, metadata.UniqueID, metadata.ASIN, _dataSource, _logger,
                        AZW3 ? _settings.offsetAZW3 : _settings.offset, "");

                await Task.Run(() => xray.CreateXray(_progress, _cancelTokens.Token)).ConfigureAwait(false);

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
                    _logger.Log("Aliases file not found.");
                else
                {
                    xray.LoadAliases();
                    _logger.Log($"Character aliases read from {xray.AliasPath}.");
                }

                _logger.Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if (await Task.Run(() => xray.ExpandFromRawMl(metadata.GetRawMlStream(), SafeShow, _progress, _cancelTokens.Token, _settings.ignoresofthyphen, !_settings.useNewVersion)).ConfigureAwait(false) > 0)
                {
                    _logger.Log("Build canceled or error occurred while processing locations and chapters.");
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Build canceled.");
                return;
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred while building the X-Ray:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return;
            }

            _logger.Log("Saving X-Ray to file...");
            string outFolder;
            try
            {
                if (_settings.android)
                {
                    outFolder = _settings.outDir + @"\Android\" + metadata.ASIN;
                    Directory.CreateDirectory(outFolder);
                }
                else
                {
                    outFolder = OutputDirectory(metadata.Author, metadata.Title, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Failed to create output directory: " + ex.Message + "\r\n" + ex.StackTrace + "\r\nFiles will be placed in the default output directory.");
                outFolder = _settings.outDir;
            }
            var newPath = outFolder + "\\" + xray.XRayName(_settings.android);

            if (_settings.useNewVersion)
            {
                try
                {
                    xray.SaveToFileNew(newPath, _progress, _cancelTokens.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.Log("Building canceled.");
                    return;
                }
                catch (Exception ex)
                {
                    // TODO: Add option to retry maybe?
                    _logger.Log($"An error occurred while creating the new X-Ray database. Is it opened in another program?\r\n{ex.Message}");
                    return;
                }
                XrPath = outFolder + @"\XRAY.entities." + metadata.ASIN;

                //Save the new XRAY.ASIN.previewData file
                try
                {
                    string PdPath = outFolder + @"\XRAY." + metadata.ASIN + ".previewData";
                    xray.SavePreviewToFile(PdPath);
                    _logger.Log($"X-Ray previewData file created successfully!\r\nSaved to {PdPath}");
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("An error occurred saving the previewData file: {0}\r\n{1}", ex.Message, ex.StackTrace));
                }
            }
            else
            {
                xray.SaveToFileOld(newPath);
            }
            _logger.Log($"X-Ray file created successfully!\r\nSaved to {newPath}");

            checkFiles(metadata.Author, metadata.Title, metadata.ASIN);

            if (_settings.playSound)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
                player.Play();
            }

            metadata.Dispose();
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
                    MessageBox.Show($"No {_dataSource.Name} link was specified.", $"Missing {_dataSource.Name} Link");
                    return;
                }
                if (!txtGoodreads.Text.ToLower().Contains(_settings.dataSource.ToLower()))
                {
                    MessageBox.Show($"Invalid {_dataSource.Name} link was specified.\r\n"
                        + $"If you do not want to use {_dataSource.Name}, you can change the data source in Settings."
                        , $"Invalid {_dataSource.Name} Link");
                    return;
                }
            }
            if (_settings.realName.Trim().Length == 0 || _settings.penName.Trim().Length == 0)
            {
                MessageBox.Show(
                    "Both Real and Pen names are required for End Action\r\n" +
                    "file creation. This information allows you to rate this\r\n" +
                    "book on Amazon. Please review the settings page.",
                    "Amazon Customer Details Not found");
                return;
            }

            var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.saverawml, _logger));
            if (metadata == null)
                return;

            SetDatasourceLabels(); // Reset the dataSource for the new build process
            _logger.Log($"Book's {_dataSource.Name} URL: {txtGoodreads.Text}");
            try
            {
                BookInfo bookInfo = new BookInfo(metadata, txtGoodreads.Text);

                string outputDir = OutputDirectory(bookInfo.author, bookInfo.sidecarName, true);

                _logger.Log("Attempting to build Author Profile...");
                AuthorProfile ap = new AuthorProfile(_logger);
                if (!await ap.GenerateAsync(bookInfo, new AuthorProfile.Settings
                {
                    AmazonTld = _settings.amazonTLD,
                    Android = _settings.android,
                    OutDir = _settings.outDir,
                    SaveBio = _settings.saveBio,
                    UseNewVersion = _settings.useNewVersion,
                    UseSubDirectories = _settings.useSubDirectories
                }, _cancelTokens.Token)) return;
                SaPath = $@"{outputDir}\StartActions.data.{bookInfo.asin}.asc";
                ApPath = $@"{outputDir}\AuthorProfile.profile.{bookInfo.asin}.asc";
                _logger.Log("Attempting to build Start Actions and End Actions...");

                string AsinPrompt(string title, string author)
                {
                    var frmAsin = new frmASIN
                    {
                        Text = "Series Information",
                        lblTitle = {Text = title},
                        lblAuthor = {Text = author},
                        tbAsin = {Text = ""}
                    };
                    frmAsin.ShowDialog();
                    return frmAsin.tbAsin.Text;
                }

                EndActions ea = new EndActions(ap, bookInfo, metadata.RawMlSize, _dataSource, new EndActions.Settings
                {
                    AmazonTld = _settings.amazonTLD,
                    Android = _settings.android,
                    OutDir = _settings.outDir,
                    PenName = _settings.penName,
                    RealName = _settings.realName,
                    UseNewVersion = _settings.useNewVersion,
                    UseSubDirectories = _settings.useSubDirectories,
                    PromptAsin = _settings.promptASIN
                }, AsinPrompt, _logger);
                if (!await ea.Generate()) return;

                if (_settings.useNewVersion)
                {
                    await ea.GenerateNewFormatData(_progress, _cancelTokens.Token);

                    // TODO: Do the templates differently
                    Model.EndActions eaBase;
                    try
                    {
                        var template = File.ReadAllText(Environment.CurrentDirectory + @"\dist\BaseEndActions.json", Encoding.UTF8);
                        eaBase = JsonConvert.DeserializeObject<Model.EndActions>(template);
                    }
                    catch (FileNotFoundException)
                    {
                        _logger.Log(@"Unable to find dist\BaseEndActions.json, make sure it has been extracted!");
                        return;
                    }
                    catch (Exception e)
                    {
                        _logger.Log($@"An error occurred while loading dist\BaseEndActions.json (make sure any new versions have been extracted!)\r\n{e.Message}\r\n{e.StackTrace}");
                        return;
                    }

                    await ea.GenerateEndActionsFromBase(eaBase);

                    StartActions sa;
                    try
                    {
                        var template = File.ReadAllText(Environment.CurrentDirectory + @"\dist\BaseStartActions.json", Encoding.UTF8);
                        sa = JsonConvert.DeserializeObject<StartActions>(template);
                    }
                    catch (FileNotFoundException)
                    {
                        _logger.Log(@"Unable to find dist\BaseStartActions.json, make sure it has been extracted!");
                        return;
                    }
                    catch (Exception e)
                    {
                        _logger.Log($@"An error occurred while loading dist\BaseStartActions.json (make sure any new versions have been extracted!)\r\n{e.Message}\r\n{e.StackTrace}");
                        return;
                    }

                    // TODO: Separate out SA logic
                    string saContent = null;
                    if (_settings.downloadSA)
                    {
                        _logger.Log("Attempting to download Start Actions...");
                        try
                        {
                            saContent = await Amazon.DownloadStartActions(metadata.ASIN);
                        }
                        catch
                        {
                            _logger.Log("No pre-made Start Actions available, building...");
                        }
                    }
                    if (string.IsNullOrEmpty(saContent))
                        saContent = ea.GenerateStartActionsFromBase(sa);
                    ea.WriteStartActions(saContent);

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
                _logger.Log("An error occurred while creating the new Author Profile, Start Actions, and/or End Actions files:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                metadata.Dispose();
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

                XRay xray = new XRay(txtGoodreads.Text, _dataSource, _logger);
                int result = await Task.Run(() => xray.SaveXml(path, _progress, _cancelTokens.Token));
                if (result == 1)
                    _logger.Log("Warning: Unable to download character data as no character data found on Goodreads.");
                else if (result == 2)
                    _logger.Log("Download cancelled.");
                else
                    _logger.Log("Character data has been successfully saved to: " + path);
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred while saving character data to XML: {ex.Message}\r\nPath was: {path}");
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
            if (!Directory.Exists(_settings.outDir))
            {
                MessageBox.Show("Specified output directory does not exist. Please review the settings page.",
                    "Output Directory Not found");
                return;
            }

            //this.TopMost = true;
            using (var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, false, _logger)))
            {
                if (metadata == null)
                    return;

                try
                {
                    List<BookInfo> books = (await _dataSource.SearchBookAsync(metadata.Author, metadata.Title)).ToList();
                    string bookUrl;
                    if (books.Count > 1)
                    {
                        _logger.Log($"Warning: Multiple results returned from {_dataSource.Name}...");
                        var frmG = new frmGR(_logger) { BookList = books };
                        frmG.ShowDialog();
                        bookUrl = books[frmG.cbResults.SelectedIndex].dataUrl;
                    }
                    else if (books.Count == 1)
                        bookUrl = books[0].dataUrl;
                    else
                    {
                        _logger.Log($"Unable to find this book on {_dataSource.Name}!");
                        return;
                    }

                    if (!string.IsNullOrEmpty(bookUrl))
                    {
                        txtGoodreads.Text = bookUrl;
                        txtGoodreads.Refresh();
                        _logger.Log(
                            $"Book found on {_dataSource.Name}!\r\n{metadata.Title} by {metadata.Author}\r\n{_dataSource.Name} URL: {bookUrl}\r\n"
                            + "You may want to visit the URL to ensure it is correct.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log("An error occurred while searching: " + ex.Message + "\r\n" + ex.StackTrace);
                }
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
            Exiting = true;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

            foreach (var dir in new [] { "out", "log", "dmp", "tmp" })
                Directory.CreateDirectory(Environment.CurrentDirectory + $"\\{dir}");

            if (_settings.outDir == "")
                _settings.outDir = Environment.CurrentDirectory + @"\out";

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
                _dataSource = new Goodreads(_logger);
                rdoGoodreads.Text = "Goodreads";
                lblGoodreads.Text = "Goodreads URL:";
                lblGoodreads.Left = 134;
                _tooltip.SetToolTip(btnDownloadTerms, "Save Goodreads info to an XML file.");
                _tooltip.SetToolTip(btnSearchGoodreads, "Try to search for this book on Goodreads.");
            }
            else
            {
                btnSearchGoodreads.Enabled = false;
                _dataSource = new Shelfari(_logger);
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
            foreach (var fileLoc in filePaths.Where(File.Exists))
            {
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
                btnKindleExtras.Enabled = !btnKindleExtras.Enabled;
                btnOneClick.Enabled = !btnOneClick.Enabled;
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


            var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, false, _logger));
            if (metadata == null)
            {
                txtMobi.Text = "";
                return;
            }
            metadata.CheckDRM();
            pbCover.Image = (Image) metadata.coverImage?.Clone();

            lblTitle.Visible = true;
            lblAuthor.Visible = true;
            lblAsin.Visible = true;
            txtTitle.Visible = true;
            txtAuthor.Visible = true;
            txtAsin.Visible = true;

            txtAuthor.Text = metadata.Author;
            txtTitle.Text = metadata.Title;
            txtAsin.Text = metadata.ASIN;
            _tooltip.SetToolTip(txtAsin, AmazonUrl(txtAsin.Text));

            openBook.Clear();
            openBook.Add(metadata.Author);
            openBook.Add(metadata.Title);
            openBook.Add(metadata.ASIN);

            checkFiles(metadata.Author, metadata.Title, metadata.ASIN);

            try
            {
                // Directory.Delete(randomFile, true);
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred while trying to delete temporary files: {ex.Message}\r\n{ex.StackTrace}\r\n"
                    + "Try deleting these files manually.");
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            cmsPreview.Show(btnPreview, new Point(2, btnPreview.Height));
        }

        private async void tmiAuthorProfile_Click(object sender, EventArgs e)
        {
            await UIFunctions.ShowPreview(Filetype.AuthorProfile, ApPath, _settings.outDir, _logger);
        }

        private async void tmiStartAction_Click(object sender, EventArgs e)
        {
            await UIFunctions.ShowPreview(Filetype.StartActions, ApPath, _settings.outDir, _logger);
        }

        private async void tmiEndAction_Click(object sender, EventArgs e)
        {
            await UIFunctions.ShowPreview(Filetype.EndActions, ApPath, _settings.outDir, _logger);
        }

        private async void tmiXray_Click(object sender, EventArgs e)
        {
            await UIFunctions.ShowPreview(Filetype.XRay, ApPath, _settings.outDir, _logger);
        }

        private async void btnUnpack_Click(object sender, EventArgs e)
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(@"Specified book was not found.", @"Book Not Found");
                return;
            }
            if (!Directory.Exists(_settings.outDir))
            {
                MessageBox.Show(@"Specified output directory does not exist.\r\nPlease review the settings page.", @"Output Directory Not found");
                return;
            }
            var metadata = await Task.Run(() => new Metadata(txtMobi.Text)).ConfigureAwait(false);
            if (metadata != null)
            {
                _logger.Log("Extracted rawml successfully!\r\n");
                metadata.Dispose();
            }
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
                _logger.Log("Invalid or no file selected.");
                return;
            }
            var newVer = XRayUtil.CheckXRayVersion(selPath);
            if (newVer == XRayUtil.XRayVersion.Invalid)
            {
                _logger.Log("Invalid X-Ray file.");
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
                _logger.Log("Character data has been successfully extracted and saved to: " + outfile);
            }
            catch (Exception ex)
            {
                _logger.Log("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
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
            new frmAbout().ShowDialog();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var frmCreateXr = new frmCreateXR();
            if (openBook.Count == 3)
            {
                frmCreateXr.txtAuthor.Text = openBook[0];
                frmCreateXr.txtTitle.Text = openBook[1];
                frmCreateXr.txtAsin.Text = openBook[2];
            }
            frmCreateXr.ShowDialog();
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
            if (!_cancelTokens.IsCancellationRequested)
            {
                _logger.Log("Canceling...");
                _cancelTokens.Cancel();
            }
        }
    }
}