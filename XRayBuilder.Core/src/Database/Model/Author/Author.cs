namespace XRayBuilder.Core.Database.Model
{
    /// <summary>
    /// POCO version of <see cref="AuthorModel"/>
    /// </summary>
    public class Author
    {
        public string Asin { get; set; }

        public string Name { get; set; }

        public string Biography { get; set; }

        public string ImageUrl { get; set; }
    }
}