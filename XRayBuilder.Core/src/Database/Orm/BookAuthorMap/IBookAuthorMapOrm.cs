using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilder.Core.Database.Orm.BookAuthorMap
{
    public interface IBookAuthorMapOrm
    {
        Task<IEnumerable<long>> GetAuthorIdsForBookAsync(long bookId, CancellationToken cancellationToken);
        Task<IEnumerable<long>> GetBookIdsForAuthorAsync(long authorId, CancellationToken cancellationToken);
        Task UpdateAuthorsForBookAsync(long bookId, long[] authorIds, CancellationToken cancellationToken);
    }
}