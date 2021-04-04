using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilder.Core.Database.Model.BookAuthorMap;

namespace XRayBuilder.Core.Database.Orm.BookAuthorMap
{
    public sealed class BookAuthorMapOrm : IBookAuthorMapOrm
    {
        private readonly IDatabaseConnection _connection;

        private const string Table = "BookAuthorMap";

        public BookAuthorMapOrm(IDatabaseConnection connection)
        {
            _connection = connection;
        }

        public Task<IEnumerable<long>> GetAuthorIdsForBookAsync(long bookId, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                bookId
            };

            return _connection.QueryAsync<long>(@$"
SELECT {nameof(BookAuthorMapModel.AuthorId)} FROM {Table}
WHERE {nameof(BookAuthorMapModel.BookId)} = @{nameof(parameters.bookId)}", parameters, cancellationToken);
        }

        public Task<IEnumerable<long>> GetBookIdsForAuthorAsync(long authorId, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                authorId
            };

            return _connection.QueryAsync<long>(@$"
SELECT {nameof(BookAuthorMapModel.BookId)} FROM {Table}
WHERE {nameof(BookAuthorMapModel.AuthorId)} = @{nameof(parameters.authorId)}", parameters, cancellationToken);
        }

        public async Task UpdateAuthorsForBookAsync(long bookId, long[] authorIds, CancellationToken cancellationToken)
        {
            // Upsert authors for the book
            var parameters = authorIds.Select(authorId => new
            {
                authorId,
                bookId
            }).ToArray();

            // Can't use nameof for the params in the array :(
            await _connection.ExecuteAsync($@"
INSERT INTO {Table}
({nameof(BookAuthorMapModel.AuthorId)}, {nameof(BookAuthorMapModel.BookId)})
VALUES (@authorId, @bookId)
ON CONFLICT ({nameof(BookAuthorMapModel.AuthorId)}, {nameof(BookAuthorMapModel.BookId)})
DO UPDATE SET
{nameof(BookAuthorMapModel.AuthorId)}=@authorId,
{nameof(BookAuthorMapModel.BookId)}=@bookId", parameters, cancellationToken);

            // Remove any authors for the book that weren't included in the upsert
            var parameters2 = new
            {
                bookId,
                authorIds
            };

            await _connection.ExecuteAsync($@"
DELETE FROM {Table}
WHERE {nameof(BookAuthorMapModel.BookId)} = @{nameof(parameters2.bookId)}
AND {nameof(BookAuthorMapModel.AuthorId)} NOT IN @{nameof(parameters2.authorIds)}", parameters2, cancellationToken);
        }
    }
}