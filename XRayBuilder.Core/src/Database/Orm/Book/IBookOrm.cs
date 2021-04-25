using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Book;

namespace XRayBuilder.Core.Database.Orm.Book
{
    public interface IBookOrm
    {
        [ItemCanBeNull]
        Task<BookModel> GetByIdAsync(long bookId, CancellationToken cancellationToken);

        [ItemNotNull]
        Task<IEnumerable<BookModel>> GetByIdsAsync(long[] bookIds, CancellationToken cancellationToken);

        [ItemCanBeNull]
        Task<BookModel> GetByAsinAsync([NotNull] string asin, CancellationToken cancellationToken);

        Task<long> UpdateAsync([NotNull] BookModel bookModel, CancellationToken cancellationToken);

        Task<long> UpsertAsync([NotNull] BookModel bookModel, CancellationToken cancellationToken);
    }
}