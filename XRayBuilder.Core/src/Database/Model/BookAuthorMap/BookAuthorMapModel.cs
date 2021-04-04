namespace XRayBuilder.Core.Database.Model.BookAuthorMap
{
    public sealed class BookAuthorMapModel
    {
        public long BookAuthorMapId { get; set; }
        public long BookId { get; set; }
        public long AuthorId { get; set; }
    }
}