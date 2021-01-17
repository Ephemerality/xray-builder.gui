using System.Collections.Generic;

namespace XRayBuilder.Core.Libraries.Database.Model
{
    public class Book
    {
        public List<Author> Authors = new List<Author>();
        public string Asin { get; set; }
        public string Url { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public string RecordPath { get; set; }
        public string RawMlPath { get; set; }
    }
}