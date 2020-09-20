using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.Unpack.KFX;
using XRayBuilder.Core.Unpack.Mobi;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Model.Export;

namespace XRayBuilder.Console.Logic
{
    public sealed class XRay
    {
        private readonly ILogger _logger;
        private readonly IAmazonClient _amazonClient;
        private readonly SecondaryDataSourceFactory _secondaryDataSourceFactory;
        private readonly IXRayService _xrayService;
        private readonly IProgressBar _progress;
        private readonly IAliasesRepository _aliasesRepository;
        private readonly IKfxXrayService _kfxXrayService;
        private readonly XRayExporterFactory _xrayExporterFactory;
        private readonly IDirectoryService _directoryService;

        public XRay(
            ILogger logger,
            IAmazonClient amazonClient,
            SecondaryDataSourceFactory secondaryDataSourceFactory,
            IXRayService xrayService,
            IProgressBar progress,
            IAliasesRepository aliasesRepository,
            IKfxXrayService kfxXrayService,
            XRayExporterFactory xrayExporterFactory,
            IDirectoryService directoryService)
        {
            _logger = logger;
            _amazonClient = amazonClient;
            _secondaryDataSourceFactory = secondaryDataSourceFactory;
            _xrayService = xrayService;
            _progress = progress;
            _aliasesRepository = aliasesRepository;
            _kfxXrayService = kfxXrayService;
            _xrayExporterFactory = xrayExporterFactory;
            _directoryService = directoryService;
        }

        public sealed class Request
        {
            public Request([NotNull] string bookPath, [CanBeNull] string dataUrl, bool includeTopics, bool splitAliases, [CanBeNull] string amazonTld)
            {
                BookPath = bookPath;
                DataUrl = dataUrl;
                IncludeTopics = includeTopics;
                SplitAliases = splitAliases;
                AmazonTld = amazonTld;
            }

            [NotNull]
            public string BookPath { get; }
            [CanBeNull]
            public string DataUrl { get; }
            public bool IncludeTopics { get; }
            public bool SplitAliases { get; }
            [CanBeNull]
            public string AmazonTld { get; }
        }

