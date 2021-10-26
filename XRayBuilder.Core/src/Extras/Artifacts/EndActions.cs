using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace XRayBuilder.Core.Extras.Artifacts
{
    public class EndActions
    {
        [JsonProperty("bookInfo")]
        public BookInformation BookInfo { get; set; }

        [JsonProperty("widgets")]
        public Widget[] Widgets { get; set; }

        [JsonProperty("layouts")]
        public Layout[] Layouts { get; set; }

        [JsonProperty("data")]
        public DataJson Data { get; set; }

        [JsonProperty("bottomSheetEnabled")]
        public bool? BottomSheetEnabled { get; set; }

        public class BookInformation
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "bookInfo";

            [JsonProperty("asin")]
            public string Asin { get; set; }

            [JsonProperty("contentType")]
            public string ContentType { get; set; }

            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("refTagSuffix")]
            public string RefTagSuffix { get; set; }

            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }

            [JsonProperty("embeddedID")]
            public string EmbeddedID { get; set; }

            [JsonProperty("fictionStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string FictionStatus { get; set; }

            [JsonProperty("erl")]
            public long Erl { get; set; }
        }

        public class Options
        {
            [JsonProperty("refTagPartial", NullValueHandling = NullValueHandling.Ignore)]
            public string RefTagPartial { get; set; }

            [JsonProperty("showShareComponent", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowShareComponent { get; set; }

            [JsonProperty("dataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string DataKey { get; set; }

            [JsonProperty("subscriptionInfoDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string SubscriptionInfoDataKey { get; set; }

            [JsonProperty("followInfoDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string FollowInfoDataKey { get; set; }

            [JsonProperty("buyInStore", NullValueHandling = NullValueHandling.Ignore)]
            public bool? BuyInStore { get; set; }

            [JsonProperty("buyButtonVisible", NullValueHandling = NullValueHandling.Ignore)]
            public bool? BuyButtonVisible { get; set; }

            [JsonProperty("dataIsCurrentBook", NullValueHandling = NullValueHandling.Ignore)]
            public bool? DataIsCurrentBook { get; set; }

            [JsonProperty("oneClickBorrowSupported", NullValueHandling = NullValueHandling.Ignore)]
            public bool? OneClickBorrowSupported { get; set; }

            [JsonProperty("showShareButton", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowShareButton { get; set; }

            [JsonProperty("showWishListButton", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowWishListButton { get; set; }

            [JsonProperty("showBadges", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowBadges { get; set; }
        }

        public class Strings
        {
            [JsonProperty("title")]
            public Dictionary<string, string> Title { get; set; }

            [JsonProperty("buttonText", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> ButtonText { get; set; }
        }

        public class Widget
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("metricsTag")]
            public string MetricsTag { get; set; }

            [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
            public Options Options { get; set; }

            [JsonProperty("strings", NullValueHandling = NullValueHandling.Ignore)]
            public Strings Strings { get; set; }
        }

        public class WidgetPlacements
        {
            [JsonProperty("body")]
            public string[][] Body { get; set; }
        }

        public class Layout
        {
            [JsonProperty("metricsTag")]
            public string MetricsTag { get; set; }

            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("widgetPlacements", NullValueHandling = NullValueHandling.Ignore)]
            public WidgetPlacements WidgetPlacements { get; set; }

            [JsonProperty("requiredWidgets", NullValueHandling = NullValueHandling.Ignore)]
            public string[] RequiredWidgets { get; set; }
        }

        public class CustomerProfile
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "customerProfile";

            [JsonProperty("penName", NullValueHandling = NullValueHandling.Ignore)]
            public string PenName { get; set; }

            [JsonProperty("realName", NullValueHandling = NullValueHandling.Ignore)]
            public string RealName { get; set; }
        }

        public class Rating
        {
            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }
        }

        public class AuthorSubscriptions
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "authorSubscriptionInfoList";

            [JsonProperty("subscriptions")]
            public Subscription[] Subscriptions { get; set; }
        }

        public class GrokShelfInfo
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "goodReadsShelfInfo";

            [JsonProperty("asin")]
            public string Asin { get; set; }

            [JsonProperty("shelves")]
            public string[] Shelves { get; set; }

            [JsonProperty("is_sensitive")]
            public bool IsSensitive { get; set; }

            [JsonProperty("is_autoshelving_enabled")]
            public bool IsAutoshelvingEnabled { get; set; }
        }

        public class DataJson
        {
            [JsonProperty("followSubscriptions")]
            public AuthorSubscriptions FollowSubscriptions { get; set; }

            [JsonProperty("nextBook", NullValueHandling = NullValueHandling.Ignore)]
            public Book NextBook { get; set; }

            [JsonProperty("publicSharedRating")]
            public Rating PublicSharedRating { get; set; }

            [JsonProperty("customerProfile")]
            public CustomerProfile CustomerProfile { get; set; }

            [JsonProperty("authorBiosBSE")]
            public AuthorBios AuthorBiosBSE { get; set; }

            [JsonProperty("rating")]
            public Rating Rating { get; set; }

            [JsonProperty("authorBios")]
            public AuthorBios AuthorBios { get; set; }

            [JsonProperty("authorRecs")]
            [CanBeNull]
            public Recs AuthorRecs { get; set; }

            [JsonProperty("customersWhoBoughtRecs")]
            [CanBeNull]
            public Recs CustomersWhoBoughtRecs { get; set; }

            [JsonProperty("authorSubscriptions")]
            public AuthorSubscriptions AuthorSubscriptions { get; set; }

            [JsonProperty("citationRecs", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Recs CitationRecs { get; set; }

            [JsonProperty("grokShelfInfo", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public GrokShelfInfo GrokShelfInfo { get; set; }

            [JsonProperty("microgenreRecs", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Recs MicrogenreRecs { get; set; }
        }
    }
}
