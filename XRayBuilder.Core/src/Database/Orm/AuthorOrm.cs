using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model;

namespace XRayBuilder.Core.Database.Orm
{
    public sealed class AuthorOrm
    {
        private readonly IDatabaseConnection _connection;

        private const string Table = "Author";

        public AuthorOrm(IDatabaseConnection connection)
        {
            _connection = connection;
        }

        [ItemCanBeNull]
        public async Task<AuthorModel> GetByIdAsync(long authorId, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                authorId
            };

            var query = $"SELECT * FROM {Table} WHERE {nameof(AuthorModel.AuthorId)} = @{nameof(authorId)}";

            return (await _connection.QueryAsync<AuthorModel>(query, parameters, cancellationToken)).SingleOrDefault();
        }

        [ItemCanBeNull]
        public async Task<AuthorModel> GetByAsinAsync([NotNull] string asin, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                asin
            };

            var query = $"SELECT * FROM {Table} WHERE {nameof(AuthorModel.Asin)} = @{nameof(asin)}";

            return (await _connection.QueryAsync<AuthorModel>(query, parameters, cancellationToken)).SingleOrDefault();
        }

        public Task<AuthorModel> InsertAsync([NotNull] AuthorModel authorModel, CancellationToken cancellationToken)
        {
            
        }

        public async Task<long> UpdateAsync([NotNull] AuthorModel authorModel, CancellationToken cancellationToken)
        {
            if (authorModel.AuthorId == default)
                throw new ArgumentException(@"Author ID not set", nameof(authorModel));

            return authorModel.AuthorId;
        }

        public Task<long> UpsertAsync([NotNull] AuthorModel authorModel, CancellationToken cancellationToken)
        {
            if (authorModel.AuthorId != default)
                return UpdateAsync(authorModel, cancellationToken);

            var query = $"INSERT INTO {Table} (columns) VALUES (values) ON CONFLICT ({nameof(AuthorModel.Asin)}) DO UPDATE SET updatestuff; SELECT LAST_INSERT_ROWID() AS id";

            return _connection.ExecuteScalarAsync<long>(query, authorModel, cancellationToken);
        }
    }
}