using System.Collections.Async;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilderGUI.DataSources.Amazon.Model;

namespace XRayBuilderGUI.DataSources.Amazon
{
    // todo: client should be http stuff, parser should parse
    public interface IAmazonClient
    {
        string ParseAsin(string input);
        string ParseAsinFromUrl(string input);
        string Url(string tld, string asin);
        Task<AuthorSearchResults> SearchAuthor(string author, string bookAsin, string TLD, CancellationToken cancellationToken = default);
        Task<BookInfo> SearchBook(string title, string author, string TLD, CancellationToken cancellationToken = default);
        IAsyncEnumerable<BookInfo> EnhanceBookInfos(IEnumerable<BookInfo> books);
        Task<string> DownloadStartActions(string asin, CancellationToken cancellationToken = default);
        Task<NextBookResult> DownloadNextInSeries(string asin, CancellationToken cancellationToken = default);
    }
}