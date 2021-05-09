namespace XRayBuilder.Core.Database.Model
{
    /// <summary>
    /// Database model for the Author table
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