using System;
using Newtonsoft.Json;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Extras.Artifacts
{
    public static class Extensions
    {
        public static Book BookInfoToBook(BookInfo bookInfo, bool featured)
        {
            if (bookInfo == null)
                return null;
            return new Book
            {
                Class = featured ? "featuredRecommendation" : "recommendation",
                Asin = bookInfo.Asin,
                Title = bookInfo.Title,
                Authors = new[] { bookInfo.Author },
                ImageUrl = bookInfo.ImageUrl,
                Description = featured ? bookInfo.Description : null,
                AmazonRating = featured ? bookInfo.AmazonRating : null,
                NumberOfReviews = featured ? (int?)bookInfo.Reviews : null
            };
        }
    }

    public class AuthorBios
    {
        [JsonProperty("class")]
        public string Class { get; set; } = "authorBioList";

        [JsonProperty("authors")]
        public Author[] Authors { get; set; }
    }

    public class Recs
    {
        [JsonProperty("class")]
        public string Class { get; set; } = "recommendationList";

        [JsonProperty("recommendations")]
        public Book[] Recommendations { get; set; }
    }

    public class Author
    {
        [JsonProperty("class")]
        public string Class { get; set; } = "authorBio";

        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class Book
    {
        private double? _amazonRating;

        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("authors")]
        public string[] Authors { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("hasSample")]
        public bool HasSample { get; set; }

        [JsonProperty("amazonRating", NullValueHandling = NullValueHandling.Ignore)]
        public double? AmazonRating
        {
            get => _amazonRating.HasValue ? (double?) Math.Round(_amazonRating.Value, 1) : null;
            set => _amazonRating = value;
        }

        [JsonProperty("numberOfReviews", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberOfReviews { get; set; }
    }

    public class Subscription
    {
        [JsonProperty("class")]
        public string Class { get; set; } = "authorSubscriptionInfo";

        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subscribed")]
        public bool Subscribed { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }
}
