using Newtonsoft.Json;

namespace XRayBuilder.Core.DataSources.Amazon.Model
{
    public sealed class Binding
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    public sealed class BindingInformation
    {
        [JsonProperty("binding")]
        public Binding Binding { get; set; }
    }

    public sealed class BookSeriesInfo
    {
        [JsonProperty("position")]
        public int? Position { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("seriesTitle")]
        public string SeriesTitle { get; set; }

        [JsonProperty("seriesPageLink")]
        public string SeriesPageLink { get; set; }

        [JsonProperty("longMessage")]
        public string LongMessage { get; set; }

        [JsonProperty("shortMessage")]
        public string ShortMessage { get; set; }
    }

    public sealed class Role
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public sealed class ContributorFields
    {
        [JsonProperty("FIELDS")]
        public string[] Fields { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }
    }

    public sealed class Link
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("rel")]
        public string Rel { get; set; }
    }

    public sealed class Contributor
    {
        [JsonProperty("roles")]
        public Role[] Roles { get; set; }

        [JsonProperty("contributor")]
        public ContributorFields ContributorFields { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }

        [JsonProperty("bylineInfoType")]
        public string BylineInfoType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public sealed class ByLine
    {
        [JsonProperty("contributors")]
        public Contributor[] Contributors { get; set; }
    }

    public sealed class Rating
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("shortDisplayString")]
        public string ShortDisplayString { get; set; }

        [JsonProperty("fullStarCount")]
        public int FullStarCount { get; set; }

        [JsonProperty("hasHalfStar")]
        public bool HasHalfStar { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public sealed class ThreeStar
    {
        [JsonProperty("percentage")]
        public int Percentage { get; set; }

        [JsonProperty("labelDisplayString")]
        public string LabelDisplayString { get; set; }

        [JsonProperty("percentageDisplayString")]
        public string PercentageDisplayString { get; set; }
    }

    public sealed class FiveStar
    {
        [JsonProperty("percentage")]
        public int Percentage { get; set; }

        [JsonProperty("labelDisplayString")]
        public string LabelDisplayString { get; set; }

        [JsonProperty("percentageDisplayString")]
        public string PercentageDisplayString { get; set; }
    }

    public sealed class TwoStar
    {
        [JsonProperty("percentage")]
        public int Percentage { get; set; }

        [JsonProperty("labelDisplayString")]
        public string LabelDisplayString { get; set; }

        [JsonProperty("percentageDisplayString")]
        public string PercentageDisplayString { get; set; }
    }

    public sealed class OneStar
    {
        [JsonProperty("percentage")]
        public int Percentage { get; set; }

        [JsonProperty("labelDisplayString")]
        public string LabelDisplayString { get; set; }

        [JsonProperty("percentageDisplayString")]
        public string PercentageDisplayString { get; set; }
    }

    public sealed class FourStar
    {
        [JsonProperty("percentage")]
        public int Percentage { get; set; }

        [JsonProperty("labelDisplayString")]
        public string LabelDisplayString { get; set; }

        [JsonProperty("percentageDisplayString")]
        public string PercentageDisplayString { get; set; }
    }

    public sealed class Histogram
    {
        [JsonProperty("threeStar")]
        public ThreeStar ThreeStar { get; set; }

        [JsonProperty("fiveStar")]
        public FiveStar FiveStar { get; set; }

        [JsonProperty("twoStar")]
        public TwoStar TwoStar { get; set; }

        [JsonProperty("oneStar")]
        public OneStar OneStar { get; set; }

        [JsonProperty("fourStar")]
        public FourStar FourStar { get; set; }
    }

    public sealed class Count
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public sealed class CustomerReviewsSummary
    {
        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("histogram")]
        public Histogram Histogram { get; set; }

        [JsonProperty("count")]
        public Count Count { get; set; }
    }

    public sealed class ViewOnAmazon
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public sealed class Links
    {
        [JsonProperty("viewOnAmazon")]
        public ViewOnAmazon ViewOnAmazon { get; set; }
    }

    public sealed class Metabinding
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    public sealed class WebsiteDisplayGroup
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    public sealed class ProductCategory
    {

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("websiteDisplayGroup")]
        public WebsiteDisplayGroup WebsiteDisplayGroup { get; set; }
    }

    public sealed class LowRes
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public sealed class HiRes
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public sealed class Image
    {
        [JsonProperty("lowRes")]
        public LowRes LowRes { get; set; }

        [JsonProperty("hiRes")]
        public HiRes HiRes { get; set; }

        [JsonProperty("variant")]
        public string Variant { get; set; }
    }

    public sealed class ProductImages
    {
        [JsonProperty("images")]
        public Image[] Images { get; set; }

        [JsonProperty("altText")]
        public string AltText { get; set; }
    }

    public sealed class Title
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("shortDisplayString")]
        public string ShortDisplayString { get; set; }
    }

    public sealed class Parameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public sealed class Data
    {
        [JsonProperty("displayString")]
        public string DisplayString { get; set; }

        [JsonProperty("parameters")]
        public Parameter[] Parameters { get; set; }
    }

    public sealed class Product
    {
        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("bindingInformation")]
        public BindingInformation BindingInformation { get; set; }

        [JsonProperty("bookSeriesInfo")]
        public BookSeriesInfo BookSeriesInfo { get; set; }

        [JsonProperty("byLine")]
        public ByLine ByLine { get; set; }

        [JsonProperty("customerReviewsSummary")]
        public CustomerReviewsSummary CustomerReviewsSummary { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("productCategory")]
        public ProductCategory ProductCategory { get; set; }

        [JsonProperty("productImages")]
        public ProductImages ProductImages { get; set; }

        [JsonProperty("title")]
        public Title Title { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public sealed class Content
    {
        [JsonProperty("ASINList")]
        public string[] AsinList { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("totalResultCount")]
        public int? TotalResultCount { get; set; }

        [JsonProperty("searchAllOfAmazonUrl")]
        public string SearchAllOfAmazonUrl { get; set; }

        [JsonProperty("products")]
        public Product[] Products { get; set; }

        [JsonProperty("requestedAsins")]
        public string[] RequestedAsins { get; set; }

        [JsonProperty("ProductStrategy")]
        public string ProductStrategy { get; set; }
    }

    public sealed class BrandLogo
    {
        [JsonProperty("imageWidth")]
        public int ImageWidth { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("shape")]
        public string Shape { get; set; }

        [JsonProperty("imageOffsetTop")]
        public int ImageOffsetTop { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("hideBrandLogo")]
        public bool HideBrandLogo { get; set; }

        [JsonProperty("imageHeight")]
        public int ImageHeight { get; set; }
    }

    public sealed class PageContext
    {
        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("storeType")]
        public string StoreType { get; set; }

        [JsonProperty("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("rootPagePath")]
        public string RootPagePath { get; set; }

        [JsonProperty("authorId")]
        public string AuthorId { get; set; }

        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("rootPageId")]
        public string RootPageId { get; set; }

        [JsonProperty("pagePath")]
        public string PagePath { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("afid")]
        public string Afid { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("brandLogo")]
        public BrandLogo BrandLogo { get; set; }
    }

    public sealed class AmazonAuthorPayload
    {
        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("pageContext")]
        public PageContext PageContext { get; set; }
    }
}