        /// <summary>
        /// Builds an X-Ray file from the parameters given and returns the path at which the file has been saved (or null if something failed)
        /// </summary>
        public async Task<string> BuildAsync([NotNull] Request request, CancellationToken cancellationToken)
        {
            using var metadata = await GetAndValidateMetadataAsync(request.BookPath, cancellationToken);
            if (metadata == null)
                return null;

            var dataSource = string.IsNullOrEmpty(request.DataUrl) || request.DataUrl == SecondarySourceRoentgen.FakeUrl
                ? _secondaryDataSourceFactory.Get(SecondaryDataSourceFactory.Enum.Roentgen)
                : _secondaryDataSourceFactory.GetInferredSource(request.DataUrl);
            if (dataSource == null)
            {
                _logger.Log("Data source could not be determined from the given path.");
                return null;
            }

            Core.XRay.XRay xray;
            try
            {
                xray = await _xrayService.CreateXRayAsync(request.DataUrl, metadata.DbName, metadata.UniqueId, metadata.Asin, request.AmazonTld ?? "com", request.IncludeTopics, dataSource, _progress, cancellationToken);

                if (xray.Terms.Count == 0)
                {
                    _logger.Log($"No terms were available on {dataSource.Name}, cancelling the build...");
                    return null;
                }

                var aliasPath = _directoryService.GetAliasPath(xray.Asin);
                _xrayService.ExportAndDisplayTerms(xray, dataSource, aliasPath, false, request.SplitAliases);

                if (xray.Terms.Any(term => term.Aliases?.Count > 0))
                    _logger.Log("Character aliases read from the XML file.");
                else if (!File.Exists(aliasPath))
                    _logger.Log("Aliases file not found.");
                else
                {
                    _aliasesRepository.LoadAliasesForXRay(xray);
                    _logger.Log($"Character aliases read from {aliasPath}.");
                }

                _logger.Log("Initial X-Ray built, adding locations and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                Task buildTask = metadata switch
                {
                    // ReSharper disable AccessToDisposedClosure
                    Metadata _ => Task.Run(() => _xrayService.ExpandFromRawMl(xray, metadata, metadata.GetRawMlStream(), true, true, 25, true, null, _progress, cancellationToken, true, false), cancellationToken),
                    KfxContainer kfx => Task.Run(() => _kfxXrayService.AddLocations(xray, kfx, true, 25, _progress, cancellationToken), cancellationToken),
                    _ => throw new NotSupportedException()
                };
                await buildTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Build canceled.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred while building the X-Ray:\r\n{ex.Message}\r\n{ex.StackTrace}");
                return null;
            }

            _logger.Log("Saving X-Ray to file...");
            var xrayPath = _directoryService.GetArtifactPath(ArtifactType.XRay, metadata, Path.GetFileNameWithoutExtension(request.BookPath), true);

            try
            {
                var xrayExporter = _xrayExporterFactory.Get(XRayExporterFactory.Enum.Sqlite);
                xrayExporter.Export(xray, xrayPath, _progress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Building canceled.");
                return null;
            }
            catch (Exception ex)
            {
                // TODO: Add option to retry maybe?
                _logger.Log($"An error occurred while creating the X-Ray file. Is it opened in another program?\r\n{ex.Message}");
                return null;
            }

            _logger.Log($"X-Ray file created successfully!\r\nSaved to {xrayPath}");

            return xrayPath;
        }

        private async Task<IMetadata> GetAndValidateMetadataAsync(string mobiFile, CancellationToken cancellationToken)
        {
            _logger.Log("Extracting metadata...");
            try
            {
                var metadata = MetadataLoader.Load(mobiFile);
                try
                {
                    await CheckAndFixIncorrectAsinOrThrowAsync(metadata, mobiFile, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failed to validate/fix ASIN: {ex.Message}\r\nContinuing anyway...", LogLevel.Error);
                }

                _logger.Log($"Got metadata!\r\nDatabase Name: {metadata.DbName}\r\nUniqueID: {metadata.UniqueId}\r\nASIN: {metadata.Asin}");

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred extracting metadata: {ex.Message}\r\n{ex.StackTrace}");
            }

            return null;
        }

        private async Task CheckAndFixIncorrectAsinOrThrowAsync(IMetadata metadata, string bookPath, CancellationToken cancellationToken)
        {
            if (AmazonClient.IsAsin(metadata.Asin))
                return;

            // todo add check for fixasin flag
            if (!metadata.CanModify)
                throw new Exception($"Invalid Amazon ASIN detected: {metadata.Asin}!\r\nKindle may not display an X-Ray for this book.\r\nYou must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) or a MOBI editor (exth 113 and optionally 504) to change this.");

            _logger.Log($"Invalid Amazon ASIN detected: {metadata.Asin}!\nKindle may not display an X-Ray for this book.\r\nAttempting to fix it automatically...");
            _logger.Log($"Searching Amazon for {metadata.Title} by {metadata.Author}...");
            // todo config tld
            var amazonSearchResult = await _amazonClient.SearchBook(metadata.Title, metadata.Author, "com", cancellationToken);
            if (amazonSearchResult != null)
            {
                metadata.SetAsin(amazonSearchResult.Asin);
#if NETCOREAPP3_1
                await using var fs = new FileStream(bookPath, FileMode.Create);
#else
                using var fs = new FileStream(bookPath, FileMode.Create);
#endif
                metadata.Save(fs);
                _logger.Log($"Successfully updated the ASIN to {metadata.Asin}! Be sure to copy this new version of the book to your Kindle device.");
            }
            else
                _logger.Log("Unable to automatically find a matching ASIN for this book on Amazon :(");
        }
    }
}