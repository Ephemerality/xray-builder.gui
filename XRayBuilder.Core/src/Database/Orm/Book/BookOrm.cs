using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilder.Core.Database.Model.Book;

namespace XRayBuilder.Core.Database.Orm.Book
{
    public sealed class BookOrm : IBookOrm
    {
        private readonly IDatabaseConnection _connection;

        private const string Table = "Book";

        public BookOrm(IDatabaseConnection connection)
        {
            _connection = connection;
        }

        public async Task<BookModel> GetByIdAsync(long bookId, CancellationToken cancellationToken)
            => (await GetByIdsAsync(new [] {bookId}, cancellationToken)).SingleOrDefault();

        public Task<IEnumerable<BookModel>> GetByIdsAsync(long[] bookIds, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                bookIds
            };

            var query = $"SELECT * FROM {Table} WHERE {nameof(BookModel.BookId)} IN @{nameof(bookIds)}";

            return _connection.QueryAsync<BookModel>(query, parameters, cancellationToken);
        }

        public async Task<BookModel> GetByAsinAsync(string asin, CancellationToken cancellationToken)
        {
            var parameters = new
            {
                asin
            };

            var query = $"SELECT * FROM {Table} WHERE {nameof(BookModel.Asin)} = @{nameof(asin)}";

            return (await _connection.QueryAsync<BookModel>(query, parameters, cancellationToken)).SingleOrDefault();
        }

        public async Task<long> UpdateAsync(BookModel bookModel, CancellationToken cancellationToken)
        {
            if (bookModel.BookId == default)
                throw new ArgumentException(@"Book ID not set", nameof(bookModel));

            var query = $@"UPDATE {Table} SET
{nameof(BookModel.Title)}=@{nameof(BookModel.Title)},
{nameof(BookModel.Asin)}=@{nameof(BookModel.Asin)},
{nameof(BookModel.GoodreadsId)}=@{nameof(BookModel.GoodreadsId)},
{nameof(BookModel.LibraryThingId)}=@{nameof(BookModel.LibraryThingId)}
{nameof(BookModel.ShelfariId)}=@{nameof(BookModel.ShelfariId)}
{nameof(BookModel.Isbn)}=@{nameof(BookModel.Isbn)}
WHERE {nameof(bookModel.BookId)}=@{nameof(BookModel.BookId)}";

            await _connection.ExecuteScalarAsync<BookModel>(query, bookModel, cancellationToken);

            return bookModel.BookId;
        }

        public Task<long> UpsertAsync(BookModel bookModel, CancellationToken cancellationToken)
        {
            if (bookModel.BookId != default)
                return UpdateAsync(bookModel, cancellationToken);

            var query = $@"INSERT INTO {Table}
({nameof(BookModel.Title)}, {nameof(BookModel.Asin)}, {nameof(BookModel.GoodreadsId)}, {nameof(BookModel.LibraryThingId)}), {nameof(BookModel.ShelfariId)}), {nameof(BookModel.Isbn)})
VALUES (@{nameof(BookModel.Title)}, @{nameof(BookModel.Asin)}, @{nameof(BookModel.GoodreadsId)}, @{nameof(BookModel.LibraryThingId)}, @{nameof(BookModel.ShelfariId)}, @{nameof(BookModel.Isbn)})
ON CONFLICT ({nameof(BookModel.Asin)})
DO UPDATE SET
{nameof(BookModel.Title)}=@{nameof(BookModel.Title)},
{nameof(BookModel.Asin)}=@{nameof(BookModel.Asin)},
{nameof(BookModel.GoodreadsId)}=@{nameof(BookModel.GoodreadsId)},
{nameof(BookModel.LibraryThingId)}=@{nameof(BookModel.LibraryThingId)}
{nameof(BookModel.ShelfariId)}=@{nameof(BookModel.ShelfariId)}
{nameof(BookModel.Isbn)}=@{nameof(BookModel.Isbn)}
SELECT LAST_INSERT_ROWID() AS id";

            return _connection.ExecuteScalarAsync<long>(query, bookModel, cancellationToken);
        }
    }
}