using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.DataSources.Logic
{
    /// <summary>
    /// Logic for handling book searches that involves more than just returning the raw results
    /// </summary>
    [UsedImplicitly]
    public class BookSearchService : IBookSearchService
    {
        public async Task<BookInfo[]> SearchSecondarySourceAsync(ISecondarySource dataSource, IMetadata metadata, CancellationToken cancellationToken = default)
            => (await dataSource.SearchBookAsync(metadata, cancellationToken))
                .OrderByDescending(book => book.Reviews)
                .ThenByDescending(book => book.Editions)
                .ToArray();

        public sealed class Parameters
        {
            public string Title { get; set; }
            public string Author { get; set; }
            [CanBeNull]
            public string Asin { get; set; }
        }
    }
}