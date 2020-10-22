using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.DataSources.Amazon
{
    // todo: client should be http stuff, parser should parse
    public interface IAmazonClient
    {
        [CanBeNull]
        string ParseAsin(string input);
        [CanBeNull]
        string ParseAsinFromUrl(string input);
        string Url(string tld, string asin);
        Task<AuthorSearchResults> SearchAuthor(string author, string TLD, CancellationToken cancellationToken, bool enableLog = true);
        Task<BookInfo> SearchBook(string title, string author, string TLD, CancellationToken cancellationToken);
        IAsyncEnumerable<BookInfo> EnhanceBookInfos(IEnumerable<BookInfo> books, CancellationToken cancellationToken);
    }
}