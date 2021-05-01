using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Author;

namespace XRayBuilder.Core.Database.Repository
{
    public interface IAuthorRepository
    {
        [ItemCanBeNull]
        Task<AuthorModel> GetByIdAsync(long authorId, CancellationToken cancellationToken);

        [ItemNotNull]
        Task<IEnumerable<AuthorModel>> GetByIdsAsync(IEnumerable<long> authorIds, CancellationToken cancellationToken);

        [ItemCanBeNull]
        Task<AuthorModel> GetByAsinAsync([NotNull] string asin, CancellationToken cancellationToken);

        Task<long> AddOrUpdateAsync([NotNull] AuthorModel authorModel, CancellationToken cancellationToken);
    }
}