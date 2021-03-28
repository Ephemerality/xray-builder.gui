using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Author;
using XRayBuilder.Core.Database.Model.Book;
using XRayBuilder.Core.Database.Orm.Author;
using XRayBuilder.Core.Database.Orm.Book;
using XRayBuilder.Core.Database.Orm.BookAuthorMap;

namespace XRayBuilder.Core.Database.Repository
{
    public sealed class BookRepository
    {
        private readonly IBookOrm _bookOrm;
        private readonly IAuthorOrm _authorOrm;
        private readonly IBookAuthorMapOrm _bookAuthorMapOrm;
        private readonly BookConverter _bookConverter = new();
        private readonly IDatabaseConnection _connection;

        public BookRepository(IBookOrm bookOrm, IAuthorOrm authorOrm, IBookAuthorMapOrm bookAuthorMapOrm, IDatabaseConnection connection)
        {
            _bookOrm = bookOrm;
            _authorOrm = authorOrm;
            _bookAuthorMapOrm = bookAuthorMapOrm;
            _connection = connection;
        }

        #region Converter

        private sealed class BookConverter
        {
            public Book ToPoco(BookModel bookModel, IEnumerable<AuthorModel> authorModels)
                => new()
                {
                    Asin = bookModel.Asin,
                    Title = bookModel.Title,
                    Authors = authorModels.ToList(),
                    Isbn = bookModel.Isbn,
                    GoodreadsId = bookModel.GoodreadsId,
                    ShelfariId = bookModel.ShelfariId,
                    LibraryThingId = bookModel.LibraryThingId
                };

            public BookModel ToModel(Book book)
                => new BookModel
                {
                    Asin = book.Asin,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    GoodreadsId = book.GoodreadsId,
                    ShelfariId = book.ShelfariId,
                    LibraryThingId = book.LibraryThingId
                };
        }

        #endregion

        public async Task<Book> GetByAsinAsync([NotNull] string asin, CancellationToken cancellationToken)
        {
            var bookModel = await _bookOrm.GetByAsinAsync(asin, cancellationToken);
            var authorIds = await _bookAuthorMapOrm.GetAuthorsForBookAsync(bookModel.BookId, cancellationToken);
            var authorModels = await _authorOrm.GetByIdsAsync(authorIds, cancellationToken);

            return _bookConverter.ToPoco(bookModel, authorModels);
        }

        public async Task AddOrUpdateAsync(Book book, CancellationToken cancellationToken)
        {
            using var transaction = await _connection.BeginTransactionAsync(cancellationToken);
            var bookId = await _bookOrm.UpsertAsync(_bookConverter.ToModel(book), cancellationToken);

            var authorIds = await new AsyncEnumerable<long>(async yield =>
            {
                foreach (var author in book.Authors)
                    await yield.ReturnAsync(await _authorOrm.UpsertAsync(author, yield.CancellationToken));
            }).ToArrayAsync(cancellationToken);

            await _bookAuthorMapOrm.UpdateAuthorsForBookAsync(bookId, authorIds, cancellationToken);

            transaction.Commit();
        }
    }
}