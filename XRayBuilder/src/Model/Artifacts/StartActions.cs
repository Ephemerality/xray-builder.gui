using Newtonsoft.Json;

namespace XRayBuilderGUI.Model.Artifacts
{
    public class StartActions
    {
        [JsonProperty("bookInfo")]
        public BookInformation BookInfo { get; set; }

        [JsonProperty("widgets")]
        public Widget[] Widgets { get; set; }

        [JsonProperty("layouts")]
        public Layout[] Layouts { get; set; }

        [JsonProperty("data")]
        public DataJson Data { get; set; }

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

            [JsonProperty("erl")]
            public int Erl { get; set; }
        }

        public class WidgetOptions
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
        }

        public class WidgetStrings
        {
            [JsonProperty("imagesDescription", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText ImagesDescription { get; set; }

            [JsonProperty("imagesButtonText", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText ImagesButtonText { get; set; }

            [JsonProperty("entitiesButtonText", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText EntitiesButtonText { get; set; }

            [JsonProperty("entitiesDescription", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText EntitiesDescription { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText Text { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText Title { get; set; }

            [JsonProperty("panelRowTitle", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText PanelRowTitle { get; set; }
        }

        public class Widget
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

        public class FooterJson
        {
            [JsonProperty("widgetSlots")]
            public string[][] WidgetSlots { get; set; }
        }

        public class BodyJson
        {
            [JsonProperty("titleKey", NullValueHandling = NullValueHandling.Ignore)]
            public string TitleKey { get; set; }

            [JsonProperty("widgetSlots")]
            public string[][] WidgetSlots { get; set; }
        }

        public class WidgetPlacements
        {
            [JsonProperty("footer")]
            public FooterJson[] Footer { get; set; }

            [JsonProperty("body")]
            public BodyJson[] Body { get; set; }
        }

        public class LayoutOptions
        {
            [JsonProperty("providesHeaderInfo")]
            public bool? ProvidesHeaderInfo { get; set; }
        }

        public class LayoutStrings
        {
            [JsonProperty("seriesGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText SeriesGroup { get; set; }

            [JsonProperty("xrayGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText XrayGroup { get; set; }

            [JsonProperty("bookDescriptionEInkGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText BookDescriptionEInkGroup { get; set; }

            [JsonProperty("authorsGroupWithSingleAuthor", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText AuthorsGroupWithSingleAuthor { get; set; }

            [JsonProperty("readingTimeGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText ReadingTimeGroup { get; set; }

            [JsonProperty("popularHighlightsGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText PopularHighlightsGroup { get; set; }

            [JsonProperty("citationsGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText CitationsGroup { get; set; }

            [JsonProperty("welcomeGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText WelcomeGroup { get; set; }

            [JsonProperty("audibleGroup", NullValueHandling = NullValueHandling.Ignore)]
            public LocalizedText AudibleGroup { get; set; }
        }

        public class Layout
        {
            [JsonProperty("metricsTag")]
            public string MetricsTag { get; set; }

            [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
            public LayoutOptions Options { get; set; }

            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("strings")]
            public LayoutStrings Strings { get; set; }

            [JsonProperty("widgetPlacements")]
            public WidgetPlacements WidgetPlacements { get; set; }

            [JsonProperty("requiredWidgets", NullValueHandling = NullValueHandling.Ignore)]
            public string[] RequiredWidgets { get; set; }
        }

        public class SeriesPosition
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

        public class AuthorSubscriptions
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "authorSubscriptionInfoList";

            [JsonProperty("subscriptions")]
            public Subscription[] Subscriptions { get; set; }
        }

        public class WelcomeText
        {
            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("localizedText")]
            public LocalizedText LocalizedText { get; set; }

            [JsonProperty("localizedSubtext")]
            public LocalizedText LocalizedSubtext { get; set; }
        }

        public class PopularHighlightsText
        {
            [JsonProperty("class")]
            public string Class { get; set; }

            [JsonProperty("localizedText")]
            public LocalizedText LocalizedText { get; set; }
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

        public class ReadingTime
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "time";

            [JsonProperty("hours")]
            public int Hours { get; set; }

            [JsonProperty("minutes")]
            public int Minutes { get; set; }

            [JsonProperty("formattedTime")]
            public LocalizedText FormattedTime { get; set; }
        }

        public class ReadingPages
        {
            [JsonProperty("class")]
            public string Class { get; set; } = "pages";

            [JsonProperty("pagesInBook")]
            public int PagesInBook { get; set; }
        }

        public class DataJson
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
        }
    }
}
