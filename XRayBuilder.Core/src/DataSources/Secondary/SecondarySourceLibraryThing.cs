using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceLibraryThing : ISecondarySource
    {
        public string Name { get; } = "LibraryThing";
        public bool SearchEnabled { get; } = true;
        public int UrlLabelPosition { get; } = 6;
        public bool SupportsNotableClips { get; } = true;

        public Task<IEnumerable<BookInfo>> SearchBookAsync(string author, string title, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<BookInfo>> SearchBookByAsinAsync(string asin, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}