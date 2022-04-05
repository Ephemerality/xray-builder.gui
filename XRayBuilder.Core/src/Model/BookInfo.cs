using System;
using System.Collections.Generic;
using System.Drawing;
using Ephemerality.Unpack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries;
using Image = SixLabors.ImageSharp.Image;

namespace XRayBuilder.Core.Model
{
    // TODO: Remove defaults and privates, move logic
    public class BookInfo : IEquatable<BookInfo>
    {
        public string Title { get; set; }
        public string Author { get; }
        public string Asin { get; }
        [CanBeNull]
        public string Tld { get; set; }
        public string AmazonUrl => string.IsNullOrEmpty(Asin)
            ? null
            : $"https://www.amazon.{(string.IsNullOrEmpty(Tld) ? "com" : Tld)}/dp/{Asin}";
        public string Databasename { get; }
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public double? AmazonRating { get; set; }
        public int Reviews { get; set; }
        public string DataUrl { get; set; } = "";

        // TODO: Author class
        public string AuthorAsin { get; } = "";
        public string AuthorImageUrl { get; set; } = "";

        public string GoodreadsId { get; set; } = "";
        public int Editions { get; set; }

        // Added StartAction info
        public int ReadingHours { get; set; }
        public int ReadingMinutes { get; set; }
        public int PageCount { get; set; }
        public SeriesInfo Series { get; set; } = new SeriesInfo();

        public int ImageCount { get; set; }

        /// <summary>
        /// Used to store the cover image once downloaded (manually)
        /// </summary>
        [CanBeNull]
        public Image CoverImage { get; set; }

        // List of clips and their highlight/like count
        public List<NotableClip> NotableClips;

        [CanBeNull]
        public string Guid { get; set; }

        public BookInfo(IMetadata metadata, string dataUrl)
        {
            Title = metadata.Title;
            Author = metadata.Author;
            Asin = metadata.Asin;
            if (metadata.UniqueId != null)
                Guid = Functions.ConvertGuid(metadata.UniqueId);
            Databasename = metadata.DbName;
            DataUrl = dataUrl;
            ImageCount = metadata.ImageCount;
        }

        public BookInfo(string title, string author, string asin)
        {
            Title = title;
            Author = author;
            Asin = asin;
        }

        public override string ToString() => $"{Title} - {Author}";

        public bool Equals(BookInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Title == other.Title && Author == other.Author && Asin == other.Asin;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BookInfo) obj);
        }

        public override int GetHashCode()
        {
#if NETFRAMEWORK
            unchecked
            {
                var hashCode = Title != null ? Title.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Author != null ? Author.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Asin != null ? Asin.GetHashCode() : 0);
                return hashCode;
            }
#else
            return HashCode.Combine(Asin, Author, Title);
#endif
        }
    }
}
