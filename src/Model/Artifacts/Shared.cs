using System;
using Newtonsoft.Json;

namespace XRayBuilderGUI.Model.Artifacts
{
    public static class Extensions
    {
        public static void Replace(this LocalizedText text, string toReplace, string replacement)
        {
            var props = text.GetType().GetProperties();
            foreach (var prop in props)
            {
                var curVal = prop.GetValue(text).ToString();
                prop.SetValue(text, curVal.Replace(toReplace, replacement));
            }
        }

        public static Book BookInfoToBook(BookInfo bookInfo, bool featured)
        {
            if (bookInfo == null)
                return null;
            return new Book
            {
                Class = featured ? "featuredRecommendation" : "recommendation",
                Asin = bookInfo.asin,
                Title = bookInfo.title,
                Authors = new[] { bookInfo.author },
                ImageUrl = bookInfo.bookImageUrl,
                Description = featured ? bookInfo.desc : null,
                AmazonRating = featured ? (double?)bookInfo.amazonRating : null,
                NumberOfReviews = featured ? (int?)bookInfo.numReviews : null
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

    public class LocalizedText
    {
        [JsonProperty("de")]
        public string De { get; set; }

        [JsonProperty("en")]
        public string En { get; set; }

        [JsonProperty("en-US")]
        public string EnUS { get; set; }

        [JsonProperty("es")]
        public string Es { get; set; }

        [JsonProperty("fr")]
        public string Fr { get; set; }

        [JsonProperty("it")]
        public string It { get; set; }

        [JsonProperty("ja")]
        public string Ja { get; set; }

        [JsonProperty("nl")]
        public string Nl { get; set; }

        [JsonProperty("pt-BR")]
        public string PtBR { get; set; }

        [JsonProperty("ru")]
        public string Ru { get; set; }

        [JsonProperty("zh-CN")]
        public string ZhCN { get; set; }
    }
}
