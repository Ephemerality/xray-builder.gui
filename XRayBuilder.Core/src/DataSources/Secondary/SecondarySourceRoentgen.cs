using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceRoentgen : ISecondarySource
    {
        private readonly IRoentgenClient _roentgenClient;
        private readonly ILogger _logger;

        public SecondarySourceRoentgen(IRoentgenClient roentgenClient, ILogger logger)
        {
            _roentgenClient = roentgenClient;
            _logger = logger;
        }

        public string Name => "Roentgen";
        public bool SearchEnabled => false;
        public int UrlLabelPosition => 0;
        public bool SupportsNotableClips => false;

        public const string FakeUrl = "https://roentgen";

        public bool IsMatchingUrl(string url) => url.Equals(FakeUrl);

        public string GetIdFromUrl(string url) => null;

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

        public async Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            try
            {
                var terms = await _roentgenClient.DownloadTermsAsync(asin, tld, cancellationToken);
                if (terms == null)
                {
                    _logger.Log("No terms were available for this book :(");
                    return Enumerable.Empty<Term>();
                }

                terms = terms.Where(term => term.Type == "character" || includeTopics).ToArray();
                _logger.Log($"Successfully downloaded {terms.Length} terms from Roentgen!");
                return terms;
            }
            catch (Exception e)
            {
                _logger.Log($"Failed to download terms: {e.Message}");
                return Enumerable.Empty<Term>();
            }
        }

        public Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
