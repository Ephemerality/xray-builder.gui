using System;
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
using XRayBuilderGUI.Extras.Artifacts;
using XRayBuilderGUI.Extras.AuthorProfile;
using XRayBuilderGUI.Model;
using XRayBuilderGUI.Properties;
using XRayBuilderGUI.UI.Preview.Logic;
using XRayBuilderGUI.Unpack;

namespace XRayBuilderGUI.UI
{
    public partial class frmMain : Form
    {
        // TODO: Remove logging from classes that shouldn't have it
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;
        private readonly IAuthorProfileGenerator _authorProfileGenerator;
        private readonly PreviewProviderFactory _previewProviderFactory;
        private readonly IAmazonInfoParser _amazonInfoParser;
        private readonly Container _diContainer;

        // TODO: Fix up these paths
        private string EaPath = "";
        private string SaPath = "";
        private string ApPath = "";
        private string XrPath = "";

        public frmMain(
            ILogger logger,
            IHttpClient httpClient,
            Container diContainer,
            IAuthorProfileGenerator authorProfileGenerator,
            IAmazonClient amazonClient,
            PreviewProviderFactory previewProviderFactory,
            IAmazonInfoParser amazonInfoParser)
        {
            InitializeComponent();
            _progress = new ProgressBarCtrl(prgBar);
            var rtfLogger = new RtfLogger(txtOutput);
            _logger = logger;
            _diContainer = diContainer;
            _authorProfileGenerator = authorProfileGenerator;
            _amazonClient = amazonClient;
            _previewProviderFactory = previewProviderFactory;
            _amazonInfoParser = amazonInfoParser;
            _logger.LogEvent += rtfLogger.Log;
            _httpClient = httpClient;
        }

        private readonly ToolTip _tooltip = new ToolTip();
        private readonly Settings _settings = Settings.Default;
        private readonly string _currentLog = $@"{Environment.CurrentDirectory}\log\{DateTime.Now:HH.mm.ss.dd.MM.yyyy}.txt";

        private readonly IProgressBar _progress;

        private CancellationTokenSource _cancelTokens = new CancellationTokenSource();
        private ISecondarySource _dataSource;

        public DialogResult SafeShow(string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return (DialogResult)Invoke(new Func<DialogResult>(() => MessageBox.Show(this, msg, caption, buttons, icon, def)));
        }

        // todo consolidate output path building
        public string OutputDirectory(string author, string title, string asin, string fileName, bool create)
        {
            var outputDir = "";

            if (_settings.android)
                outputDir = $@"{_settings.outDir}\Android\{asin}";
            else if (!_settings.useSubDirectories)
                outputDir = _settings.outDir;

            if (!Functions.ValidateFilename(author, title))
                _logger.Log("Warning: The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.");

            if (string.IsNullOrEmpty(outputDir))
                outputDir = Functions.GetBookOutputDirectory(author, title, create);

            if (_settings.outputToSidecar)
                outputDir = Path.Combine(outputDir, $"{fileName}.sdr");

            if (create)
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    _logger.Log("An error occurred creating output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                    outputDir = _settings.outDir;
                }
            }

            return outputDir;
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

