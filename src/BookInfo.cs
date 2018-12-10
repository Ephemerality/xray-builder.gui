using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
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
        public string Path { get; set; }
        public string SidecarName { get; set; }
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

        // List of clips and their highlight/like count
        public List<NotableClip> notableClips;

        private readonly IMetadata _metadata;
        private string _guid;
        private Bitmap _bookImage;

        public string Guid
        {
            private set => _guid = Functions.ConvertGuid(value);
            get => _guid;
        }

        public BookInfo(IMetadata metadata, string dataUrl)
        {
            _metadata = metadata;
            Title = metadata.Title;
            Author = metadata.Author;
            Asin = metadata.Asin;
            Guid = metadata.UniqueId;
            Databasename = metadata.DbName;
            SidecarName = Functions.RemoveInvalidFileChars(metadata.Title);
            DataUrl = dataUrl;
        }

        public BookInfo(string title, string author, string asin, string guid, string databasename, string path, string sidecarName, string dataUrl, string rawmlPath)
        {
            Title = title;
            Author = author;
            Asin = asin;
            Guid = guid;
            Databasename = databasename;
            Path = path;
            this.SidecarName = sidecarName;
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
        public async Task GetAmazonInfo(string amazonUrl, CancellationToken cancellationToken = default)
        {
            if (amazonUrl == "") return;
            GetAmazonInfo(await HttpClient.GetPageAsync(amazonUrl, cancellationToken));
        }

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon page.
        /// </summary>
        /// <param name="bookDoc">Book's Amazon page, pre-downloaded</param>
        public void GetAmazonInfo(HtmlDocument bookDoc)
        {
            if (ImageUrl == "")
            {
                // Parse Book image URL
                HtmlNode bookImageLoc = bookDoc.DocumentNode.SelectSingleNode("//*[@id='imgBlkFront']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='imageBlock']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='series-detail-product-image']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='ebooksImgBlkFront']") //co.uk seems to use this id sometimes
                    // for more generic matching, such as on audiobooks (which apparently have BXXXXXXXX asins also)
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='main-image']");
                if (bookImageLoc == null)
                    throw new HtmlWebException(string.Format(@"Error finding book image. If you want, you can report the book's Amazon URL to help with parsing.\r\n{0}", AmazonUrl));
                else
                    ImageUrl = Regex.Replace(bookImageLoc.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);
                if (ImageUrl.Contains("base64"))
                {
                    ImageUrl = bookImageLoc.GetAttributeValue("data-a-dynamic-image", "");
                    Match match = Regex.Match(ImageUrl, @"(https://.*?_\.(jpg|jpeg|gif|png))");
                    if (match.Success)
                    {
                        ImageUrl = match.Groups[1].Value;
                        if (!ImageUrl.EndsWith(".png"))
                            ImageUrl = Regex.Replace(ImageUrl, @"_.*?_\.", string.Empty);
                    }
                }

                // cleanup to match retail file image links
                if (ImageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                    ImageUrl = ImageUrl.Replace(@"https://images-na.ssl-images-amazon", @"http://ecx.images-amazon");

                // Use no image URL
                if (ImageUrl == "")
                    ImageUrl = "http://ecx.images-amazon.com/images/G/01/x-site/icons/no-img-sm.gif";
            }
            if (Description == "")
            {
                HtmlNode descNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='bookDescription_feature_div']/noscript")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-size-medium series-detail-description-text']");
                if (descNode != null && descNode.InnerText != "")
                {
                    Description = descNode.InnerText.Trim();
                    // Following the example of Amazon, cut off desc around 1000 characters.
                    // If conveniently trimmed at the end of the sentence, let it end with the punctuation.
                    // If the sentence continues, cut it off and replace the space with an ellipsis
                    if (Description.Length > 1000)
                    {
                        Description = Description.Substring(0, 1000);
                        int lastPunc = Description.LastIndexOfAny(new [] {'.', '!', '?'});
                        int lastSpace = Description.LastIndexOf(' ');
                        if (lastPunc > lastSpace)
                            Description = Description.Substring(0, lastPunc + 1);
                        else
                            Description = Description.Substring(0, lastSpace) + '\u2026';
                    }
                    Description = System.Net.WebUtility.HtmlDecode(Description);
                    Description = Description.Clean();
                }
            }
            if (Reviews == 0)
            {
                try
                {
                    HtmlNode ratingNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrPopover']")
                        ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='fl acrStars']/span");
                    if (ratingNode != null)
                    {
                        string aRating = ratingNode.GetAttributeValue("title", "0");
                        AmazonRating = float.Parse(ratingNode.GetAttributeValue("title", "0").Substring(0, aRating.IndexOf(' ')));
                        HtmlNode reviewsNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrCustomerReviewText']")
                            ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-link-normal']");
                        if (reviewsNode != null)
                        {
                            Match match = Regex.Match(reviewsNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                            if (match.Success)
                                Reviews = int.Parse(match.Value.Replace(".", "").Replace(",", ""));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HtmlWebException("Error finding book ratings. If you want, you can report the book's Amazon URL to help with parsing.\r\n" +
                        "Error: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        public Bitmap CoverImage()
        {
            if (ImageUrl == "") return null;
            if (_bookImage != null) return _bookImage;
            _bookImage = Task.Run(() => HttpClient.GetImageAsync(ImageUrl)).Result;
            return _bookImage;
        }
    }
}
