using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Console.Logic
{
    public sealed class XRay
    {
        private readonly ILogger _logger;
        private readonly IAmazonClient _amazonClient;

        public XRay(ILogger logger, IAmazonClient amazonClient)
        {
            _logger = logger;
            _amazonClient = amazonClient;
        }

        public async Task Build(CancellationToken cancellationToken)
        {
            using var metadata = await GetAndValidateMetadataAsync("", cancellationToken);
            if (metadata == null)
                return;
            
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
                await using var fs = new FileStream(bookPath, FileMode.Create);
                metadata.Save(fs);
                _logger.Log($"Successfully updated the ASIN to {metadata.Asin}! Be sure to copy this new version of the book to your Kindle device.");
            }
            else
                _logger.Log("Unable to automatically find a matching ASIN for this book on Amazon :(");
        }
    }
}