            // todo this is crap
            var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, _settings.saverawml, _logger));
            if (metadata == null)
                return;

            // Added author name to log output
            _logger.Log($"Book's {_dataSource.Name} URL: {txtGoodreads.Text}");
            if (_cancelTokens.IsCancellationRequested) return;
            _logger.Log("Attempting to build X-Ray...");

            //If AZW3 file use AZW3 offset, if checked. Checked by default.
            var AZW3 = Path.GetExtension(txtMobi.Text) == ".azw3" && _settings.overrideOffset;
            _logger.Log("Offset: " + (AZW3 ? $"{_settings.offsetAZW3} (AZW3)" : _settings.offset.ToString()));

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            try
            {
                if (rdoGoodreads.Checked)
                    xray = new XRay(txtGoodreads.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _dataSource, _logger,
                        AZW3 ? _settings.offsetAZW3 : _settings.offset, "", false);
                else
                    xray = new XRay(txtXMLFile.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _dataSource, _logger,
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
                    outFolder = _settings.outDir + @"\Android\" + metadata.Asin;
                    Directory.CreateDirectory(outFolder);
                }
                else
                {
                    outFolder = OutputDirectory(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), true);
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
                XrPath = outFolder + @"\XRAY.entities." + metadata.Asin;

                //Save the new XRAY.ASIN.previewData file
                try
                {
                    var PdPath = outFolder + @"\XRAY." + metadata.Asin + ".previewData";
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

            checkFiles(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text));

            if (_settings.playSound)
            {
                var player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
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
                var bookInfo = new BookInfo(metadata, txtGoodreads.Text, txtMobi.Text);

                var outputDir = OutputDirectory(bookInfo.Author, bookInfo.Title, bookInfo.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), true);

                _logger.Log("Attempting to build Author Profile...");

                ApPath = $@"{outputDir}\AuthorProfile.profile.{bookInfo.Asin}.asc";

                // TODO: Load existing ap to use for end actions / start actions
                if (!Settings.Default.overwrite && File.Exists(ApPath))
                {
                    _logger.Log("AuthorProfile file already exists... Skipping!\r\n" +
                                "Please review the settings page if you want to overwite any existing files.");
                    return;
                }

                var response = await _authorProfileGenerator.GenerateAsync(new AuthorProfileGenerator.Request
                {
                    Book = bookInfo,
                    Settings = new AuthorProfileGenerator.Settings
                    {
                        AmazonTld = _settings.amazonTLD,
                        SaveBio = _settings.saveBio,
                        UseNewVersion = _settings.useNewVersion,
                        EditBiography = _settings.editBiography
                    }
                }, _cancelTokens.Token);

                if (response == null)
                    return;

                var authorProfileOutput = JsonConvert.SerializeObject(AuthorProfileGenerator.CreateAp(response, bookInfo.Asin));

                try
                {
                    File.WriteAllText(ApPath, authorProfileOutput);
                    _logger.Log("Author Profile file created successfully!\r\nSaved to " + ApPath);
                }
                catch (Exception ex)
                {
                    _logger.Log("An error occurred while writing the Author Profile file: " + ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }

                SaPath = $@"{outputDir}\StartActions.data.{bookInfo.Asin}.asc";
                _logger.Log("Attempting to build Start Actions and End Actions...");

                string AsinPrompt(string title, string author)
                {
                    var frmAsin = _diContainer.GetInstance<frmASIN>();
                    frmAsin.Text = "Series Information";
                    frmAsin.lblTitle.Text = title;
                    frmAsin.lblAuthor.Text = author;
                    frmAsin.tbAsin.Text = "";
                    frmAsin.ShowDialog();
                    return frmAsin.tbAsin.Text;
                }

                var ea = new EndActions(response, bookInfo, metadata.RawMlSize, _dataSource, new EndActions.Settings
                {
                    AmazonTld = _settings.amazonTLD,
                    Android = _settings.android,
                    OutDir = _settings.outDir,
                    OutputToSidecar = _settings.outputToSidecar,
                    PenName = _settings.penName,
                    RealName = _settings.realName,
                    UseNewVersion = _settings.useNewVersion,
                    UseSubDirectories = _settings.useSubDirectories,
                    PromptAsin = _settings.promptASIN
                }, AsinPrompt, _logger, _httpClient, _amazonClient, _amazonInfoParser);
                if (!await ea.Generate()) return;

                if (_settings.useNewVersion)
                {
                    await ea.GenerateNewFormatData(_progress, _cancelTokens.Token);

                    // TODO: Do the templates differently
                    Extras.Artifacts.EndActions eaBase;
                    try
                    {
                        var template = File.ReadAllText(Environment.CurrentDirectory + @"\dist\BaseEndActions.json", Encoding.UTF8);
                        eaBase = JsonConvert.DeserializeObject<Extras.Artifacts.EndActions>(template);
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
                            saContent = await _amazonClient.DownloadStartActions(metadata.Asin);
                            _logger.Log("Successfully downloaded pre-made Start Actions!");
                        }
                        catch
                        {
                            _logger.Log("No pre-made Start Actions available, building...");
                        }
                    }
                    if (string.IsNullOrEmpty(saContent))
                        saContent = ea.GenerateStartActionsFromBase(sa);

                    _logger.Log("Writing StartActions to file...");
                    File.WriteAllText(ea.SaPath, saContent);
                    _logger.Log("StartActions file created successfully!\r\nSaved to " + SaPath);

                    cmsPreview.Items[3].Enabled = true;
                    EaPath = $@"{outputDir}\EndActions.data.{bookInfo.Asin}.asc";
                }
                else
                    ea.GenerateOld();

                cmsPreview.Items[1].Enabled = true;

                checkFiles(bookInfo.Author, bookInfo.Title, bookInfo.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text));
                if (_settings.playSound)
                {
                    var player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\done.wav");
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
            var path = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(txtMobi.Text) + ".xml";
            try
            {
                txtXMLFile.Text = path;

                var xray = new XRay(txtGoodreads.Text, _dataSource, _logger);
                var result = await Task.Run(() => xray.SaveXml(path, _progress, _cancelTokens.Token));
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
                    var books = new BookInfo[0];
                    if (_settings.searchByAsin)
                        books = (await _dataSource.SearchBookByAsinAsync(metadata.Asin)).ToArray();

                    if (books.Length <= 0)
                    {
                        books = (await _dataSource.SearchBookAsync(metadata.Author, metadata.Title)).ToArray();
                        if (books.Length <= 0)
                        {
                            _logger.Log($"Unable to find this book on {_dataSource.Name}!\nEnsure the book's title ({metadata.Title}) is accurate!");
                            return;
                        }
                    }

                    string bookUrl;
                    if (books.Length == 1)
                        bookUrl = books[0].DataUrl;
                    else
                    {
                        books = books.OrderByDescending(book => book.Reviews)
                            .ThenByDescending(book => book.Editions)
                            .ToArray();

                        // Pre-load cover images
                        foreach (var book in books.Where(book => !string.IsNullOrEmpty(book.ImageUrl)))
                        {
                            try
                            {
                                book.CoverImage = await _httpClient.GetImageAsync(book.ImageUrl, cancellationToken: _cancelTokens.Token);
                            }
                            catch (Exception ex)
                            {
                                _logger.Log("Failed to download cover image: " + ex.Message);
                            }
                        }

                        _logger.Log($"Warning: Multiple results returned from {_dataSource.Name}...");
                        var frmG = new frmGR(books);
                        frmG.ShowDialog();
                        bookUrl = books[frmG.cbResults.SelectedIndex].DataUrl;
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
            var frmSet = new frmSettings();
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

            var args = Environment.GetCommandLineArgs();

            txtMobi.Text = args.Skip(1).Where(File.Exists).Select(Path.GetFullPath).FirstOrDefault()
                           ?? _settings.mobiFile;

            if (txtXMLFile.Text == "") txtXMLFile.Text = _settings.xmlFile;

            // TODO: Maybe do something about these paths
            // TODO: ExtLoader or something?
            foreach (var dir in new [] { "out", "log", "dmp", "tmp", "ext" })
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
            // todo: use enum directly for setting - consider passing enum vs datasource
            var datasource = (SecondaryDataSourceFactory.Enum) Enum.Parse(typeof(SecondaryDataSourceFactory.Enum), _settings.dataSource);

            _dataSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(datasource);
            btnSearchGoodreads.Enabled = _dataSource.SearchEnabled;
            lblGoodreads.Left = _dataSource.UrlLabelPosition;
            rdoGoodreads.Text = _dataSource.Name;
            lblGoodreads.Text = $"{_dataSource.Name} URL:";
            _tooltip.SetToolTip(btnDownloadTerms, $"Save {_dataSource.Name} info to an XML file.");
            _tooltip.SetToolTip(btnSearchGoodreads, _dataSource.SearchEnabled
                ? $"Try to search for this book on {_dataSource.Name}."
                : $"Search is disabled when {_dataSource.Name} is selected as a data source.");
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
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

        private void txtMobi_TextChanged(object sender, EventArgs e)
        {
            if (txtMobi.Text == "" || !File.Exists(txtMobi.Text)) return;
            txtGoodreads.Text = "";
            prgBar.Value = 0;

            var metadata = UIFunctions.GetAndValidateMetadata(txtMobi.Text, false, _logger);
            if (metadata == null)
            {
                txtMobi.Text = "";
                return;
            }
            metadata.CheckDrm();
            pbCover.Image = (Image) metadata.CoverImage?.Clone();

            lblTitle.Visible = true;
            lblAuthor.Visible = true;
            lblAsin.Visible = true;
            txtTitle.Visible = true;
            txtAuthor.Visible = true;
            txtAsin.Visible = true;

            txtAuthor.Text = metadata.Author;
            txtTitle.Text = metadata.Title;
            txtAsin.Text = metadata.Asin;
            _tooltip.SetToolTip(txtAsin, _amazonClient.Url(_settings.amazonTLD, txtAsin.Text));

            checkFiles(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text));
            btnBuild.Enabled = metadata.RawMlSupported;
            btnOneClick.Enabled = metadata.RawMlSupported;

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
            await ShowPreview(PreviewProviderFactory.PreviewType.AuthorProfile, ApPath);
        }

        private async void tmiStartAction_Click(object sender, EventArgs e)
        {
            await ShowPreview(PreviewProviderFactory.PreviewType.StartActions, SaPath);
        }

        private async void tmiEndAction_Click(object sender, EventArgs e)
        {
            await ShowPreview(PreviewProviderFactory.PreviewType.EndActions, EaPath);
        }

        private async void tmiXray_Click(object sender, EventArgs e)
        {
            await ShowPreview(PreviewProviderFactory.PreviewType.XRay, XrPath);
        }

        private void btnUnpack_Click(object sender, EventArgs e)
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

            _logger.Log("Extracting raw markup...");
            using (var metadata = MetadataLoader.Load(txtMobi.Text))
            {
                var rawMlPath = UIFunctions.RawMlPath(Path.GetFileNameWithoutExtension(txtMobi.Text));
                metadata.SaveRawMl(rawMlPath);
                _logger.Log($"Extracted to {rawMlPath}!\r\n");
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
            Process.Start(_amazonClient.Url(_settings.amazonTLD, txtAsin.Text));
        }

        private void btnExtractTerms_Click(object sender, EventArgs e)
        {
            var selPath = UIFunctions.GetFile("Open a Kindle X-Ray file...", "", "ASC files|*.asc", _settings.outDir);
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
                var outfile = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(selPath) + ".xml";
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

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            var frmCreateXr = new frmCreateXR();
            var metadata = await Task.Run(() => UIFunctions.GetAndValidateMetadata(txtMobi.Text, false, _logger));
            if (metadata != null)
            {
                frmCreateXr.txtAuthor.Text = metadata.Author;
                frmCreateXr.txtTitle.Text = metadata.Title;
                frmCreateXr.txtAsin.Text = metadata.Asin;
            }
            frmCreateXr.ShowDialog();
        }

        // TODO: Fix this mess
        private void checkFiles(string author, string title, string fileName, string asin)
        {
            var bookOutputDir = OutputDirectory(author, Functions.RemoveInvalidFileChars(title), asin, fileName, false);

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

        private async Task ShowPreview(PreviewProviderFactory.PreviewType type, string filePath)
        {
            var previewProvider = _previewProviderFactory.Get(type);

            string selPath;
            if (File.Exists(filePath))
                selPath = filePath;
            else
            {
                selPath = UIFunctions.GetFile($"Open a Kindle {previewProvider.Name} file...", "", "ASC files|*.asc", _settings.outDir);
                if (!selPath.Contains(previewProvider.FilenameValidator))
                {
                    _logger.Log($"Invalid {previewProvider.Name} file.");
                    return;
                }
            }

            try
            {
                var previewForm = previewProvider.GenForm();
                await previewForm.Populate(selPath, _cancelTokens.Token);
                previewForm.ShowDialog();

            }
            catch (Exception ex)
            {
                _logger.Log("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}