using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
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
        string SanitizeDataLocation(string dataLocation);
        Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default);
        Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default);
        Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default);
        Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default);
    }

    public abstract class SecondarySource : ISecondarySource
    {
        public abstract string Name { get; }
        public abstract bool SearchEnabled { get; }
        public abstract int UrlLabelPosition { get; }
        public abstract bool SupportsNotableClips { get; }
        public abstract bool IsMatchingUrl(string url);
        public abstract Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default);
        public abstract Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default);
        public abstract Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default);
        public abstract Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default);
        public abstract Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default);

        public virtual string SanitizeDataLocation(string dataLocation)
        {
            if (!dataLocation.ToLower().StartsWith("http://") && !dataLocation.ToLower().StartsWith("https://"))
                dataLocation = $"https://{dataLocation}";
            return dataLocation;
        }
    }
}