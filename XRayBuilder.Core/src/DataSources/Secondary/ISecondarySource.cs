using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    // TODO Consider splitting sources into terms/xray vs metadata/extras
    // TODO Factory based on URL?
    public interface ISecondarySource
    {
        string Name { get; }
        bool SearchEnabled { get; }
        int UrlLabelPosition { get; }
        bool SupportsNotableClips { get; }
        Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default);
        Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default);
        Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default);
        Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default);
    }
}
