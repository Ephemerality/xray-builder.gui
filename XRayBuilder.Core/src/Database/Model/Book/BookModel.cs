namespace XRayBuilder.Core.Database.Model.Book
{
    /// <summary>
    /// Database model for the Book table
    /// </summary>
    public sealed class BookModel
    {
        public long BookId { get; set; }

        public string Title { get; set; }

        public string Asin { get; set; }

        public string GoodreadsId { get; set; }

        public string LibraryThingId { get; set; }

        public string ShelfariId { get; set; }

        public string Isbn { get; set; }
    }
}