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
        {
            var results = (await dataSource.SearchBookAsync(metadata, cancellationToken)).ToArray();
            if (results.Length == 1)
                return results;

            return results
                .OrderByDescending(book => book.Editions)
                .ThenByDescending(book => book.Reviews)
                .ToArray();
        }
    }
}