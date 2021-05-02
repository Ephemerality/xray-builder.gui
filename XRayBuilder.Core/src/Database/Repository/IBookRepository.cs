using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Book;

namespace XRayBuilder.Core.Database.Repository
{
    public interface IBookRepository
    {
        [ItemCanBeNull]
        Task<Book> GetByAsinAsync([NotNull] string asin, CancellationToken cancellationToken);

        /// <summary>
        /// Adds <paramref name="book"/> to the database or updates an existing one if there is an ASIN match.
        /// Also adds/updates the author table with <see cref="Book.Authors"/>
        /// </summary>
        Task AddOrUpdateAsync(Book book, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a set of <paramref name="books"/> to the database or updates existing entries.
        /// Also adds/updates the author table with the authors from each <see cref="Book"/>.
        /// - Each distinct author (by ASIN) is only updated once
        /// - If multiple versions of the author with conflicting info exist in <paramref name="books"/>, only the first is used
        /// </summary>
        Task AddUpOrUpdateBulkAsync(Book[] books, CancellationToken cancellationToken);
    }
}