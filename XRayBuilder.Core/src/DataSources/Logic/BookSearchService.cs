using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.DataSources.Logic
{
    /// <summary>
    /// Logic for handling book searches that involves more than just returning the raw results
    /// </summary>
    [UsedImplicitly]
    public class BookSearchService : IBookSearchService
    {
        public async Task<BookInfo[]> SearchSecondarySourceAsync(ISecondarySource dataSource, Parameters parameters, CancellationToken cancellationToken = default)
        {
            var books = new BookInfo[0];
            // If ASIN is available, use it to search first before falling back to author/title
            if (!string.IsNullOrEmpty(parameters.Asin))
                books = (await dataSource.SearchBookByAsinAsync(parameters.Asin, cancellationToken)).ToArray();

            if (books.Length <= 0)
                books = (await dataSource.SearchBookAsync(parameters.Author, parameters.Title, cancellationToken)).ToArray();

            return books.OrderByDescending(book => book.Reviews)
                .ThenByDescending(book => book.Editions)
                .ToArray();
        }

        public sealed class Parameters
        {
            public string Title { get; set; }
            public string Author { get; set; }
            [CanBeNull]
            public string Asin { get; set; }
        }
    }
}