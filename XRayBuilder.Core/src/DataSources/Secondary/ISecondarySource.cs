using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    // TODO Consider splitting sources into terms/xray vs metadata/extras
    public interface ISecondarySource
    {
        string Name { get; }
        bool SearchEnabled { get; }
        int UrlLabelPosition { get; }
        bool SupportsNotableClips { get; }
        bool IsMatchingUrl(string url);
        /// <summary>
        /// Parse a book's ID from a <paramref name="url"/>
        /// </summary>
        [CanBeNull]
        string GetIdFromUrl(string url);
        Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default);
        Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default);
        Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default);
        Task GetExtrasAsync(BookInfo curBook, [CanBeNull] IProgressBar progress = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, [CanBeNull] IProgressBar progress, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, [CanBeNull] IProgressBar progress = null, CancellationToken cancellationToken = default);

        public string SanitizeDataLocation(string dataLocation)
        {
            if (!dataLocation.ToLower().StartsWith("http://") && !dataLocation.ToLower().StartsWith("https://"))
                dataLocation = $"https://{dataLocation}";
            return dataLocation;
        }
    }
}
