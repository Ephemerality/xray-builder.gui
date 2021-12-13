using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.XRay.Logic.Build;
using XRayBuilder.Core.XRay.Model.Export;

namespace XRayBuilder.Console.Logic
{
    public sealed class XRay
    {
        private readonly ILogger _logger;
        private readonly SecondaryDataSourceFactory _secondaryDataSourceFactory;
        private readonly IProgressBar _progress;
        private readonly XRayExporterFactory _xrayExporterFactory;
        private readonly IDirectoryService _directoryService;
        private readonly IXRayBuildService _xrayBuildService;

        public XRay(
            ILogger logger,
            SecondaryDataSourceFactory secondaryDataSourceFactory,
            IProgressBar progress,
            XRayExporterFactory xrayExporterFactory,
            IDirectoryService directoryService,
            IXRayBuildService xrayBuildService)
        {
            _logger = logger;
            _secondaryDataSourceFactory = secondaryDataSourceFactory;
            _progress = progress;
            _xrayExporterFactory = xrayExporterFactory;
            _directoryService = directoryService;
            _xrayBuildService = xrayBuildService;
        }

        public sealed class Request
        {
            public Request([NotNull] string bookPath, [CanBeNull] string dataUrl, bool includeTopics, [CanBeNull] string amazonTld)
            {
                BookPath = bookPath;
                DataUrl = dataUrl;
                IncludeTopics = includeTopics;
                AmazonTld = amazonTld;
            }

            [NotNull]
            public string BookPath { get; }
            [CanBeNull]
            public string DataUrl { get; }
            public bool IncludeTopics { get; }
            [CanBeNull]
            public string AmazonTld { get; }
        }

        /// <summary>
        /// Builds an X-Ray file from the parameters given and returns the path at which the file has been saved (or null if something failed)
        /// </summary>
        public async Task<string> BuildAsync([NotNull] Request request, CancellationToken cancellationToken)
        {
            var dataSource = string.IsNullOrEmpty(request.DataUrl) || request.DataUrl == SecondarySourceRoentgen.FakeUrl
                ? _secondaryDataSourceFactory.Get(SecondaryDataSourceFactory.Enum.Roentgen)
                : _secondaryDataSourceFactory.GetInferredSource(request.DataUrl);
            if (dataSource == null)
            {
                _logger.Log("Data source could not be determined from the given path.");
                return null;
            }

            try
            {
                var xray = await _xrayBuildService.BuildAsync(
                    new XRayBuildService.Request(request.BookPath, request.DataUrl, request.IncludeTopics, request.AmazonTld ?? "com", dataSource),
                    null,
                    null,
                    null,
                    _progress,
                    cancellationToken
                );

                _logger.Log("Saving X-Ray to file...");
                var xrayPath = _directoryService.GetArtifactPath(ArtifactType.XRay, xray.Author, xray.Title, xray.Asin, Path.GetFileNameWithoutExtension(request.BookPath), xray.DatabaseName, xray.Guid, true);
                var xrayExporter = _xrayExporterFactory.Get(XRayExporterFactory.Enum.Sqlite);
                xrayExporter.Export(xray, xrayPath, _progress, cancellationToken);
                _logger.Log($"X-Ray file created successfully!\r\nSaved to {xrayPath}");

                return xrayPath;
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
        }
    }
}