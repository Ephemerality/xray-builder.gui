using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceRoentgen : SecondarySource
    {
        private readonly IRoentgenClient _roentgenClient;
        private readonly ILogger _logger;

        public SecondarySourceRoentgen(IRoentgenClient roentgenClient, ILogger logger)
        {
            _roentgenClient = roentgenClient;
            _logger = logger;
        }

        public override string Name { get; } = "Roentgen";
        public override bool SearchEnabled { get; } = false;
        public override int UrlLabelPosition { get; } = 0;
        public override bool SupportsNotableClips { get; } = false;

        public const string FakeUrl = "https://roentgen";

        public override bool IsMatchingUrl(string url)
        {
            return url.Equals(FakeUrl);
        }

        public override Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public override Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public override Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public override Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public override async Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
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

        public override Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
