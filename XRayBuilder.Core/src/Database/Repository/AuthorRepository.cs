using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Author;
using XRayBuilder.Core.Database.Orm.Author;

namespace XRayBuilder.Core.Database.Repository
{
    /// <summary>
    /// Handle storage for <see cref="AuthorModel"/>
    /// (really just a proxy for <see cref="IAuthorOrm"/> for now for consistency)
    /// </summary>
    public sealed class AuthorRepository
    {
        private readonly IAuthorOrm _authorOrm;

        public AuthorRepository(IAuthorOrm authorOrm)
        {
            _authorOrm = authorOrm;
        }

        public Task<AuthorModel> GetByIdAsync(long authorId, CancellationToken cancellationToken)
            => _authorOrm.GetByIdAsync(authorId, cancellationToken);

        public Task<IEnumerable<AuthorModel>> GetByIdsAsync(IEnumerable<long> authorIds, CancellationToken cancellationToken)
            => _authorOrm.GetByIdsAsync(authorIds, cancellationToken);

        public Task<AuthorModel> GetByAsinAsync([NotNull] string asin, CancellationToken cancellationToken)
            => _authorOrm.GetByAsinAsync(asin, cancellationToken);

        public Task<long> AddOrUpdateAsync([NotNull] AuthorModel authorModel, CancellationToken cancellationToken)
            => _authorOrm.UpsertAsync(authorModel, cancellationToken);
    }
}