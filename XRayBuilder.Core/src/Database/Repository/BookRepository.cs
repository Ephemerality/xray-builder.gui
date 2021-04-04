using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using JetBrains.Annotations;
using XRayBuilder.Core.Database.Model.Author;
using XRayBuilder.Core.Database.Model.Book;
using XRayBuilder.Core.Database.Orm.Book;
using XRayBuilder.Core.Database.Orm.BookAuthorMap;

namespace XRayBuilder.Core.Database.Repository
{
    public sealed class BookRepository
    {
        private readonly IBookOrm _bookOrm;
        private readonly IBookAuthorMapOrm _bookAuthorMapOrm;
        private readonly BookConverter _bookConverter = new();
        private readonly IDatabaseConnection _connection;
        private readonly AuthorRepository _authorRepository;

        public BookRepository(IBookOrm bookOrm, IBookAuthorMapOrm bookAuthorMapOrm, IDatabaseConnection connection, AuthorRepository authorRepository)
        {
            _bookOrm = bookOrm;
            _bookAuthorMapOrm = bookAuthorMapOrm;
            _connection = connection;
            _authorRepository = authorRepository;
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
            var authorIds = await _bookAuthorMapOrm.GetAuthorIdsForBookAsync(bookModel.BookId, cancellationToken);
            var authorModels = await _authorRepository.GetByIdsAsync(authorIds, cancellationToken);

            return _bookConverter.ToPoco(bookModel, authorModels);
        }

        public async Task AddOrUpdateAsync(Book book, CancellationToken cancellationToken)
        {
            using var transaction = await _connection.BeginTransactionAsync(cancellationToken);
            var bookId = await _bookOrm.UpsertAsync(_bookConverter.ToModel(book), cancellationToken);

            var authorIds = await new AsyncEnumerable<long>(async yield =>
            {
                foreach (var author in book.Authors)
                    await yield.ReturnAsync(await _authorRepository.AddOrUpdateAsync(author, yield.CancellationToken));
            }).ToArrayAsync(cancellationToken);

            await _bookAuthorMapOrm.UpdateAuthorsForBookAsync(bookId, authorIds, cancellationToken);

            transaction.Commit();
        }
    }
}