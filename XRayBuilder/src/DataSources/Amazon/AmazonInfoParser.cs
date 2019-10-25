using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Http;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Model;

namespace XRayBuilderGUI.DataSources.Amazon
{
    public class AmazonInfoParser : IAmazonInfoParser
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;

        public AmazonInfoParser(ILogger logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public class InfoResponse
        {
            public string ImageUrl { get; set; }
            public string Description { get; set; }
            public int Reviews { get; set; }
            public float Rating { get; set; }

            public void ApplyToBookInfo(BookInfo bookInfo)
            {
                bookInfo.ImageUrl = ImageUrl;
                bookInfo.Description = Description;
                bookInfo.Reviews = Reviews;
                bookInfo.AmazonRating = Rating;
            }
        }

        public async Task<InfoResponse> GetAndParseAmazonDocument(string amazonUrl, CancellationToken cancellationToken = default)
        {
            var doc = await _httpClient.GetPageAsync(amazonUrl, cancellationToken);
            return ParseAmazonDocument(doc);
        }

        /// <summary>
        /// Retrieves a book's description, image URL, and rating from the Amazon document
        /// </summary>
        public InfoResponse ParseAmazonDocument(HtmlDocument bookDoc)
        {
            var response = new InfoResponse();

            #region Image URL
            var bookImageLoc = bookDoc.DocumentNode.SelectSingleNode("//*[@id='imgBlkFront']")
                ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='imageBlock']")
                ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='series-detail-product-image']")
                ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='ebooksImgBlkFront']") //co.uk seems to use this id sometimes
                                                                                         // for more generic matching, such as on audiobooks (which apparently have BXXXXXXXX asins also)
                ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='main-image']");

            if (bookImageLoc == null)
                _logger.Log("Error finding book image.");
            else
            {
                var imageUrl = Regex.Replace(bookImageLoc.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);
                if (imageUrl.Contains("base64"))
                {
                    imageUrl = bookImageLoc.GetAttributeValue("data-a-dynamic-image", "");
                    var match = Regex.Match(imageUrl, @"(https://.*?_\.(jpg|jpeg|gif|png))");
                    if (match.Success)
                    {
                        imageUrl = match.Groups[1].Value;
                        if (!imageUrl.EndsWith(".png"))
                            imageUrl = Regex.Replace(imageUrl, @"_.*?_\.", string.Empty);
                    }
                }

                // cleanup to match retail file image links
                if (imageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                    imageUrl = imageUrl.Replace(@"https://images-na.ssl-images-amazon", @"http://ecx.images-amazon");

                // Use no image URL
                if (string.IsNullOrEmpty(imageUrl))
                    imageUrl = "http://ecx.images-amazon.com/images/G/01/x-site/icons/no-img-sm.gif";

                response.ImageUrl = imageUrl;
            }

            #endregion

            #region Description
            var descNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='bookDescription_feature_div']/noscript")
                ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-size-medium series-detail-description-text']");
            if (descNode != null && descNode.InnerText != "")
            {
                var description = descNode.InnerText.Trim();
                // Following the example of Amazon, cut off desc around 1000 characters.
                // If conveniently trimmed at the end of the sentence, let it end with the punctuation.
                // If the sentence continues, cut it off and replace the space with an ellipsis
                if (description.Length > 1000)
                {
                    description = description.Substring(0, 1000);
                    var lastPunc = description.LastIndexOfAny(new[] { '.', '!', '?' });
                    var lastSpace = description.LastIndexOf(' ');
                    if (lastPunc > lastSpace)
                        description = description.Substring(0, lastPunc + 1);
                    else
                        description = description.Substring(0, lastSpace) + '\u2026';
                }
                description = System.Net.WebUtility.HtmlDecode(description);
                response.Description = description.Clean();
            }
            #endregion

            #region Reviews
            try
            {
                var ratingNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrPopover']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='fl acrStars']/span");
                if (ratingNode != null)
                {
                    var aRating = ratingNode.GetAttributeValue("title", "0");
                    response.Rating = float.Parse(ratingNode.GetAttributeValue("title", "0").Substring(0, aRating.IndexOf(' ')));
                    var reviewsNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrCustomerReviewText']")
                        ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-link-normal']");
                    if (reviewsNode != null)
                    {
                        var match = Regex.Match(reviewsNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                        if (match.Success)
                            response.Reviews = int.Parse(match.Value.Replace(".", "").Replace(",", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException("Error finding book ratings. If you want, you can report the book's Amazon URL to help with parsing.\r\n" +
                    "Error: " + ex.Message + "\r\n" + ex.StackTrace, ex);
            }
            #endregion

            return response;
        }
    }
}
