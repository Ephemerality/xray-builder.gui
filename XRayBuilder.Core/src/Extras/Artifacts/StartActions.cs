using System.Collections.Generic;
using System.Drawing.Printing;
using Newtonsoft.Json;

namespace XRayBuilder.Core.Extras.Artifacts
{
    public sealed class StartActions
    {
        [JsonProperty("bookInfo")]
        public BookInformation BookInfo { get; set; }

        [JsonProperty("widgets")]
        public Widget[] Widgets { get; set; }

        [JsonProperty("layouts")]
        public Layout[] Layouts { get; set; }

        [JsonProperty("data")]
        public DataJson Data { get; set; }

        public sealed class BookInformation
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

            [JsonProperty("bisacCodes")]
            public string[] BisacCodes { get; set; }

            [JsonProperty("fictionStatus")]
            public string FictionStatus { get; set; }

            [JsonProperty("erl")]
            public int Erl { get; set; }
        }

        public sealed class WidgetOptions
        {
            [JsonProperty("dataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string DataKey { get; set; }

            [JsonProperty("displayLimitKey", NullValueHandling = NullValueHandling.Ignore)]
            public string DisplayLimitKey { get; set; }

            [JsonProperty("displayLimit", NullValueHandling = NullValueHandling.Ignore)]
            public int? DisplayLimit { get; set; }

            [JsonProperty("timeDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string TimeDataKey { get; set; }

            [JsonProperty("pageDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string PageDataKey { get; set; }

            [JsonProperty("preferredTypeOrder", NullValueHandling = NullValueHandling.Ignore)]
            public string[] PreferredTypeOrder { get; set; }

            [JsonProperty("imagesThreshold", NullValueHandling = NullValueHandling.Ignore)]
            public int? ImagesThreshold { get; set; }

            [JsonProperty("imagesFormat", NullValueHandling = NullValueHandling.Ignore)]
            public string ImagesFormat { get; set; }

            [JsonProperty("dynamicButtonDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string DynamicButtonDataKey { get; set; }

            [JsonProperty("dynamicButtonActionKey", NullValueHandling = NullValueHandling.Ignore)]
            public string DynamicButtonActionKey { get; set; }

            [JsonProperty("displayIfClicked", NullValueHandling = NullValueHandling.Ignore)]
            public bool? DisplayIfClicked { get; set; }

            [JsonProperty("clickOnlyOnce", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ClickOnlyOnce { get; set; }

            [JsonProperty("refTagPartial", NullValueHandling = NullValueHandling.Ignore)]
            public string RefTagPartial { get; set; }

            [JsonProperty("featureKey", NullValueHandling = NullValueHandling.Ignore)]
            public string FeatureKey { get; set; }

            [JsonProperty("seriesPositionDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string SeriesPositionDataKey { get; set; }

            [JsonProperty("subscriptionInfoDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string SubscriptionInfoDataKey { get; set; }

            [JsonProperty("followInfoDataKey", NullValueHandling = NullValueHandling.Ignore)]
            public string FollowInfoDataKey { get; set; }

            [JsonProperty("initialLines", NullValueHandling = NullValueHandling.Ignore)]
            public int? InitialLines { get; set; }

            [JsonProperty("moreLines", NullValueHandling = NullValueHandling.Ignore)]
            public int? MoreLines { get; set; }

            [JsonProperty("buyInStore", NullValueHandling = NullValueHandling.Ignore)]
            public bool? BuyInStore { get; set; }

            [JsonProperty("buyButtonVisible", NullValueHandling = NullValueHandling.Ignore)]
            public bool? BuyButtonVisible { get; set; }

            [JsonProperty("oneClickBorrowSupported", NullValueHandling = NullValueHandling.Ignore)]
            public bool? OneClickBorrowSupported { get; set; }

            [JsonProperty("showShareButton", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowShareButton { get; set; }

            [JsonProperty("showWishListButton", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowWishListButton { get; set; }

            [JsonProperty("showBadges", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowBadges { get; set; }
        }

        public sealed class WidgetStrings
        {
            [JsonProperty("imagesDescription", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> ImagesDescription { get; set; }

            [JsonProperty("imagesButtonText", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> ImagesButtonText { get; set; }

            [JsonProperty("entitiesButtonText", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> EntitiesButtonText { get; set; }

            [JsonProperty("entitiesDescription", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> EntitiesDescription { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> Text { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> Title { get; set; }

            [JsonProperty("panelRowTitle", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, string> PanelRowTitle { get; set; }
        }

        public sealed class Widget
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("metricsTag")]
            public string MetricsTag { get; set; }

            [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
            public WidgetOptions Options { get; set; }

            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("strings", NullValueHandling = NullValueHandling.Ignore)]
            public WidgetStrings Strings { get; set; }
        }

        public sealed class FooterJson
        {
            [JsonProperty("widgetSlots")]
            public string[][] WidgetSlots { get; set; }
        }

        public sealed class BodyJson
        {
            [JsonProperty("titleKey", NullValueHandling = NullValueHandling.Ignore)]
            public string TitleKey { get; set; }

            [JsonProperty("widgetSlots")]
            public string[][] WidgetSlots { get; set; }
        }

        public sealed class WidgetPlacements
        {
            [JsonProperty("footer")]
            public FooterJson[] Footer { get; set; }

            [JsonProperty("body")]
            public BodyJson[] Body { get; set; }
        }

        public sealed class LayoutOptions
        {
            [JsonProperty("providesHeaderInfo")]
            public bool? ProvidesHeaderInfo { get; set; }
        }

        public sealed class Layout
        {
            [JsonProperty("metricsTag")]
            public string MetricsTag { get; set; }

            [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
            public LayoutOptions Options { get; set; }

            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("strings")]
            public Dictionary<string, Dictionary<string, string>> Strings { get; set; }

            [JsonProperty("widgetPlacements")]
            public WidgetPlacements WidgetPlacements { get; set; }

            [JsonProperty("requiredWidgets", NullValueHandling = NullValueHandling.Ignore)]
            public string[] RequiredWidgets { get; set; }
        }

        public sealed class SeriesPosition
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "seriesPosition";

            [JsonProperty("positionInSeries")]
            public int PositionInSeries { get; set; }

            [JsonProperty("totalInSeries")]
            public int TotalInSeries { get; set; }

            [JsonProperty("seriesName")]
            public string SeriesName { get; set; }
        }

        public sealed class AuthorSubscriptions
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "authorSubscriptionInfoList";

            [JsonProperty("subscriptions")]
            public Subscription[] Subscriptions { get; set; }
        }

        public sealed class WelcomeText
        {
            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("localizedText")]
            public Dictionary<string, string> LocalizedText { get; set; }

            [JsonProperty("localizedSubtext")]
            public Dictionary<string, string> LocalizedSubtext { get; set; }
        }

        public sealed class PopularHighlightsText
        {
            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("localizedText")]
            public Dictionary<string, string> LocalizedText { get; set; }
        }

        public class GrokShelfInfo
        {
            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("asin")]
            public string Asin { get; set; }

            [JsonProperty("shelves")]
            public string[] Shelves { get; set; }

            [JsonProperty("is_sensitive")]
            public bool IsSensitive { get; set; }

            [JsonProperty("is_autoshelving_enabled")]
            public bool IsAutoshelvingEnabled { get; set; }
        }

        public sealed class ReadingTime
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "time";

            [JsonProperty("hours")]
            public int Hours { get; set; }

            [JsonProperty("minutes")]
            public int Minutes { get; set; }

            [JsonProperty("formattedTime")]
            public Dictionary<string, string> FormattedTime { get; set; }
        }

        public sealed class ReadingPages
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "pages";

            [JsonProperty("pagesInBook")]
            public int PagesInBook { get; set; }
        }

        public sealed class DataJson
        {
            [JsonProperty("seriesPosition")]
            public SeriesPosition SeriesPosition { get; set; }

            [JsonProperty("followSubscriptions")]
            public AuthorSubscriptions FollowSubscriptions { get; set; }

            [JsonProperty("welcomeText")]
            public WelcomeText WelcomeText { get; set; }

            [JsonProperty("popularHighlightsText")]
            public PopularHighlightsText PopularHighlightsText { get; set; }

            [JsonProperty("grokShelfInfo")]
            public GrokShelfInfo GrokShelfInfo { get; set; }

            [JsonProperty("bookDescription")]
            public Book BookDescription { get; set; }

            [JsonProperty("authorBios")]
            public AuthorBios AuthorBios { get; set; }

            [JsonProperty("authorRecs")]
            public Recs AuthorRecs { get; set; }

            [JsonProperty("authorFeaturedRecs", NullValueHandling = NullValueHandling.Ignore)]
            public Recs AuthorFeaturedRecs { get; set; }

            [JsonProperty("citationFeaturedRecs", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Recs CitationFeaturedRecs { get; set; }

            [JsonProperty("leftNavCitationRecs", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Recs LeftNavCitationRecs { get; set; }

            [JsonProperty("currentBook")]
            public Book CurrentBook { get; set; }

            [JsonProperty("readingTime")]
            public ReadingTime ReadingTime { get; set; }

            [JsonProperty("previousBookInTheSeries")]
            public Book PreviousBookInTheSeries { get; set; }

            [JsonProperty("authorSubscriptions")]
            public AuthorSubscriptions AuthorSubscriptions { get; set; }

            [JsonProperty("readingPages")]
            public ReadingPages ReadingPages { get; set; }

            [JsonProperty("kuUrlAction", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public UrlAction KindleUnlimitedUrlAction { get; set; }

            [JsonProperty("kuTeaserData", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DynamicButton KindleUnlimitedTeaserData { get; set; }
        }

        public sealed class UrlAction
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "urlAction";

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public sealed class DynamicButton
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "dynamicButton";

            [JsonProperty("localizedText")]
            public Dictionary<string, string> LocalizedText { get; set; }

            [JsonProperty("localizedButtonText")]
            public Dictionary<string, string> LocalizedButtonText { get; set; }
        }
    }
}
