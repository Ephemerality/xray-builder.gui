using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
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
using XRayBuilderGUI.Localization.Main;
using XRayBuilderGUI.UI.Preview.Model;
using Container = SimpleInjector.Container;

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

            toolStrip.Renderer = ToolStripTheme.Renderer();
        }

        private readonly ToolTip _tooltip = new();
        private readonly Settings _settings = Settings.Default;
        private readonly string _currentLog = $@"{Environment.CurrentDirectory}\log\{DateTime.Now:HH.mm.ss.dd.MM.yyyy}.txt";

        private readonly IProgressBar _progress;

        private CancellationTokenSource _cancelTokens = new();
        private ISecondarySource _dataSource;

        private IMetadata _openedMetadata;

        private DialogResult SafeShow([Localizable(true)] string msg, [Localizable(true)] string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
        {
            return (DialogResult)Invoke(new Func<DialogResult>(() => MessageBox.Show(this, msg, caption, buttons, icon, def)));
        }

        private void ToggleInterface(bool enabled)
        {
            toolStrip.ClearAllSelections();
            toolStrip.Enabled = enabled;

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
            var file = UIFunctions.GetBook("");
            if (string.IsNullOrEmpty(file) || txtMobi.Text.Equals(file))
                return;
            txtMobi.Text = file;
        }

        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = UIFunctions.GetFile("Open an entity file", txtXMLFile.Text,  "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt");
        }

        private async void btnBuild_Click(object sender, EventArgs e)
        {
            ToggleInterface(false);
            await btnBuild_Run();
            ToggleInterface(true);
        }

        private async Task<IMetadata> GetAndValidateMetadataAsync(string mobiFile, bool saveRawMl, CancellationToken cancellationToken)
        {
            _logger.Log(MainStrings.ExtractingMetadata);
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
                    _logger.Log($@"{MainStrings.FailedToValidateAsin}: {ex.Message}\r\n{MainStrings.ContinuingAnyway}...", LogLevel.Error);
                }

                if (!Settings.Default.useNewVersion && metadata.DbName.Length == 31)
                {
                    MessageBox.Show(string.Format(MainStrings.DatabaseNameLengthWarning, metadata.DbName), "Database Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (saveRawMl && metadata.RawMlSupported)
                {
                    _logger.Log(MainStrings.SavingRawml);
                    metadata.SaveRawMl(_directoryService.GetRawmlPath(mobiFile));
                }
                _logger.Log($@"{MainStrings.GotMetadata}{Environment.NewLine}{MainStrings.Asin}: {metadata.Asin}");

                _openedMetadata = metadata;
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.Log($@"{MainStrings.ErrorExtractingMetadata}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return null;
        }

        private async Task btnBuild_Run()
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(MainStrings.BookNotFound, MainStrings.BookNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (rdoGoodreads.Checked && txtGoodreads.Text == "")
            {
                MessageBox.Show(string.Format(MainStrings.NoSourceLinkSpecified, _dataSource.Name), string.Format(MainStrings.NoSourceLinkTitle, _dataSource.Name), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Directory.Exists(_settings.outDir))
            {
                MessageBox.Show($@"{MainStrings.SpecifiedOutputDirectoryDoesNotExist}{Environment.NewLine}{MainStrings.ReviewSettingsPage}", MainStrings.OutputDirNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_settings.realName.Trim().Length == 0 || _settings.penName.Trim().Length == 0)
            {
                MessageBox.Show($@"{MainStrings.PenNamesRequired}{Environment.NewLine}{MainStrings.InformationAllowsRatingOnAmazon}{Environment.NewLine}{MainStrings.ReviewSettingsPage}", MainStrings.AmazonCustomerDetailsNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_settings.buildSource == "XML" && txtXMLFile.Text == "")
            {
                MessageBox.Show("No supported file containing term data was specified.\r\nBrowse for an XML or TXT file containing character\r\nand topic data for this e-book before trying to\r\ncreate an X-Ray file.",
                    "Missing Terms File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            prgBar.Value = 0;

            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, _settings.saverawml, _cancelTokens.Token);
            if (metadata == null)
                return;

            // Added author name to log output
            _logger.Log($@"{string.Format(MainStrings.BooksSourceUrl, _dataSource.Name)}: {txtGoodreads.Text}");
            if (_cancelTokens.IsCancellationRequested) return;
            _logger.Log(MainStrings.AttemptingBuildXRay);

            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay xray;
            SetDatasourceLabels(); // Reset the dataSource for the new build process
            try
            {
                var selectedSource = _dataSource;
                Task<XRay> xrayTask;
                if (rdoGoodreads.Checked)
                    xrayTask = _xrayService.CreateXRayAsync(txtGoodreads.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _settings.amazonTLD, _settings.includeTopics, selectedSource, _progress, _cancelTokens.Token);
                else if (rdoRoentgen.Checked)
                {
                    selectedSource = _diContainer.GetInstance<SecondarySourceRoentgen>();
                    xrayTask = _xrayService.CreateXRayAsync(txtGoodreads.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _settings.roentgenRegion, _settings.includeTopics, selectedSource, _progress, _cancelTokens.Token);
                }
                else
                {
                    // TODO Set datasource properly
                    selectedSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(SecondaryDataSourceFactory.Enum.File);
                    xrayTask = _xrayService.CreateXRayAsync(txtXMLFile.Text, metadata.DbName, metadata.UniqueId, metadata.Asin, _settings.amazonTLD, _settings.includeTopics, selectedSource, _progress, _cancelTokens.Token);
                }

                xray = await Task.Run(() => xrayTask).ConfigureAwait(false);

                if (xray.Terms.Count == 0 && DialogResult.No == MessageBox.Show(MainStrings.NoTermsAvailable, MainStrings.NoTermsTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                {
                    _logger.Log(MainStrings.Cancelling);
                    return;
                }

                var aliasPath = _directoryService.GetAliasPath(xray.Asin);
                _xrayService.ExportAndDisplayTerms(xray, selectedSource, _settings.overwriteAliases, _settings.splitAliases);

                if (_settings.enableEdit && DialogResult.Yes == MessageBox.Show($@"{MainStrings.TermsExportedOrAlreadyExist}\r\n{MainStrings.OpenInNotepad}\r\n{MainStrings.SeeMobilereads}", MainStrings.Aliases, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(aliasPath);
                }
                if (xray.Terms.Any(term => term.Aliases?.Count > 0))
                    _logger.Log(MainStrings.AliasesReadFromXml);
                else if (!File.Exists(aliasPath))
                    _logger.Log(MainStrings.AliasesFileNotFound);
                else
                {
                    _aliasesRepository.LoadAliasesForXRay(xray);
                    _logger.Log(string.Format(MainStrings.AliasesReadFrom, aliasPath));
                }

                _logger.Log(MainStrings.InitialXRayBuiltAddingChapters);
                //Expand the X-Ray file from the unpacked mobi
                Task buildTask;
                switch (metadata)
                {
                    case Metadata _:
                        bool EditChaptersCallback()
                        {
                            if (xray.Unattended || !_settings.enableEdit)
                                return false;

                            return DialogResult.Yes == SafeShow(MainStrings.OpenChaptersFileNotepad, MainStrings.Chapters,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        }
                        // ReSharper disable twice AccessToDisposedClosure
                        // todo just pass metadata instead of calling getrawmlstream
                        buildTask = Task.Run(() => _xrayService.ExpandFromRawMl(xray, metadata, metadata.GetRawMlStream(), _settings.useNewVersion, _settings.skipNoLikes, _settings.minClipLen, _settings.overwriteChapters, EditChaptersCallback, _progress, _cancelTokens.Token, _settings.ignoresofthyphen, !_settings.useNewVersion));
                        break;
                    case KfxContainer kfx:
                        if (!_settings.useNewVersion)
                            throw new Exception(MainStrings.BuildingOldFormatNotSupported);

                        buildTask = Task.Run(() => _kfxXrayService.AddLocations(xray, kfx, _settings.skipNoLikes, _settings.minClipLen, _progress, _cancelTokens.Token));
                        break;
                    default:
                        throw new NotSupportedException();
                }
                await buildTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.Log(MainStrings.BuildCancelled);
                return;
            }
            catch (Exception ex)
            {
                _logger.Log($@"{MainStrings.ErrorBuildingXRay}:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return;
            }

            _logger.Log(MainStrings.SavingXRay);
            var xrayPath = _directoryService.GetArtifactPath(ArtifactType.XRay, metadata, Path.GetFileNameWithoutExtension(txtMobi.Text), true);

            try
            {
                var xrayExporter = _xrayExporterFactory.Get(_settings.useNewVersion ? XRayExporterFactory.Enum.Sqlite : XRayExporterFactory.Enum.Json);
                xrayExporter.Export(xray, xrayPath, _progress, _cancelTokens.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Log(MainStrings.BuildCancelled);
                return;
            }
            catch (Exception ex)
            {
                // TODO: Add option to retry maybe?
                _logger.Log($@"{MainStrings.ErrorBuildingXRay}. {MainStrings.OpenedInAnotherProgram}{Environment.NewLine}{ex.Message}");
                return;
            }

            if (_settings.useNewVersion)
            {
                //Save the new XRAY.ASIN.previewData file
                try
                {
                    var pdPath = _directoryService.GetArtifactPath(ArtifactType.XRayPreview, metadata, Path.GetFileNameWithoutExtension(txtMobi.Text), true);
                    _previewDataExporter.Export(xray, pdPath);
                    _logger.Log($@"{MainStrings.PreviewData}\r\n{string.Format(MainStrings.SavedTo, pdPath)}");
                }
                catch (Exception ex)
                {
                    _logger.Log($@"{MainStrings.ErrorPreviewData}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }

            _logger.Log($@"{MainStrings.XRayCreated}{Environment.NewLine}{string.Format(MainStrings.SavedTo, xrayPath)}");

            CheckFiles(metadata.Author, metadata.Title, metadata.Asin, Path.GetFileNameWithoutExtension(txtMobi.Text), metadata.DbName, metadata.Guid);

            if (_settings.playSound)
            {
                var player = new System.Media.SoundPlayer($@"{Environment.CurrentDirectory}\done.wav");
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
                MessageBox.Show(MainStrings.BookNotFound, MainStrings.BookNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (txtGoodreads.Text == "")
            {
                MessageBox.Show(string.Format(MainStrings.NoSourceLinkSpecified, _dataSource.Name), string.Format(MainStrings.NoSourceLinkTitle, _dataSource.Name), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!txtGoodreads.Text.ToLower().Contains(_settings.dataSource.ToLower()))
            {
                MessageBox.Show($@"{string.Format(MainStrings.InvalidSourceLink, _dataSource.Name)}{Environment.NewLine}{string.Format(MainStrings.ReviewSettingsForSource, _dataSource.Name)}", string.Format(MainStrings.InvalidSourceLinkTitle, _dataSource.Name), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_settings.realName.Trim().Length == 0 || _settings.penName.Trim().Length == 0)
            {
                MessageBox.Show($@"{MainStrings.PenNamesRequired}{Environment.NewLine}{MainStrings.InformationAllowsRatingOnAmazon}{Environment.NewLine}{MainStrings.ReviewSettingsPage}", MainStrings.AmazonCustomerDetailsNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, _settings.saverawml, _cancelTokens.Token);
            if (metadata == null)
                return;

            SetDatasourceLabels(); // Reset the dataSource for the new build process
            _logger.Log($@"{string.Format(MainStrings.BooksSourceUrl, _dataSource.Name)}: {txtGoodreads.Text}");
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
                    _logger.Log($@"{MainStrings.AllExtrasExistNoOverwrite}{Environment.NewLine}{MainStrings.Cancelling}");
                    return;
                }

                async Task<TActions> DownloadActionsArtifact<TActions>(string type, Func<string, string, CancellationToken, Task<TActions>> download) where TActions : class
                {
                    _logger.Log(string.Format(MainStrings.DownloadingActions, type));
                    try
                    {
                        var actions = await download(metadata.Asin, _settings.roentgenRegion, _cancelTokens.Token);
                        if (actions == null)
                        {
                            _logger.Log(string.Format(MainStrings.NoPremadeActionsAvailable, type));
                            return null;
                        }

                        _logger.Log(string.Format(MainStrings.PreMadeActionsDownloaded, type));
                        return actions;
                    }
                    catch (Exception e)
                    {
                        _logger.Log($@"{MainStrings.NoPremadeActionsAvailable}:{Environment.NewLine}{e.Message}");
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
                            _logger.Log(MainStrings.WritingStartActionsFile);
                            File.WriteAllText(saPath, Functions.ExpandUnicode(JsonConvert.SerializeObject(startActions)));
                            _logger.Log($@"{MainStrings.StartActionsCreated}{Environment.NewLine}{string.Format(MainStrings.SavedTo, saPath)}");
                            needSa = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($@"{MainStrings.ErrorCreatingStartActions}: {ex.Message}\r\n{ex.StackTrace}");
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
                            _logger.Log(MainStrings.WritingEndActionsFile);
                            File.WriteAllText(eaPath, Functions.ExpandUnicode(JsonConvert.SerializeObject(endActions)));
                            _logger.Log($@"{MainStrings.EndActionsCreated}{Environment.NewLine}{string.Format(MainStrings.SavedTo, eaPath)}");
                            needEa = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($@"{MainStrings.ErrorEndActions}: {ex.Message}\r\n{ex.StackTrace}");
                    }
                }
                else if (eaExists && _settings.autoBuildAP && needAp)
                {
                    endActions = JsonUtil.DeserializeFile<EndActions>(eaPath);
                    _logger.Log(string.Format(MainStrings.LoadedExistingEndActions, eaPath));
                }

                if (!needAp && !needSa && !needEa)
                {
                    _logger.Log(MainStrings.AllExtrasDownloadedStopping);
                    return;
                }

                AuthorProfileGenerator.Response authorProfileResponse;
                _logger.Log(MainStrings.BuildingAuthorProfile);
                if ((needSa || needAp) && endActions != null && _settings.autoBuildAP)
                {
                    authorProfileResponse = await _endActionsAuthorConverter.ConvertAsync(endActions, _cancelTokens.Token);
                    _logger.Log(MainStrings.BuiltAuthorProfileFromExisting);
                }
                else
                {
                    bool EditBioCallback(string message)
                    {
                        if (!_settings.editBiography)
                            return false;

                        return DialogResult.Yes == MessageBox.Show(message, MainStrings.Biography, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    }
                    authorProfileResponse = await _authorProfileGenerator.GenerateAsync(new AuthorProfileGenerator.Request
                    {
                        Book = bookInfo,
                        Settings = new AuthorProfileGenerator.Settings
                        {
                            AmazonTld = _settings.amazonTLD,
                            SaveBio = _settings.saveBio,
                            UseNewVersion = _settings.useNewVersion,
                            EditBiography = _settings.editBiography
                        }
                    }, EditBioCallback, _progress, _cancelTokens.Token);

                    if (authorProfileResponse == null)
                        return;
                }

                if (needAp)
                {
                    try
                    {
                        var authorProfileOutput = JsonConvert.SerializeObject(AuthorProfileGenerator.CreateAp(authorProfileResponse, bookInfo.Asin));
                        File.WriteAllText(apPath, authorProfileOutput);
                        _logger.Log($@"{MainStrings.AuthorProfileCreated}{Environment.NewLine}{string.Format(MainStrings.SavedTo, apPath)}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($@"{MainStrings.ErrorWritingAuthorProfile}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        return;
                    }
                }

                if (needSa || needEa)
                {
                    _logger.Log(MainStrings.BuildingStartEndActions);

                    string AsinPrompt(string title, string author)
                    {
                        var frmAsin = _diContainer.GetInstance<frmASIN>();
                        frmAsin.Text = MainStrings.SeriesInformation;
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
                        EstimatePageCount = _settings.pageCount,
                        EditDescription = _settings.editDescription
                    };

                    var endActionsDataGenerator = _diContainer.GetInstance<IEndActionsDataGenerator>();
                    var endActionsResponse = _settings.useNewVersion
                        ? await endActionsDataGenerator.GenerateNewFormatData(bookInfo, endActionsSettings, _dataSource, authorProfileResponse, AsinPrompt, metadata, _progress, _cancelTokens.Token)
                        : await endActionsDataGenerator.GenerateOld(bookInfo, endActionsSettings, _progress, _cancelTokens.Token);

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

                        _logger.Log(MainStrings.WritingEndActionsFile);
                        File.WriteAllText(eaPath, endActionsContent);
                        _logger.Log($@"{MainStrings.EndActionsCreated}{Environment.NewLine}{string.Format(MainStrings.SavedTo, eaPath)}");
                    }

                    if (needSa)
                    {
                        var startActions = _startActionsArtifactService.GenerateStartActions(endActionsResponse.Book, authorProfileResponse);

                        _logger.Log(MainStrings.WritingStartActionsFile);
                        try
                        {
                            File.WriteAllText(saPath, Functions.ExpandUnicode(JsonConvert.SerializeObject(startActions)));
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($@"{MainStrings.ErrorCreatingStartActions}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        }

                        _logger.Log($@"{MainStrings.StartActionsCreated}{Environment.NewLine}{string.Format(MainStrings.SavedTo, saPath)}");
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
                _logger.Log($@"{MainStrings.ErrorCreatingExtras}:{Environment.NewLine}{ex.Message}\r\n{ex.StackTrace}");
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
                MessageBox.Show(MainStrings.NoLinkSpecified, MainStrings.NoLinkTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show(MainStrings.BookNotFound, MainStrings.BookNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    _logger.Log(string.Format(MainStrings.ExportingTermsFrom, _dataSource.Name));
                    await Task.Run(() => _termsService.DownloadAndSaveAsync(_dataSource, txtGoodreads.Text, path, null, null, _settings.includeTopics, _progress, _cancelTokens.Token));
                }
                else if (rdoRoentgen.Checked)
                {
                    try
                    {
                        _logger.Log(string.Format(MainStrings.ExportingTermsFrom, "Roentgen"));
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
                    _logger.Log(MainStrings.CantExportFromFile);
                    return;
                }
                _logger.Log(string.Format(MainStrings.CharacterDataSavedTo, path));
                txtXMLFile.Text = path;
            }
            catch (OperationCanceledException)
            {
                _logger.Log(MainStrings.DownloadCancelled);
            }
            catch (Exception ex)
            {
                _logger.Log($@"{string.Format(MainStrings.ErrorSavingXml, path)}: {ex.Message}");
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
                MessageBox.Show(MainStrings.BookNotFound, MainStrings.BookNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!Directory.Exists(_settings.outDir))
            {
                MessageBox.Show(MainStrings.OutputDirNotFoundReviewSettings, MainStrings.OutputDirNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //this.TopMost = true;
            using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, false, _cancelTokens.Token);
            if (metadata == null)
                return;

            try
            {
                var bookSearchService = _diContainer.GetInstance<IBookSearchService>();
                var books = await bookSearchService.SearchSecondarySourceAsync(_dataSource, metadata, _cancelTokens.Token);

                if (books.Length <= 0)
                {
                    _logger.Log(string.Format(MainStrings.UnableToFindBookOnSource, _dataSource.Name, metadata.Title));
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
                            _logger.Log($@"{MainStrings.FailedCoverImageDownload}: {ex.Message}");
                        }
                    }

                    _logger.Log(string.Format(MainStrings.MultipleResultsFromSource, _dataSource.Name));
                    var frmG = new frmGR(books, _dataSource);
                    frmG.ShowDialog();
                    bookUrl = books[frmG.cbResults.SelectedIndex].DataUrl;
                }

                if (!string.IsNullOrEmpty(bookUrl))
                {
                    txtGoodreads.Text = bookUrl;
                    txtGoodreads.Refresh();
                    _logger.Log($@"{string.Format(MainStrings.BookFoundOnSource, _dataSource.Name)}{Environment.NewLine}{string.Format(MainStrings.TitleByAuthor, metadata.Title, metadata.Author)}{Environment.NewLine}{string.Format(MainStrings.SourceUrl, _dataSource.Name)}: {bookUrl}{Environment.NewLine}{MainStrings.VisitUrl}");
                    SetDatasourceLink(bookUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.Log($@"{MainStrings.ErrorSearching}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using var frmSet = _diContainer.GetInstance<frmSettings>();
            frmSet.ShowDialog();
            SetDatasourceLabels();
            AdjustUi();

            if (!txtGoodreads.Text.ToLower().Contains(_settings.dataSource.ToLower()))
                txtGoodreads.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settings.mobiFile = txtMobi.Text;
            _settings.xmlFile = txtXMLFile.Text;
            _settings.Goodreads = txtGoodreads.Text;
            _settings.Save();

            if (txtOutput.Text.Trim().Length != 0)
                File.WriteAllText(_currentLog, txtOutput.Text);
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ActiveControl = lblGoodreads;
            btnBrowseMobi.ToolTipText = MainStrings.OpenKindleBook;
            btnBrowseFolders.ToolTipText = MainStrings.OpenOutputDirectory;
            btnOneClick.ToolTipText = MainStrings.OneClickTooltip;
            btnBrowseXML.ToolTipText = MainStrings.OpenXmlOrTxt;
            btnKindleExtras.ToolTipText = MainStrings.BuildExtrasTooltip;
            btnBuild.ToolTipText = MainStrings.TryToBuildXRay;
            btnSettings.ToolTipText = MainStrings.ConfigureXRayBuilder;
            btnPreview.ToolTipText = MainStrings.ViewPreviewOfGeneratedFiles;
            btnUnpack.ToolTipText = MainStrings.SaveRawMlTooltip;
            btnExtractTerms.ToolTipText = MainStrings.ExtractXRayToXml;
            btnCreate.ToolTipText = MainStrings.CreateXmlTooltip;

            _tooltip.SetToolTip(pbCover, "Double-click to open\r\nthe book details window");

            _tooltip.SetToolTip(rdoGoodreads, MainStrings.UseLinkAsDataSource);
            _tooltip.SetToolTip(rdoRoentgen, MainStrings.DownloadFromRoentgen);
            _tooltip.SetToolTip(rdoFile, MainStrings.LoadTermsFromFile);

            _tooltip.SetToolTip(pbFile1, MainStrings.StartActions);
            _tooltip.SetToolTip(pbFile2, MainStrings.AuthorProfile);
            _tooltip.SetToolTip(pbFile3, MainStrings.EndActions);
            _tooltip.SetToolTip(pbFile4, MainStrings.XRay);

            DragEnter += frmMain_DragEnter;
            DragDrop += frmMain_DragDrop;

            var args = Environment.GetCommandLineArgs();

            txtMobi.Text = args.Skip(1).Where(File.Exists).Select(Path.GetFullPath).FirstOrDefault()
                           ?? _settings.mobiFile;

            if (txtXMLFile.Text == "") txtXMLFile.Text = _settings.xmlFile;

            // TODO: Maybe do something about these paths
            // TODO: ExtLoader or something?
            foreach (var dir in new [] { "out", "log", "dmp", "tmp", "ext", "rec" })
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\{dir}");

            if (_settings.outDir == "")
                _settings.outDir = $@"{Environment.CurrentDirectory}\out";

            txtGoodreads.Text = !File.Exists(txtMobi.Text)
                    ? ""
                    : _settings.Goodreads;

            if (_settings.buildSource == "Goodreads")
            {
                rdoGoodreads.Checked = true;
                btnBrowseXML.Visible = false;
                btnDownloadTerms.Visible = true;
            }
            else if (_settings.buildSource == "Roentgen")
            {
                rdoRoentgen.Checked = true;
                btnBrowseXML.Visible = false;
                btnDownloadTerms.Visible = true;
            }
            else
            {
                rdoFile.Checked = true;
                btnBrowseXML.Visible = true;
                btnDownloadTerms.Visible = false;
            }

            SetDatasourceLabels();
            AdjustUi();
        }

        private void AdjustUi()
        {
            txtGoodreads.Location = new Point(lblGoodreads.Location.X + lblGoodreads.Width + 11, txtGoodreads.Location.Y);
            txtGoodreads.Size = new Size(groupBox1.Width - txtGoodreads.Location.X - 13, txtGoodreads.Size.Height);
        }

        private void SetSelectedDatasource()
        {
            if (rdoGoodreads.Checked)
            {
                btnDownloadTerms.ToolTipText = $"Save {_dataSource.Name} terms to an XML file.";
            }
            else if (rdoRoentgen.Checked)
            {
                btnDownloadTerms.ToolTipText = "Save Roentgen terms to an XML file.";
            }

            _settings.buildSource = rdoGoodreads.Checked
                ? "Goodreads"
                : rdoRoentgen.Checked
                    ? "Roentgen"
                    : "XML";
            _settings.Save();
        }

        private void SetDatasourceLink(string url)
        {
            txtDatasource.Visible = true;
            if (string.IsNullOrEmpty(url))
            {
                txtDatasource.Text = "Search datasource...";
                return;
            }

            // todo this is weird
            var matchId = Regex.Match(url, @"(show|work)/(\d+)");
            if (!matchId.Success)
                return;
            _tooltip.SetToolTip(txtDatasource, url);
            txtDatasource.Text = matchId.Groups[2].Value;
        }

        private void SetDatasourceLabels()
        {
            // todo: use enum directly for setting - consider passing enum vs datasource
            var datasource = (SecondaryDataSourceFactory.Enum) Enum.Parse(typeof(SecondaryDataSourceFactory.Enum), _settings.dataSource);

            _dataSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(datasource);
            btnSearchGoodreads.Enabled = _dataSource.SearchEnabled;
            lblGoodreads.Left = _dataSource.UrlLabelPosition;
            rdoGoodreads.Text = _dataSource.Name;
            lblGoodreads.Text = $@"{string.Format(MainStrings.SourceUrl, _dataSource.Name)}:";
            if (rdoGoodreads.Checked)
                btnDownloadTerms.ToolTipText = $"Save {_dataSource.Name} terms to an XML file.";
            else if (rdoRoentgen.Checked)
                btnDownloadTerms.ToolTipText = "Save Roentgen terms to an XML file.";
            btnSearchGoodreads.ToolTipText = _dataSource.SearchEnabled
                ? $"Try to search for this book on {_dataSource.Name}."
                : $"Search is disabled when {_dataSource.Name} is selected as a data source.";

            lblDatasource.Text = $"{_dataSource.Name}:";
            txtDatasource.Enabled = _dataSource.SearchEnabled;
            SetDatasourceLink(txtGoodreads.Text);
            SetSelectedDatasource();
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
            if (((RadioButton)sender).Text == rdoFile.Text)
            {
                txtXMLFile.Enabled = true;
                btnBrowseXML.Visible = true;
                btnDownloadTerms.Visible = false;
            }
            else
            {
                txtXMLFile.Enabled = false;
                btnBrowseXML.Visible = false;
                btnDownloadTerms.Visible = true;
            }
            SetDatasourceLabels();
        }

        private void ClearInterface(bool bookOpened)
        {
            if (!bookOpened)
            {
                // todo centralized name (should the app name be localized?)
                Text = "X-Ray Builder GUI";
                txtMobi.Text = "";
                txtOutput.Clear();
            }
            else
                Text = $"X-Ray Builder GUI - {txtMobi.Text}";

            txtGoodreads.Text = "";
            pbCover.Image = Resources.missing_cover;
            txtTitle.Text = "";
            txtAuthor.Text = "";
            txtAsin.Text = "";
            txtXMLFile.Text = "";
            // todo another copy
            txtDatasource.Text = "Search datasource...";
            prgBar.Value = 0;
            _openedMetadata = null;
            CheckFiles("","","","","","");
        }

        private async void txtMobi_TextChanged(object sender, EventArgs e)
        {
            if (txtMobi.Text == "" || !File.Exists(txtMobi.Text))
            {
                ClearInterface(false);
                return;
            }
            ClearInterface(true);

            ToggleInterface(false);

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

            //TODO: Check ASIN matches selected Amazon region.
            //try
            //{
            //    _logger.Log(@$"Checking if this ASIN matches a book available on Amazon{_settings.amazonTLD}...");
            //    var localBookResult = await _amazonClient.SearchBook(metadata.Title, metadata.Author, _settings.amazonTLD, _cancelTokens.Token);
            //    _logger.Log(localBookResult.Asin == metadata.Asin
            //        ? $@"Successfully found a matching book:  {localBookResult.Title}!"
            //        : $"Warning: {localBookResult.Title} by {localBookResult.Author} was found, but the ASIN does not match for your selected Amazon region!\n\rURL: {localBookResult.AmazonUrl}");
            //}
            //catch (Exception)
            //{
            //    // Ignored
            //}

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
            ToggleInterface(true);
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
                MessageBox.Show(MainStrings.BookNotFound, MainStrings.BookNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!Directory.Exists(_settings.outDir))
            {
                MessageBox.Show(MainStrings.OutputDirNotFoundReviewSettings, MainStrings.OutputDirNotFoundTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _logger.Log(MainStrings.ExtractingRawMl);
            using var metadata = MetadataLoader.Load(txtMobi.Text);
            var rawMlPath = _directoryService.GetRawmlPath(txtMobi.Text);
            metadata.SaveRawMl(rawMlPath);
            _logger.Log(string.Format(MainStrings.ExtractedToPath, rawMlPath));
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
            if (string.Equals(txtAsin.Text, "ASIN"))
                return;
            Process.Start(_amazonClient.Url(_settings.amazonTLD, txtAsin.Text));
        }

        private void btnExtractTerms_Click(object sender, EventArgs e)
        {
            var selPath = UIFunctions.GetFile(MainStrings.OpenXRayFile, "", "ASC files|*.asc", _settings.outDir);
            if (selPath == "" || !selPath.Contains("XRAY.entities"))
            {
                _logger.Log(MainStrings.InvalidOrNoFileSelected);
                return;
            }
            var newVer = XRayUtil.CheckXRayVersion(selPath);
            if (newVer == XRayUtil.XRayVersion.Invalid)
            {
                _logger.Log(MainStrings.InvalidXRayFile);
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
                _logger.Log(string.Format(MainStrings.CharacterDataExtractedSavedToPath, outfile));
            }
            catch (Exception ex)
            {
                _logger.Log($@"{MainStrings.Error}:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            using var frmCreateXr = _diContainer.GetInstance<frmCreateXR>();
            if (File.Exists(txtMobi.Text))
            {
                using var metadata = await GetAndValidateMetadataAsync(txtMobi.Text, false, _cancelTokens.Token);
                if (metadata != null)
                    frmCreateXr.SetMetadata(metadata);
            }

            frmCreateXr.ShowDialog();
        }

        private void CheckFiles(string author, string title, string asin, string fileName, string databaseName, string guid)
        {
            static Image SetPreviewAndPickImage(ToolStripDropDownItem toolStripDropDownItem, string path)
            {
                var fileExists = File.Exists(path);

                toolStripDropDownItem.Enabled = fileExists;

                return fileExists
                    ? Resources.file_exists
                    : Resources.file_missing;
            }

            pbFile1.Image = SetPreviewAndPickImage(tmiStartAction, _directoryService.GetArtifactPath(ArtifactType.StartActions, author, title, asin, fileName, databaseName, guid, false));
            pbFile2.Image = SetPreviewAndPickImage(tmiAuthorProfile, _directoryService.GetArtifactPath(ArtifactType.AuthorProfile, author, title, asin, fileName, databaseName, guid, false));
            pbFile3.Image = SetPreviewAndPickImage(tmiEndAction, _directoryService.GetArtifactPath(ArtifactType.EndActions, author, title, asin, fileName, databaseName, guid, false));
            pbFile4.Image = SetPreviewAndPickImage(tmiXray, _directoryService.GetArtifactPath(ArtifactType.XRay, author, title, asin, fileName, databaseName, guid, false));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cancelTokens.IsCancellationRequested)
                return;
            _logger.Log("Canceling...");
            _cancelTokens.Cancel();
        }

        private async Task ShowPreviewAsync(PreviewProviderFactory.PreviewType type, string filePath, CancellationToken cancellationToken)
        {
            var previewProvider = _previewProviderFactory.Get(type);

            string selPath;
            if (File.Exists(filePath))
                selPath = filePath;
            else
            {
                selPath = UIFunctions.GetFile(string.Format(MainStrings.OpenKindleFile, previewProvider.Name), "", "ASC files|*.asc", _settings.outDir);
                if (!selPath.Contains(previewProvider.FilenameValidator))
                {
                    _logger.Log(string.Format(MainStrings.InvalidKindleFile, previewProvider.Name));
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
                _logger.Log($@"{MainStrings.Error}:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private async Task CheckAndFixIncorrectAsinOrThrowAsync(IMetadata metadata, string bookPath, CancellationToken cancellationToken)
        {
            if (AmazonClient.IsAsin(metadata.Asin))
                return;

            if (!metadata.CanModify && DialogResult.No == MessageBox.Show(string.Format(MainStrings.InvalidAsinWarning, metadata.Asin), MainStrings.IncorrectAsinTitle, MessageBoxButtons.YesNo))
            {
                throw new Exception($"Invalid Amazon ASIN detected: {metadata.Asin}!\r\nKindle may not display an X-Ray for this book.\r\nYou must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) or a MOBI editor (exth 113 and optionally 504) to change this.");
            }

            var dialogResult = MessageBox.Show(string.Format(MainStrings.InvalidAsinShouldFix, metadata.Asin), MainStrings.IncorrectAsinTitle, MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
                return;

            _logger.Log(string.Format(MainStrings.SearchingAmazonForTitleAuthor, metadata.Title, metadata.Author));
            var amazonSearchResult = await _amazonClient.SearchBook(metadata.Title, metadata.Author, _settings.amazonTLD, cancellationToken);
            if (amazonSearchResult != null)
            {
                // Prompt if book is correct. If not, prompt for manual entry
                dialogResult = MessageBox.Show($@"{MainStrings.FoundBookAmazon}:{Environment.NewLine}{MainStrings.Title}: {amazonSearchResult.Title}{Environment.NewLine}{MainStrings.Author}: {amazonSearchResult.Author}{Environment.NewLine}{MainStrings.Asin}: {amazonSearchResult.Asin}{Environment.NewLine}{Environment.NewLine}{MainStrings.DoesThisSeemCorrect} {MainStrings.ShownAsinUsed}", MainStrings.AmazonSearchResultTitle, MessageBoxButtons.YesNoCancel);
                switch (dialogResult)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                    {
                        metadata.SetAsin(amazonSearchResult.Asin);
                        using var fs = new FileStream(bookPath, FileMode.Create);
                        metadata.Save(fs);
                        _logger.Log(string.Format(MainStrings.UpdatedAsin, metadata.Asin));
                        return;
                    }
                }
            }
            else
                _logger.Log(MainStrings.UnableToAutomaticallyFindAsinOnAmazon);

            // TODO: manual entry
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory(_settings.outDir);
        }

        // todo central paths with directoryservice
        private void btnBrowseDump_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory($@"{Environment.CurrentDirectory}\dmp");
        }

        private void btnBrowseAliasesAndChapters_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory($@"{Environment.CurrentDirectory}\ext");
        }

        private void btnBrowseLogs_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory($@"{Environment.CurrentDirectory}\log");
        }

        private void btnBrowseRecords_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory($@"{Environment.CurrentDirectory}\rec");
        }

        private void btnBrowseTemp_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory($@"{Environment.CurrentDirectory}\tmp");
        }

        private void btnBrowseXmlFolder_Click(object sender, EventArgs e)
        {
            UIFunctions.OpenDirectory($@"{Environment.CurrentDirectory}\xml");
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtOutput.SelectionLength > 0)
                Clipboard.SetText(txtOutput.SelectedText, TextDataFormat.Text);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtOutput.Text = "";
        }

        private void clearAndSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOutput.Text))
                return;
            File.WriteAllText(_currentLog, txtOutput.Text);
            txtOutput.Text = string.Empty;
        }

        private void txtDatasource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (txtDatasource.Text.Contains("Search"))
                return;
            Process.Start(txtGoodreads.Text);
        }

        private void txtGoodreads_TextChanged(object sender, EventArgs e)
        {
            SetDatasourceLink(txtGoodreads.Text);
        }

        private void pbCover_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void pbCover_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void pbCover_DoubleClick(object sender, EventArgs e)
        {
            if (_openedMetadata == null) return;
            using var frmBook = _diContainer.GetInstance<frmBookInfo>();
            frmBook.Setup(_openedMetadata, (Image) pbCover.Image.Clone(), new frmBookInfo.DialogData(_dataSource.Name, txtGoodreads.Text, "", ""));
            frmBook.ShowDialog();
            if (frmBook.Result == null)
                return;

            txtGoodreads.Text = frmBook.Result.SecondarySourceUrl;
            SetDatasourceLabels();
        }

        private void btnViewHelp_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Environment.CurrentDirectory + @"\doc\help.pdf");
            }
            catch
            {
                MessageBox.Show(MainStrings.UnableToOpenHelpDocument, MainStrings.HelpDocumentNotFoundTitle);
            }
        }

        private void btnVisitForum_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.mobileread.com/forums/showthread.php?t=245754");
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }
    }
}