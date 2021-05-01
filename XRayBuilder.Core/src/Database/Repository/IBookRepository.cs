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

        Task AddOrUpdateAsync(Book book, CancellationToken cancellationToken);
    }
}