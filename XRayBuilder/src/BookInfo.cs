using System.Collections.Generic;
using System.Drawing;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.Unpack;

namespace XRayBuilderGUI
{
    // TODO: Remove defaults and privates, move logic
    public class BookInfo
    {
        public string Title { get; set; }
        public string Author { get; set; }
        // TODO: Add TLD to go with ASIN (asin/tld class?)
        public string Asin { get; set; }
        public string Databasename { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public double AmazonRating { get; set; }
        public int Reviews { get; set; }
        public string DataUrl { get; set; } = "";
        public string AmazonUrl => string.IsNullOrEmpty(Asin) ? null : $"https://www.amazon.com/dp/{Asin}";
        // TODO: Shouldn't be here?
        public string RawmlPath { get; set; } = "";

        // TODO: Author class
        public string AuthorAsin { get; set; } = "";
        public string AuthorImageUrl { get; set; } = "";

        public string GoodreadsId { get; set; } = "";
        public int Editions { get; set; } = 0;

        // Added StartAction info
        public int ReadingHours { get; set; }
        public int ReadingMinutes { get; set; }
        public int PagesInBook { get; set; }
        public SeriesInfo Series { get; set; } = new SeriesInfo();

        /// <summary>
        /// Used to store the cover image once downloaded (manually)
        /// </summary>
        public Bitmap CoverImage { get; set; }

        // List of clips and their highlight/like count
        public List<NotableClip> notableClips;

        private readonly IMetadata _metadata;
        private string _guid;

        public string Guid
        {
            private set => _guid = Functions.ConvertGuid(value);
            get => _guid;
        }

        public BookInfo(IMetadata metadata, string dataUrl, string fileName)
        {
            _metadata = metadata;
            Title = metadata.Title;
            Author = metadata.Author;
            Asin = metadata.Asin;
            if (metadata.UniqueId != null)
                Guid = metadata.UniqueId;
            Databasename = metadata.DbName;
            FileName = fileName;
            DataUrl = dataUrl;
        }

        public BookInfo(string title, string author, string asin, string guid, string databasename, string fileName, string dataUrl, string rawmlPath)
        {
            Title = title;
            Author = author;
            Asin = asin;
            Guid = guid;
            Databasename = databasename;
            FileName = fileName;
            DataUrl = dataUrl;
            RawmlPath = rawmlPath;
        }

        public BookInfo(string title, string author, string asin)
        {
            Title = title;
            Author = author;
            Asin = asin;
        }

        public override string ToString()
        {
            return Title + " - " + Author;
        }

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon URL.
        /// </summary>
        /// <param name="amazonUrl">Book's Amazon URL</param>

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon page.
        /// </summary>
        /// <param name="bookDoc">Book's Amazon page, pre-downloaded</param>
    }
}
