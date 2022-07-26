using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Prompt;
using XRayBuilder.Core.Localization.Core;

namespace XRayBuilder.Core.Logic
{
    public sealed class MetadataService : IMetadataService
    {
        private readonly ILogger _logger;
        private readonly IXRayBuilderConfig _config;
        private readonly IAmazonClient _amazonClient;
        private readonly IDirectoryService _directoryService;

        public MetadataService(ILogger logger, IXRayBuilderConfig config, IAmazonClient amazonClient, IDirectoryService directoryService)
        {
            _logger = logger;
            _config = config;
            _amazonClient = amazonClient;
            _directoryService = directoryService;
        }

        public async Task<IMetadata> GetAndValidateMetadataAsync(string mobiFile, YesNoCancelPrompt yesNoCancelPrompt, CancellationToken cancellationToken)
        {
            _logger.Log(CoreStrings.ExtractingMetadata);
            try
            {
                var metadata = MetadataReader.Load(mobiFile);
                EbokTagPromptOrThrow(metadata, mobiFile, yesNoCancelPrompt);
                try
                {
                    await CheckAndFixIncorrectAsinOrThrowAsync(metadata, mobiFile, yesNoCancelPrompt, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Log($@"{CoreStrings.FailedToValidateAsin}: {ex.Message}\r\n{CoreStrings.ContinuingAnyway}...", LogLevel.Error);
                }

                if (_config.SaveRawl && metadata.RawMlSupported)
                {
                    _logger.Log(CoreStrings.SavingRawml);
                    metadata.SaveRawMl(_directoryService.GetRawmlPath(mobiFile));
                }

                if (!_config.UseNewVersion && metadata.DbName.Length == 31)
                    _logger.Log(string.Format(CoreStrings.DatabaseNameLengthWarning, metadata.DbName));

                _logger.Log($@"{CoreStrings.GotMetadata}{Environment.NewLine}ASIN: {metadata.Asin}");

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.Log($@"{CoreStrings.ErrorExtractingMetadata}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return null;
        }

        public async Task CheckAndFixIncorrectAsinOrThrowAsync(IMetadata metadata, string bookPath, YesNoCancelPrompt yesNoCancelPrompt, CancellationToken cancellationToken)
        {
            if (AmazonClient.IsAsin(metadata.Asin))
                return;

            if (!metadata.CanModify)
            {
                switch (yesNoCancelPrompt(CoreStrings.IncorrectAsinTitle, string.Format(CoreStrings.InvalidAsinWarning, metadata.Asin), PromptType.Warning))
                {
                    case PromptResultYesNoCancel.No:
                        throw new Exception($"Invalid Amazon ASIN detected: {metadata.Asin}!\r\nKindle may not display an X-Ray for this book.\r\nYou must either use Calibre's Quality Check plugin (Fix ASIN for Kindle Fire) or a MOBI editor (exth 113 and optionally 504) to change this.");
                    case PromptResultYesNoCancel.Cancel:
                        return;
                }

            }

            if (PromptResultYesNoCancel.Yes != yesNoCancelPrompt(CoreStrings.IncorrectAsinTitle, string.Format(CoreStrings.InvalidAsinShouldFix, metadata.Asin), PromptType.Warning))
                return;

            _logger.Log(string.Format(CoreStrings.SearchingAmazonForTitleAuthor, metadata.Title, metadata.Author));
            var amazonSearchResult = await _amazonClient.SearchBook(metadata.Title, metadata.Author, _config.AmazonTld, cancellationToken);
            if (amazonSearchResult != null)
            {
                // Prompt if book is correct. If not, prompt for manual entry
                switch (yesNoCancelPrompt(CoreStrings.AmazonSearchResultTitle, $@"{CoreStrings.FoundBookAmazon}:{Environment.NewLine}{CoreStrings.Title}: {amazonSearchResult.Title}{Environment.NewLine}{CoreStrings.Author}: {amazonSearchResult.Author}{Environment.NewLine}ASIN: {amazonSearchResult.Asin}{Environment.NewLine}{Environment.NewLine}{CoreStrings.DoesThisSeemCorrect} {CoreStrings.ShownAsinUsed}", PromptType.Info))
                {
                    case PromptResultYesNoCancel.Yes:
                    {
                        metadata.SetAsin(amazonSearchResult.Asin);
                        using var fs = new FileStream(bookPath, FileMode.Create);
                        metadata.Save(fs);
                        _logger.Log(string.Format(CoreStrings.UpdatedAsin, metadata.Asin));
                        return;
                    }
                    case PromptResultYesNoCancel.Cancel:
                        return;
                }
            }
            else
                _logger.Log(CoreStrings.UnableToAutomaticallyFindAsinOnAmazon);

            // TODO: manual entry
        }

        public void EbokTagPromptOrThrow(IMetadata md, string bookPath, YesNoCancelPrompt yesNoCancelPrompt)
        {
            if (md.CdeContentType == "EBOK")
                return;
            if (PromptResultYesNoCancel.Yes != yesNoCancelPrompt("Incorrect Content Type", $"The document type is not set to EBOK. Would you like this to be updated?{Environment.NewLine}Caution: This feature is experimental and could potentially ruin your book file.", PromptType.Warning))
                return;

            try
            {
                using var fs = new FileStream(bookPath, FileMode.Create);
                md.UpdateCdeContentType();
                md.Save(fs);
            }
            catch (IOException ex)
            {
                throw new Exception($"Failed to update Content Type, could not open with write access.{Environment.NewLine}Is the book open in another application?", ex);
            }
        }
    }
}