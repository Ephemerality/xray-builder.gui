namespace XRayBuilder.Core.Database.Model.Author
{
    /// <summary>
    /// Database model for the Author table
    /// Not bothering with a POCO version for now
    /// </summary>
    public sealed class AuthorModel
    {
        public long AuthorId { get; set; }

        public string Asin { get; set; }

        public string Name { get; set; }

        public string Biography { get; set; }

        public string ImageUrl { get; set; }
    }
}