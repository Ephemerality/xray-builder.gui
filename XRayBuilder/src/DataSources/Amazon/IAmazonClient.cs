using System.Collections.Async;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilderGUI.DataSources.Amazon.Model;

namespace XRayBuilderGUI.DataSources.Amazon
{
    // todo: client should be http stuff, parser should parse
    public interface IAmazonClient
    {
        string ParseAsin(string input);
        string ParseAsinFromUrl(string input);
        string Url(string tld, string asin);
        Task<AuthorSearchResults> SearchAuthor(BookInfo curBook, string TLD, ILogger _logger, CancellationToken cancellationToken = default);
        HtmlNode GetBioNode(AuthorSearchResults searchResults, string TLD);
        HtmlNode GetAuthorImageNode(AuthorSearchResults searchResults, string TLD);

        /// <summary>
        /// As of 2018-07-31, format changed. For some amount of time, keep both just in case.
        /// </summary>
        List<BookInfo> GetAuthorBooksNew(AuthorSearchResults searchResults, string curTitle, string curAuthor, string TLD);

        List<BookInfo> GetAuthorBooks(AuthorSearchResults searchResults, string curTitle, string curAuthor, string TLD);
        Task<BookInfo> SearchBook(string title, string author, string TLD, CancellationToken cancellationToken = default);
        IAsyncEnumerable<BookInfo> EnhanceBookInfos(IEnumerable<BookInfo> books);
        Task<string> DownloadStartActions(string asin, CancellationToken cancellationToken = default);
        Task<NextBookResult> DownloadNextInSeries(string asin, CancellationToken cancellationToken = default);
    }
}