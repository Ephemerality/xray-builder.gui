﻿using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using SimpleInjector;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Logic;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Extras.EndActions;
using XRayBuilder.Core.Extras.StartActions;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.Unpack.KFX;
using XRayBuilder.Core.Unpack.Mobi;
using XRayBuilder.Core.XRay;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Export;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Core.XRay.Model.Export;
using XRayBuilder.Core.XRay.Util;
using XRayBuilderGUI.Properties;
using XRayBuilderGUI.UI.Preview.Model;

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
        private readonly IAliasesRepository _aliasesRepository;
        private readonly IXRayService _xrayService;
        private readonly XRayExporterFactory _xrayExporterFactory;
        private readonly IPreviewDataExporter _previewDataExporter;
        private readonly ITermsService _termsService;
        private readonly Container _diContainer;
        // TODO Different type handling should come from some sort of factory or whatever
        private readonly IKfxXrayService _kfxXrayService;
        private readonly IStartActionsArtifactService _startActionsArtifactService;
        private readonly IEndActionsArtifactService _endActionsArtifactService;
        private readonly IRoentgenClient _roentgenClient;
        private readonly IEndActionsAuthorConverter _endActionsAuthorConverter;
        private readonly IDirectoryService _directoryService;

        public frmMain(
            ILogger logger,
            IHttpClient httpClient,
            Container diContainer,
            IAuthorProfileGenerator authorProfileGenerator,
            IAmazonClient amazonClient,
            PreviewProviderFactory previewProviderFactory,
            IAliasesRepository aliasesRepository,
            IPreviewDataExporter previewDataExporter,
            XRayExporterFactory xrayExporterFactory,
            IXRayService xrayService,
            ITermsService termsService,
            IKfxXrayService kfxXrayService,
            IStartActionsArtifactService startActionsArtifactService,
            IEndActionsArtifactService endActionsArtifactService,
            IRoentgenClient roentgenClient,
            IEndActionsAuthorConverter endActionsAuthorConverter,
            IDirectoryService directoryService)
        {
            InitializeComponent();
            _progress = new ProgressBarCtrl(prgBar);
            var rtfLogger = new RtfLogger(txtOutput);
            _logger = logger;
            _diContainer = diContainer;
            _authorProfileGenerator = authorProfileGenerator;
            _amazonClient = amazonClient;
            _previewProviderFactory = previewProviderFactory;
            _aliasesRepository = aliasesRepository;
            _previewDataExporter = previewDataExporter;
            _xrayExporterFactory = xrayExporterFactory;
            _xrayService = xrayService;
            _termsService = termsService;
            _kfxXrayService = kfxXrayService;
            _startActionsArtifactService = startActionsArtifactService;
            _endActionsArtifactService = endActionsArtifactService;
            _roentgenClient = roentgenClient;
            _endActionsAuthorConverter = endActionsAuthorConverter;
            _directoryService = directoryService;
            _logger.LogEvent += rtfLogger.Log;
            _httpClient = httpClient;
        }

        private readonly ToolTip _tooltip = new ToolTip();
        private readonly Settings _settings = Settings.Default;
        private readonly string _currentLog = $@"{Environment.CurrentDirectory}\log\{DateTime.Now:HH.mm.ss.dd.MM.yyyy}.txt";

        private readonly IProgressBar _progress;

        private CancellationTokenSource _cancelTokens = new CancellationTokenSource();
        private ISecondarySource _dataSource;

        private IMetadata _openedMetadata;

        private DialogResult SafeShow(string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return (DialogResult)Invoke(new Func<DialogResult>(() => MessageBox.Show(this, msg, caption, buttons, icon, def)));
        }

        private void ToggleInterface(bool enabled)
        {
            foreach (var c in Controls.OfType<Button>())
                c.Enabled = enabled;
            txtMobi.Enabled = enabled;
            txtXMLFile.Enabled = enabled && rdoFile.Checked;
            txtGoodreads.Enabled = enabled;
            rdoFile.Enabled = enabled;
            rdoGoodreads.Enabled = enabled;
            rdoRoentgen.Enabled = enabled;
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

        private async Task<IMetadata> GetAndValidateMetadataAsync(string mobiFile, bool saveRawMl, CancellationToken cancellationToken)
        {
            _logger.Log("Extracting metadata...");
            try
            {
                var metadata = MetadataLoader.Load(mobiFile);
                UIFunctions.EbokTagPromptOrThrow(metadata, mobiFile);
                try
                {
                    await CheckAndFixIncorrectAsinOrThrowAsync(metadata, mobiFile, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failed to validate/fix ASIN: {ex.Message}\r\nContinuing anyway...", LogLevel.Error);
                }

                if (!Settings.Default.useNewVersion && metadata.DbName.Length == 31)
                {
                    MessageBox.Show($"WARNING: Database Name is the maximum length. If \"{metadata.DbName}\" is the full book title, this should not be an issue.\r\nIf the title is supposed to be longer than that, you may get an error on your Kindle (WG on firmware < 5.6).\r\nThis can be resolved by either shortening the title in Calibre or manually changing the database name.\r\n");
                }

                if (saveRawMl && metadata.RawMlSupported)
                {
                    _logger.Log("Saving rawML to dmp directory...");
                    metadata.SaveRawMl(UIFunctions.RawMlPath(Path.GetFileNameWithoutExtension(mobiFile)));
                }
                _logger.Log($"Got metadata!\r\nDatabase Name: {metadata.DbName}\r\nUniqueID: {metadata.UniqueId}\r\nASIN: {metadata.Asin}");

                _openedMetadata = metadata;
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred extracting metadata: {ex.Message}\r\n{ex.StackTrace}");
            }

            return null;
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
                MessageBox.Show($"No {_dataSource.Name} link was specified.", $"Missing {_dataSource.Name} Link");
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

            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, _settings.saverawml, _cancelTokens.Token);
            if (metadata == null)
                return;

            // Added author name to log output
            _logger.Log($"Book's {_dataSource.Name} URL: {txtGoodreads.Text}");
            if (_cancelTokens.IsCancellationRequested) return;
            _logger.Log("Attempting to build X-Ray...");

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            try
            {
                Task<XRay> xrayTask;
                if (rdoGoodreads.Checked)
                    xrayTask = _xrayService.CreateXRayAsync(txtGoodreads.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _settings.amazonTLD, _settings.includeTopics, _dataSource, _progress, _cancelTokens.Token);
                else if (rdoRoentgen.Checked)
                    xrayTask = _xrayService.CreateXRayAsync(txtGoodreads.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _settings.roentgenRegion, _settings.includeTopics, _diContainer.GetInstance<SecondarySourceRoentgen>(), _progress, _cancelTokens.Token);
                else
                {
                    // TODO Set datasource properly
                    var fileDataSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(SecondaryDataSourceFactory.Enum.File);
                    xrayTask = _xrayService.CreateXRayAsync(txtXMLFile.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _settings.amazonTLD, _settings.includeTopics, fileDataSource, _progress, _cancelTokens.Token);
                }

                xray = await Task.Run(() => xrayTask).ConfigureAwait(false);

                if (xray.Terms.Count == 0
                    && DialogResult.No == MessageBox.Show("No terms were available, do you want to continue the build anyway?", "No Terms", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                {
                    _logger.Log("Cancelling...");
                    return;
                }

                _xrayService.ExportAndDisplayTerms(xray, xray.AliasPath, _settings.overwriteAliases, _settings.splitAliases);

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
                if (xray.Terms.Any(term => term.Aliases?.Count > 0))
                    _logger.Log("Character aliases read from the XML file.");
                else if (!File.Exists(xray.AliasPath))
                    _logger.Log("Aliases file not found.");
                else
                {
                    _aliasesRepository.LoadAliasesForXRay(xray);
                    _logger.Log($"Character aliases read from {xray.AliasPath}.");
                }

                _logger.Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                Task buildTask;
                switch (metadata)
                {
                    case Metadata _:
                        // ReSharper disable twice AccessToDisposedClosure
                        buildTask = Task.Run(() => _xrayService.ExpandFromRawMl(xray, metadata, metadata.GetRawMlStream(), _settings.enableEdit, _settings.useNewVersion, _settings.skipNoLikes, _settings.minClipLen, _settings.overwriteChapters, SafeShow, _progress, _cancelTokens.Token, _settings.ignoresofthyphen, !_settings.useNewVersion));
                        break;
                    case KfxContainer kfx:
                        if (!_settings.useNewVersion)
                            throw new Exception("Building the old format of X-Ray is not supported with KFX books");

                        buildTask = Task.Run(() => _kfxXrayService.AddLocations(xray, kfx, _settings.skipNoLikes, _settings.minClipLen, _progress, _cancelTokens.Token));
                        break;
                    default:
                        throw new NotSupportedException();
                }
                await buildTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Build canceled.");
                return;
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred while building the X-Ray:\r\n{ex.Message}\r\n{ex.StackTrace}");
                return;
            }

            _logger.Log("Saving X-Ray to file...");
            var xrayPath = _directoryService.GetArtifactPath(ArtifactType.XRay, metadata, Path.GetFileNameWithoutExtension(txtMobi.Text), true);

            try
            {
                var xrayExporter = _xrayExporterFactory.Get(_settings.useNewVersion ? XRayExporterFactory.Enum.Sqlite : XRayExporterFactory.Enum.Json);
                xrayExporter.Export(xray, xrayPath, _progress, _cancelTokens.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Building canceled.");
                return;
            }
            catch (Exception ex)
            {
                // TODO: Add option to retry maybe?
                _logger.Log($"An error occurred while creating the X-Ray file. Is it opened in another program?\r\n{ex.Message}");
                return;
            }

            if (_settings.useNewVersion)
            {
                //Save the new XRAY.ASIN.previewData file
                try
                {
                    var pdPath = _directoryService.GetArtifactPath(ArtifactType.XRayPreview, metadata, Path.GetFileNameWithoutExtension(txtMobi.Text), true);
                    _previewDataExporter.Export(xray, pdPath);
                    _logger.Log($"X-Ray previewData file created successfully!\r\nSaved to {pdPath}");
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred saving the previewData file: {ex.Message}\r\n{ex.StackTrace}");
                }
            }

            _logger.Log($"X-Ray file created successfully!\r\nSaved to {xrayPath}");

            CheckFiles(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), metadata.DbName, metadata.Guid);

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
            if (_settings.realName.Trim().Length == 0 || _settings.penName.Trim().Length == 0)
            {
                MessageBox.Show(
                    "Both Real and Pen names are required for End Action\r\n" +
                    "file creation. This information allows you to rate this\r\n" +
                    "book on Amazon. Please review the settings page.",
                    "Amazon Customer Details Not found");
                return;
            }

            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, _settings.saverawml, _cancelTokens.Token);
            if (metadata == null)
                return;

            SetDatasourceLabels(); // Reset the dataSource for the new build process
            _logger.Log($"Book's {_dataSource.Name} URL: {txtGoodreads.Text}");
            try
            {
                var bookInfo = new BookInfo(metadata, txtGoodreads.Text);

                var outputDir = _directoryService.GetDirectory(bookInfo.Author, bookInfo.Title, bookInfo.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), true);

                // TODO path stuff is still ugly
                var apPath = Path.Combine(outputDir, _directoryService.GetArtifactFilename(ArtifactType.AuthorProfile, bookInfo.Asin, metadata.DbName, metadata.Guid));
                var saPath = Path.Combine(outputDir, _directoryService.GetArtifactFilename(ArtifactType.StartActions, bookInfo.Asin, metadata.DbName, metadata.Guid));
                var eaPath = Path.Combine(outputDir, _directoryService.GetArtifactFilename(ArtifactType.EndActions, bookInfo.Asin, metadata.DbName, metadata.Guid));
                var saExists = File.Exists(saPath);
                var eaExists = File.Exists(eaPath);
                var apExists = File.Exists(apPath);
                var needSa = !saExists || _settings.overwriteSA;
                var needEa = !eaExists || _settings.overwriteEA;
                var needAp = !apExists || _settings.overwriteAP;
                if (!needAp && !needSa && !needEa)
                {
                    _logger.Log("All extras files already exist and none of the \"overwrite\" settings are enabled!\r\nCanceling the build process...");
                    return;
                }

                async Task<TActions> DownloadActionsArtifact<TActions>(string type, Func<string, string, CancellationToken, Task<TActions>> download) where TActions : class
                {
                    _logger.Log($"Attempting to download {type} Actions...");
                    try
                    {
                        var actions = await download(metadata.Asin, _settings.roentgenRegion, _cancelTokens.Token);
                        if (actions == null)
                        {
                            _logger.Log($"No pre-made {type} Actions available, one will be built instead...");
                            return null;
                        }

                        _logger.Log($"Successfully downloaded pre-made {type} Actions!");
                        return actions;
                    }
                    catch (Exception e)
                    {
                        _logger.Log($"No pre-made {type} Actions available (message: {e.Message}), one will be built instead...");
                        return null;
                    }
                }

                if (_settings.downloadSA && needSa)
                {
                    // todo fix duplication for saving
                    try
                    {
                        var startActions = await DownloadActionsArtifact("Start", _roentgenClient.DownloadStartActionsAsync);
                        if (startActions != null)
                        {
                            _logger.Log("Writing Start Actions to file...");
                            File.WriteAllText(saPath, Functions.ExpandUnicode(JsonConvert.SerializeObject(startActions)));
                            _logger.Log($"Start Actions file created successfully!\r\nSaved to {saPath}");
                            needSa = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"An error occurred creating the Start Actions: {ex.Message}\r\n{ex.StackTrace}");
                    }
                }

                EndActions endActions = null;
                // If the EA file exists, need to either download if desired or load existing if needed for AP
                if (_settings.downloadEA && needEa)
                {
                    try
                    {
                        endActions = await DownloadActionsArtifact("End", _roentgenClient.DownloadEndActionsAsync);
                        if (endActions != null)
                        {
                            _logger.Log("Writing End Actions to file...");
                            File.WriteAllText(eaPath, Functions.ExpandUnicode(JsonConvert.SerializeObject(endActions)));
                            _logger.Log($"End Actions file created successfully!\r\nSaved to {eaPath}");
                            needEa = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"An error occurred creating the End Actions: {ex.Message}\r\n{ex.StackTrace}");
                    }
                }
                else if (eaExists && _settings.autoBuildAP && needAp)
                {
                    endActions = JsonUtil.DeserializeFile<EndActions>(eaPath);
                    _logger.Log($"Loaded existing End Actions from {eaPath}");
                }

                if (!needAp && !needSa && !needEa)
                {
                    _logger.Log("All extras downloaded/built and none need to be overwritten, stopping here!");
                    return;
                }

                AuthorProfileGenerator.Response authorProfileResponse;
                _logger.Log("Attempting to build Author Profile...");
                if ((needSa || needAp) && endActions != null && _settings.autoBuildAP)
                {
                    authorProfileResponse = await _endActionsAuthorConverter.ConvertAsync(endActions, _cancelTokens.Token);
                    _logger.Log("Built Author Profile from the existing End Actions file!");
                }
                else
                {
                    authorProfileResponse = await _authorProfileGenerator.GenerateAsync(new AuthorProfileGenerator.Request
                    {
                        Book = bookInfo,
                        Settings = new AuthorProfileGenerator.Settings
                        {
                            AmazonTld = _settings.amazonTLD,
                            SaveBio = _settings.saveBio,
                            UseNewVersion = _settings.useNewVersion,
                            EditBiography = _settings.editBiography,
                            SaveHtml = _settings.saveHtml
                        }
                    }, _cancelTokens.Token);

                    if (authorProfileResponse == null)
                        return;
                }

                if (needAp)
                {
                    try
                    {
                        var authorProfileOutput = JsonConvert.SerializeObject(AuthorProfileGenerator.CreateAp(authorProfileResponse, bookInfo.Asin));
                        File.WriteAllText(apPath, authorProfileOutput);
                        _logger.Log($"Author Profile file created successfully!\r\nSaved to {apPath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"An error occurred while writing the Author Profile file: {ex.Message}\r\n{ex.StackTrace}");
                        return;
                    }
                }

                if (needSa || needEa)
                {
                    _logger.Log("Attempting to build Start and/or End Actions...");

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

                    var endActionsSettings = new EndActionsDataGenerator.Settings
                    {
                        AmazonTld = _settings.amazonTLD,
                        UseNewVersion = _settings.useNewVersion,
                        PromptAsin = _settings.promptASIN,
                        SaveHtml = _settings.saveHtml,
                        EstimatePageCount = _settings.pageCount
                    };

                    var endActionsDataGenerator = _diContainer.GetInstance<IEndActionsDataGenerator>();
                    var endActionsResponse = _settings.useNewVersion
                        ? await endActionsDataGenerator.GenerateNewFormatData(bookInfo, endActionsSettings, _dataSource, authorProfileResponse, AsinPrompt, metadata, _progress, _cancelTokens.Token)
                        : await endActionsDataGenerator.GenerateOld(bookInfo, endActionsSettings, _cancelTokens.Token);

                    if (endActionsResponse == null)
                        return;

                    if (needEa)
                    {
                        // Todo actions response/request stuff could still be cleaned up
                        var endActionsRequest = new EndActionsArtifactService.Request(
                            bookAsin: endActionsResponse.Book.Asin,
                            bookImageUrl: endActionsResponse.Book.ImageUrl,
                            bookDatabaseName: endActionsResponse.Book.Databasename,
                            bookGuid: endActionsResponse.Book.Guid,
                            bookErl: metadata.RawMlSize,
                            bookAmazonRating: endActionsResponse.Book.AmazonRating,
                            bookSeriesInfo: endActionsResponse.Book.Series,
                            author: authorProfileResponse.Name,
                            authorAsin: authorProfileResponse.Asin,
                            authorImageUrl: authorProfileResponse.ImageUrl,
                            authorBiography: authorProfileResponse.Biography,
                            authorOtherBooks: authorProfileResponse.OtherBooks,
                            userPenName: _settings.penName,
                            userRealName: _settings.realName,
                            customerAlsoBought: endActionsResponse.CustomerAlsoBought);

                        var endActionsContent = _settings.useNewVersion
                            ? _endActionsArtifactService.GenerateNew(endActionsRequest)
                            : _endActionsArtifactService.GenerateOld(endActionsRequest);

                        _logger.Log("Writing EndActions to file...");
                        File.WriteAllText(eaPath, endActionsContent);
                        _logger.Log($"EndActions file created successfully!\r\nSaved to {eaPath}");
                    }

                    if (needSa)
                    {
                        var startActions = _startActionsArtifactService.GenerateStartActions(endActionsResponse.Book, authorProfileResponse);

                        _logger.Log("Writing Start Actions to file...");
                        try
                        {
                            File.WriteAllText(saPath, Functions.ExpandUnicode(JsonConvert.SerializeObject(startActions)));
                        }
                        catch (Exception ex)
                        {
                            _logger.Log("An error occurred creating the Start Actions: " + ex.Message + "\r\n" + ex.StackTrace);
                        }

                        _logger.Log($"Start Actions file created successfully!\r\nSaved to {saPath}");
                    }
                }

                if (_settings.playSound)
                {
                    var player = new System.Media.SoundPlayer($@"{Environment.CurrentDirectory}\done.wav");
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred while creating the new Author Profile, Start Actions, and/or End Actions files:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                CheckFiles(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), metadata.DbName, metadata.Guid);
            }
        }

        private async void btnDownloadTerms_Click(object sender, EventArgs e)
        {
            if (rdoGoodreads.Checked && txtGoodreads.Text == "")
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
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\xml\"))
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\xml\");
            var path = $@"{Environment.CurrentDirectory}\xml\{Path.GetFileNameWithoutExtension(txtMobi.Text)}.xml";
            try
            {
                if (rdoGoodreads.Checked)
                {
                    _logger.Log($@"Exporting terms from {_dataSource.Name}...");
                    await Task.Run(() => _termsService.DownloadAndSaveAsync(_dataSource, txtGoodreads.Text, path, null, null, _settings.includeTopics, _progress, _cancelTokens.Token));
                }
                else if (rdoRoentgen.Checked)
                {
                    try
                    {
                        _logger.Log($@"Exporting terms from Roentgen...");
                        using var metadata = MetadataLoader.Load(txtMobi.Text);
                        await Task.Run(() => _termsService.DownloadAndSaveAsync(_diContainer.GetInstance<SecondarySourceRoentgen>(), null, path, metadata.Asin, _settings.roentgenRegion, _settings.includeTopics, _progress, _cancelTokens.Token));
                    }
                    catch (Exception ex) when (ex.Message.Contains("No terms"))
                    {
                        return;
                    }
                }
                else
                {
                    _logger.Log("Can't export terms from a file...");
                    return;
                }
                _logger.Log($"Character data has been successfully saved to: {path}");
                txtXMLFile.Text = path;
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Download cancelled.");
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
            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, false, _cancelTokens.Token);
            if (metadata == null)
                return;

            try
            {
                var bookSearchService = _diContainer.GetInstance<IBookSearchService>();
                var books = await bookSearchService.SearchSecondarySourceAsync(_dataSource,
                    new BookSearchService.Parameters
                    {
                        Asin = _settings.searchByAsin ? metadata.Asin : null,
                        Author = metadata.Author,
                        Title = metadata.Title
                    }, _cancelTokens.Token);

                if (books.Length <= 0)
                {
                    _logger.Log($"Unable to find this book on {_dataSource.Name}!\nEnsure the book's title ({metadata.Title}) is accurate!");
                    return;
                }

                string bookUrl;
                if (books.Length == 1)
                    bookUrl = books[0].DataUrl;
                else
                {
                    // Pre-load cover images
                    foreach (var book in books.Where(book => !string.IsNullOrEmpty(book.ImageUrl)))
                    {
                        try
                        {
                            book.CoverImage = await _httpClient.GetImageAsync(book.ImageUrl, cancellationToken: _cancelTokens.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Failed to download cover image: {ex.Message}");
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
                _logger.Log($"An error occurred while searching: {ex.Message}\r\n{ex.StackTrace}");
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
            _settings.buildSource = rdoGoodreads.Checked
                ? "Goodreads"
                : rdoRoentgen.Checked
                    ? "Roentgen"
                    : "XML";
            _settings.Save();
            if (txtOutput.Text.Trim().Length != 0)
                File.WriteAllText(_currentLog, txtOutput.Text);
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ActiveControl = lblGoodreads;
            _tooltip.SetToolTip(btnBrowseMobi, "Open a Kindle book.");
            _tooltip.SetToolTip(btnBrowseOutput, "Open the default output directory.");
            _tooltip.SetToolTip(btnOneClick, "One Click to try to build the Start\r\nAction, Author Profile, End Action\r\nand X-Ray files for this book.");
            _tooltip.SetToolTip(btnBrowseXML, "Open a supported XML or TXT file containing characters and topics.");
            _tooltip.SetToolTip(btnKindleExtras, "Try to build the Start Action, Author Profile,\r\nand End Action files for this book.");
            _tooltip.SetToolTip(btnBuild, "Try to build the X-Ray file for this book.");
            _tooltip.SetToolTip(btnSettings, "Configure X-Ray Builder GUI.");
            _tooltip.SetToolTip(btnPreview, "View a preview of the generated files.");
            _tooltip.SetToolTip(btnUnpack, "Save the rawML (raw markup) of the book\r\nin the output directory so you can review it.");
            _tooltip.SetToolTip(btnExtractTerms, "Extract an existing X-Ray file to an XML file.\r\nThis can be useful if you have characters and\r\nterms you want to reuse.");
            _tooltip.SetToolTip(btnCreate, "Create an XML file containing characters\r\nand settings, or edit an existing XML file.");

            _tooltip.SetToolTip(rdoGoodreads, "Use the above link as a terms source.");
            _tooltip.SetToolTip(rdoRoentgen, "Download terms from Roentgen if any are available.");
            _tooltip.SetToolTip(rdoFile, "Load terms from the selected file.");

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
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\{dir}");

            if (_settings.outDir == "")
                _settings.outDir = $@"{Environment.CurrentDirectory}\out";

            txtGoodreads.Text = _settings.Goodreads;
            if (_settings.buildSource == "Goodreads")
                rdoGoodreads.Checked = true;
            else if (_settings.buildSource == "Roentgen")
                rdoRoentgen.Checked = true;
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
            if (rdoGoodreads.Checked)
                _tooltip.SetToolTip(btnDownloadTerms, $"Save {_dataSource.Name} terms to an XML file.");
            else if (rdoRoentgen.Checked)
                _tooltip.SetToolTip(btnDownloadTerms, $"Save Roentgen terms to an XML file.");
            _tooltip.SetToolTip(btnSearchGoodreads, _dataSource.SearchEnabled
                ? $"Try to search for this book on {_dataSource.Name}."
                : $"Search is disabled when {_dataSource.Name} is selected as a data source.");
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var filePaths = (string[]) e.Data.GetData(DataFormats.FileDrop);
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
            if (((RadioButton)sender).Text == "File")
            {
                txtXMLFile.Enabled = true;
                btnBrowseXML.Enabled = true;
                btnDownloadTerms.Enabled = false;
            }
            else
            {
                txtXMLFile.Enabled = false;
                btnBrowseXML.Enabled = false;
                btnDownloadTerms.Enabled = true;
            }
            SetDatasourceLabels();
        }

        private async void txtMobi_TextChanged(object sender, EventArgs e)
        {
            if (txtMobi.Text == "" || !File.Exists(txtMobi.Text))
                return;
            txtGoodreads.Text = "";
            prgBar.Value = 0;
            _openedMetadata = null;

            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, false, _cancelTokens.Token);
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

            CheckFiles(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), metadata.DbName, metadata.Guid);
            btnBuild.Enabled = metadata.XRaySupported;
            btnOneClick.Enabled = metadata.XRaySupported;
            btnUnpack.Enabled = metadata.RawMlSupported;

            try
            {
                if (AmazonClient.IsAsin(metadata.Asin))
                {
                    // Fire and forget
                    #pragma warning disable 4014
                    Task.Run(() => _roentgenClient.PreloadAsync(metadata.Asin, _settings.roentgenRegion, _cancelTokens.Token)).ConfigureAwait(false);
                    #pragma warning restore 4014
                }
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            cmsPreview.Show(btnPreview, new Point(2, btnPreview.Height));
        }

        private async void tmiAuthorProfile_Click(object sender, EventArgs e)
        {
            var path = _openedMetadata != null
                ? _directoryService.GetArtifactPath(ArtifactType.AuthorProfile, _openedMetadata, Path.GetFileNameWithoutExtension(txtMobi.Text), false)
                : "";
            await ShowPreviewAsync(PreviewProviderFactory.PreviewType.AuthorProfile, path, _cancelTokens.Token);
        }

        private async void tmiStartAction_Click(object sender, EventArgs e)
        {
            var path = _openedMetadata != null
                ? _directoryService.GetArtifactPath(ArtifactType.StartActions, _openedMetadata, Path.GetFileNameWithoutExtension(txtMobi.Text), false)
                : "";
            await ShowPreviewAsync(PreviewProviderFactory.PreviewType.StartActions, path, _cancelTokens.Token);
        }

        private async void tmiEndAction_Click(object sender, EventArgs e)
        {
            var path = _openedMetadata != null
                ? _directoryService.GetArtifactPath(ArtifactType.EndActions, _openedMetadata, Path.GetFileNameWithoutExtension(txtMobi.Text), false)
                : "";
            await ShowPreviewAsync(PreviewProviderFactory.PreviewType.EndActions, path, _cancelTokens.Token);
        }

        private async void tmiXray_Click(object sender, EventArgs e)
        {
            var path = _openedMetadata != null
                ? _directoryService.GetArtifactPath(ArtifactType.XRay, _openedMetadata, Path.GetFileNameWithoutExtension(txtMobi.Text), false)
                : "";
            await ShowPreviewAsync(PreviewProviderFactory.PreviewType.XRay, path, _cancelTokens.Token);
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
            using var metadata = MetadataLoader.Load(txtMobi.Text);
            var rawMlPath = UIFunctions.RawMlPath(Path.GetFileNameWithoutExtension(txtMobi.Text));
            metadata.SaveRawMl(rawMlPath);
            _logger.Log($"Extracted to {rawMlPath}!\r\n");
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
                // TODO This should be based on the file not the setting
                var terms = newVer == XRayUtil.XRayVersion.New
                    ? _termsService.ExtractTermsNew(new SQLiteConnection($"Data Source={selPath}; Version=3;"), true)
                    : _termsService.ExtractTermsOld(selPath);
                if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                    Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
                var outfile = Environment.CurrentDirectory + @"\xml\" + Path.GetFileNameWithoutExtension(selPath) + ".xml";
                XmlUtil.SerializeToFile(terms.ToList(), outfile);
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
            var frmCreateXr = _diContainer.GetInstance<frmCreateXR>();
            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, false, _cancelTokens.Token);
            if (metadata != null)
                frmCreateXr.SetMetadata(metadata.Asin, metadata.Author, metadata.Title);
            frmCreateXr.ShowDialog();
        }

        private void CheckFiles(string author, string title, string asin, string fileName, string databaseName, string guid)
        {
            static Image SetPreviewAndPickImage(ToolStripItem toolStripItem, string path)
            {
                var fileExists = File.Exists(path);

                toolStripItem.Enabled = fileExists;

                return fileExists
                    ? Resources.file_on
                    : Resources.file_off;
            }

            pbFile1.Image = SetPreviewAndPickImage(cmsPreview.Items[2], _directoryService.GetArtifactPath(ArtifactType.StartActions, author, title, asin, fileName, databaseName, guid, false));
            pbFile2.Image = SetPreviewAndPickImage(cmsPreview.Items[0], _directoryService.GetArtifactPath(ArtifactType.AuthorProfile, author, title, asin, fileName, databaseName, guid, false));
            pbFile3.Image = SetPreviewAndPickImage(cmsPreview.Items[1], _directoryService.GetArtifactPath(ArtifactType.EndActions, author, title, asin, fileName, databaseName, guid, false));
            pbFile4.Image = SetPreviewAndPickImage(cmsPreview.Items[3], _directoryService.GetArtifactPath(ArtifactType.XRay, author, title, asin, fileName, databaseName, guid, false));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!_cancelTokens.IsCancellationRequested)
            {
                _logger.Log("Canceling...");
                _cancelTokens.Cancel();
            }
        }

        private async Task ShowPreviewAsync(PreviewProviderFactory.PreviewType type, string filePath, CancellationToken cancellationToken)
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
                await previewForm.Populate(selPath, cancellationToken);
                previewForm.ShowDialog();

            }
            catch (Exception ex)
            {
                _logger.Log("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private async Task CheckAndFixIncorrectAsinOrThrowAsync(IMetadata metadata, string bookPath, CancellationToken cancellationToken)
        {
            if (AmazonClient.IsAsin(metadata.Asin))
                return;

            if (!metadata.CanModify && DialogResult.No == MessageBox.Show($"Invalid Amazon ASIN detected: {metadata.Asin}!\nKindle may not display an X-Ray for this book.\nDo you wish to continue?", "Incorrect ASIN", MessageBoxButtons.YesNo))
            {
                throw new Exception($"Invalid Amazon ASIN detected: {metadata.Asin}!\r\nKindle may not display an X-Ray for this book.\r\nYou must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) or a MOBI editor (exth 113 and optionally 504) to change this.");
            }

            var dialogResult = MessageBox.Show($"Invalid Amazon ASIN detected: {metadata.Asin}!\nKindle may not display an X-Ray for this book.\nDo you want to fix it?\r\n(This will modify the book meaning it will need to be re-copied to your Kindle device)\r\nTHIS FEATURE IS EXPERIMENTAL AND COULD DESTROY YOUR BOOK!", "Incorrect ASIN", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
                return;

            _logger.Log($"Searching Amazon for {metadata.Title} by {metadata.Author}...");
            var amazonSearchResult = await _amazonClient.SearchBook(metadata.Title, metadata.Author, _settings.amazonTLD, cancellationToken);
            if (amazonSearchResult != null)
            {
                // Prompt if book is correct. If not, prompt for manual entry
                dialogResult = MessageBox.Show($"Found the following book on Amazon:\r\nTitle: {amazonSearchResult.Title}\r\nAuthor: {amazonSearchResult.Author}\r\nASIN: {amazonSearchResult.Asin}\r\n\r\nDoes this seem correct? If so, the shown ASIN will be used.", "Amazon Search Result", MessageBoxButtons.YesNoCancel);
                switch (dialogResult)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                    {
                        metadata.SetAsin(amazonSearchResult.Asin);
                        using var fs = new FileStream(bookPath, FileMode.Create);
                        metadata.Save(fs);
                        _logger.Log($"Successfully updated the ASIN to {metadata.Asin}! Be sure to copy this new version of the book to your Kindle device.");
                        return;
                    }
                }
            }
            else
                _logger.Log("Unable to automatically find a matching ASIN for this book on Amazon :(");

            // TODO: manual entry
        }
    }
}