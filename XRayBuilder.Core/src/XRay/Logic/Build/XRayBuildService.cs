using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack.KFX;
using Ephemerality.Unpack.Mobi;
using JetBrains.Annotations;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Localization.Core;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Logic.Aliases;

namespace XRayBuilder.Core.XRay.Logic.Build
{
    public sealed class XRayBuildService : IXRayBuildService
    {
        private readonly IMetadataService _metadataService;
        private readonly ILogger _logger;
        private readonly IXRayService _xrayService;
        private readonly IXRayBuilderConfig _config;
        private readonly IDirectoryService _directoryService;
        private readonly IAliasesRepository _aliasesRepository;
        private readonly IKfxXrayService _kfxXrayService;

        public XRayBuildService(
            IMetadataService metadataService,
            ILogger logger,
            IXRayService xrayService,
            IXRayBuilderConfig config,
            IDirectoryService directoryService,
            IAliasesRepository aliasesRepository,
            IKfxXrayService kfxXrayService)
        {
            _metadataService = metadataService;
            _logger = logger;
            _xrayService = xrayService;
            _config = config;
            _directoryService = directoryService;
            _aliasesRepository = aliasesRepository;
            _kfxXrayService = kfxXrayService;
        }

        public sealed class Request
        {
            public Request([NotNull] string bookPath, [CanBeNull] string dataUrl, bool includeTopics, [CanBeNull] string amazonTld, ISecondarySource dataSource)
            {
                BookPath = bookPath;
                DataUrl = dataUrl;
                IncludeTopics = includeTopics;
                AmazonTld = amazonTld;
                DataSource = dataSource;
            }

            [NotNull]
            public string BookPath { get; }

            [CanBeNull]
            public string DataUrl { get; }

            public bool IncludeTopics { get; }

            [CanBeNull]
            public string AmazonTld { get; }

            public ISecondarySource DataSource { get; }
        }

        /// <summary>
        /// Builds an X-Ray file from the parameters given and returns the path at which the file has been saved (or null if something failed)
        /// </summary>
        public async Task<XRay> BuildAsync(Request request, YesNoPrompt yesNoPrompt, YesNoCancelPrompt yesNoCancelPrompt, EditCallback editCallback, IProgressBar progress, CancellationToken cancellationToken)
        {
            using var metadata = await _metadataService.GetAndValidateMetadataAsync(request.BookPath, yesNoCancelPrompt, cancellationToken);
            if (metadata == null)
                return null;

            // Added author name to log output
            _logger.Log($@"{string.Format(CoreStrings.BooksSourceUrl, request.DataSource.Name)}: {request.DataUrl}");
            if (cancellationToken.IsCancellationRequested) return null;
            _logger.Log(CoreStrings.AttemptingBuildXRay);

            var xray = await _xrayService.CreateXRayAsync(request.DataUrl, metadata, request.AmazonTld ?? "com", request.IncludeTopics, request.DataSource, progress, cancellationToken);

            if (!xray.Terms.Any() && yesNoCancelPrompt != null && PromptResultYesNoCancel.Yes != yesNoCancelPrompt(CoreStrings.NoTermsTitle, CoreStrings.NoTermsAvailable, PromptType.Warning))
            {
                _logger.Log($"No terms were available on {request.DataSource.Name}, cancelling the build...");
                return null;
            }

            var aliasPath = _directoryService.GetAliasPath(xray.Asin);
            _xrayService.ExportAndDisplayTerms(xray, request.DataSource, _config.OverwriteAliases, _config.SplitAliases);

            if (_config.EnableEdit && yesNoPrompt != null && PromptResultYesNo.Yes == yesNoPrompt(CoreStrings.Aliases, $@"{CoreStrings.TermsExportedOrAlreadyExist}{Environment.NewLine}{CoreStrings.OpenForEditing}{Environment.NewLine}{CoreStrings.SeeMobilereads}", PromptType.Question))
                return null;

            if (xray.Terms.Any(term => term.Aliases?.Count > 0))
                _logger.Log(CoreStrings.AliasesReadFromXml);
            else if (!File.Exists(aliasPath))
                _logger.Log(CoreStrings.AliasesFileNotFound);
            else
            {
                _aliasesRepository.LoadAliasesForXRay(xray);
                _logger.Log(string.Format(CoreStrings.AliasesReadFrom, aliasPath));
            }

            _logger.Log(CoreStrings.InitialXRayBuiltAddingChapters);
            //Expand the X-Ray file from the unpacked mobi
            Task buildTask;
            switch (metadata)
            {
                case MobiMetadata _:
                    // ReSharper disable twice AccessToDisposedClosure
                    // todo just pass metadata instead of calling getrawmlstream
                    buildTask = Task.Run(() => _xrayService.ExpandFromRawMl(xray, metadata, metadata.GetRawMlStream(), yesNoPrompt, editCallback, progress, cancellationToken), cancellationToken);
                    break;
                case KfxContainer kfx:
                    if (!_config.UseNewVersion)
                        throw new Exception(CoreStrings.BuildingOldFormatNotSupported);

                    buildTask = Task.Run(() => _kfxXrayService.AddLocations(xray, kfx, progress, cancellationToken), cancellationToken);
                    break;
                default:
                    throw new NotSupportedException();
            }

            await buildTask.ConfigureAwait(false);

            return xray;
        }
    }
}