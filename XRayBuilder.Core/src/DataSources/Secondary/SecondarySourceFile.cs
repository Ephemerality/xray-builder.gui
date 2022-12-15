using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Logic.Terms;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceFile : ISecondarySource
    {
        private readonly ILogger _logger;
        private readonly ITermsService _termsService;

        public SecondarySourceFile(ILogger logger, ITermsService termsService)
        {
            _logger = logger;
            _termsService = termsService;
        }

        public string Name => "file";
        public bool SearchEnabled => false;
        public int UrlLabelPosition => 0;
        public bool SupportsNotableClips => false;
        public string SanitizeDataLocation(string dataLocation) => dataLocation;

        public Task<IEnumerable<Term>> GetTermsAsync(string xmlFile, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            _logger.Log("Loading terms from file...");
            var filetype = Path.GetExtension(xmlFile);
            switch (filetype)
            {
                case ".xml":
                    return Task.FromResult((IEnumerable<Term>) XmlUtil.DeserializeFile<Term[]>(xmlFile));
                case ".txt":
                    return Task.FromResult(_termsService.ReadTermsFromTxt(xmlFile));
                default:
                    _logger.Log($"Error: Bad file type \"{filetype}\"");
                    break;
            }

            return Task.FromResult(Enumerable.Empty<Term>());
        }

        public bool IsMatchingUrl(string url)
        {
            var lower = url.ToLowerInvariant();
            return File.Exists(url) && (lower.EndsWith(".xml") || lower.EndsWith(".txt"));
        }

        public string GetIdFromUrl(string url) => null;

        #region Unsupported

        public Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}