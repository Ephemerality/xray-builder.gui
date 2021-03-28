using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Author;

namespace XRayBuilder.Core.Database.Orm.Author
{
    public sealed class AuthorOrm : IAuthorOrm
    {
        private readonly IDatabaseConnection _connection;

        private const string Table = "Author";

        public AuthorOrm(IDatabaseConnection connection)
        {
            _connection = connection;
        }

        [ItemCanBeNull]
        public async Task<AuthorModel> GetByIdAsync(long authorId, CancellationToken cancellationToken)
            => (await GetByIdsAsync(new [] {authorId}, cancellationToken)).SingleOrDefault();

        [ItemNotNull]
        public Task<IEnumerable<AuthorModel>> GetByIdsAsync(IEnumerable<long> authorIds, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                authorIds
            };

            var query = $"SELECT * FROM {Table} WHERE {nameof(AuthorModel.AuthorId)} IN @{nameof(authorIds)}";

            return _connection.QueryAsync<AuthorModel>(query, parameters, cancellationToken);
        }

        [ItemCanBeNull]
        public async Task<AuthorModel> GetByAsinAsync(string asin, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                asin
            };

            var query = $"SELECT * FROM {Table} WHERE {nameof(AuthorModel.Asin)} = @{nameof(asin)}";

            return (await _connection.QueryAsync<AuthorModel>(query, parameters, cancellationToken)).SingleOrDefault();
        }

        public async Task<long> UpdateAsync(AuthorModel authorModel, CancellationToken cancellationToken)
        {
            if (authorModel.AuthorId == default)
                throw new ArgumentException(@"Author ID not set", nameof(authorModel));

            var query = $@"UPDATE {Table} SET
{nameof(AuthorModel.Asin)}=@{nameof(AuthorModel.Asin)},
{nameof(AuthorModel.Name)}=@{nameof(AuthorModel.Name)},
{nameof(AuthorModel.Biography)}=@{nameof(AuthorModel.Biography)},
{nameof(AuthorModel.ImageUrl)}=@{nameof(AuthorModel.ImageUrl)}
WHERE {nameof(authorModel.AuthorId)}=@{nameof(AuthorModel.AuthorId)}";

            await _connection.ExecuteScalarAsync<AuthorModel>(query, authorModel, cancellationToken);

            return authorModel.AuthorId;
        }

        public Task<long> UpsertAsync([NotNull] AuthorModel authorModel, CancellationToken cancellationToken)
        {
            if (authorModel.AuthorId != default)
                return UpdateAsync(authorModel, cancellationToken);

            var query = $@"INSERT INTO {Table}
({nameof(AuthorModel.Asin)}, {nameof(AuthorModel.Name)}, {nameof(AuthorModel.Biography)}, {nameof(AuthorModel.ImageUrl)})
VALUES (@{nameof(AuthorModel.Asin)}, @{nameof(AuthorModel.Name)}, @{nameof(AuthorModel.Biography)}, @{nameof(AuthorModel.ImageUrl)})
ON CONFLICT ({nameof(AuthorModel.Asin)})
DO UPDATE SET 
{nameof(AuthorModel.Asin)}=@{nameof(AuthorModel.Asin)},
{nameof(AuthorModel.Name)}=@{nameof(AuthorModel.Name)},
{nameof(AuthorModel.Biography)}=@{nameof(AuthorModel.Biography)},
{nameof(AuthorModel.ImageUrl)}=@{nameof(AuthorModel.ImageUrl)};
SELECT LAST_INSERT_ROWID() AS id";

            return _connection.ExecuteScalarAsync<long>(query, authorModel, cancellationToken);
        }
    }
}