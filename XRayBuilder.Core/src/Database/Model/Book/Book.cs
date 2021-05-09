using System.Collections.Generic;

namespace XRayBuilder.Core.Database.Model
{
    /// <summary>
    /// POCO version of <see cref="BookModel"/>
    /// </summary>
    public sealed class Book
    {
        public string Asin { get; set; }

        public string Title { get; set; }

        public List<AuthorModel> Authors { get; set; }

        public string GoodreadsId { get; set; }

        public string LibraryThingId { get; set; }

        public string ShelfariId { get; set; }

        public string Isbn { get; set; }
    }
}