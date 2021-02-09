using JetBrains.Annotations;
using SQLite;

namespace XRayBuilder.Core.Database.Model
{
    [Table(nameof(Book))]
    public sealed class Book
    {
        public Book([JetBrains.Annotations.NotNull] string title)
        {
            Title = title;
        }

        [PrimaryKey]
        [Column(nameof(BookId))]
        public long BookId { get; set; }

        [Column(nameof(Title))]
        [JetBrains.Annotations.NotNull]
        [SQLite.NotNull]
        public string Title { get; }

        [Column(nameof(Author))]
        [CanBeNull]
        public string Author { get; set; }

        [Column(nameof(Asin))]
        [CanBeNull]
        public string Asin { get; set; }

        [Column(nameof(GoodreadsId))]
        [CanBeNull]
        public string GoodreadsId { get; set; }

        [Column(nameof(LibraryThingId))]
        [CanBeNull]
        public string LibraryThingId { get; set; }

        [Column(nameof(ShelfariId))]
        [CanBeNull]
        public string ShelfariId { get; set; }

        [Column(nameof(Isbn))]
        [CanBeNull]
        public string Isbn { get; set; }
    }
}