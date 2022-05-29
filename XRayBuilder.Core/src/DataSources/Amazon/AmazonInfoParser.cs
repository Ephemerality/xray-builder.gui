using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Amazon.Model;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.DataSources.Amazon;

public class AmazonInfoParser : IAmazonInfoParser
{
    private readonly ILogger _logger;
    private readonly IHttpClient _httpClient;

    private readonly Regex _numbersRegex = new(@"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)", RegexOptions.Compiled);
    private readonly Regex _regex404 = new(@"(cs_404_logo|cs_404_link|Page Not Found)", RegexOptions.Compiled);

    private readonly Regex _dirtyParas = new(@"^<[^>]+>.*<[^>]+>$|^&apos;|&apos;$|\*|_", RegexOptions.Compiled);

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
        public int Pages { get; set; }

        public void ApplyToBookInfo(BookInfo bookInfo)
        {
            bookInfo.ImageUrl = ImageUrl;
            bookInfo.Description = Description;
            bookInfo.Reviews = Reviews;
            bookInfo.AmazonRating = Rating;
            bookInfo.PageCount = Pages;
        }
    }

    public async Task<InfoResponse> GetAndParseAmazonDocument(string amazonUrl, CancellationToken cancellationToken = default)
    {
        var doc = await _httpClient.GetPageAsync(amazonUrl, cancellationToken);
        return ParseAmazonDocument(doc);
    }

    public void CheckCaptcha(HtmlDocument doc)
    {
        var captchaCheck = doc.DocumentNode.SelectSingleNode("//input[@id='captchacharacters']");
        if (captchaCheck != null)
            throw new AmazonCaptchaException();
    }

    /// <summary>
    /// Retrieves a book's description, image URL, and rating from the Amazon document
    /// </summary>
    public InfoResponse ParseAmazonDocument(HtmlDocument bookDoc)
    {
        var response = new InfoResponse();

        if (_regex404.IsMatch(bookDoc.DocumentNode.InnerHtml))
            return response;

        CheckCaptcha(bookDoc);

        #region Image URL

        var bookImageLoc = bookDoc.DocumentNode.SelectSingleNode("//*[@id='imgBlkFront']")
                           ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='imageBlock']")
                           ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='series-detail-product-image']")
                           ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='ebooksImgBlkFront']") //co.uk seems to use this id sometimes
                           // for more generic matching, such as on audiobooks (which apparently have BXXXXXXXX asins also)
                           ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='main-image']");

        if (bookImageLoc == null)
        {
            _logger.Log("Error finding book image.");
        }
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
                       ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='bookDescription_feature_div']/div[@data-a-expander-name='book_description_expander']")
                       ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-size-medium series-detail-description-text']");
        if (descNode != null && descNode.InnerText != "")
        {
            // TODO: If this proves better than InnerHtml.Clean(), and is reliable, use in other places instead of Clean()?
            // Match all p nodes in the description node.
            // Filter out any node starting and ending with a html style tag
            // "Should" remove single bold, italic and heading lines to leave a cleaner description.
            // Then replace known entities with characters using HtmlEntity.DeEntitize and join strings (with spaces if needed) to form description.
            var nodes = descNode.SelectNodes(".//p/span[not(contains(@class,'bold'))]")
                        ?? descNode.SelectNodes(".//div/span[not(contains(@class,'bold'))]");

            var description = string.Empty;
            if (nodes != null)
            {
                var filteredNodes = nodes
                    //.Where(node => !_dirtyParas.IsMatch(node.InnerHtml.Trim()))
                    .Select(node => HtmlEntity.DeEntitize(node.InnerText.Trim()))
                    .Where(node => node.Length > 1)
                    .ToArray();

                foreach (var node in filteredNodes)
                    if (char.IsUpper(node[0])) // | (description.Length != 0 && char.IsLower(description[^1])))
                        description += " " + node;
                    else
                        description += node;

                description = description.Clean();

                //description = filteredNodes.Any()
                //    ? filteredNodes.Aggregate(description, (current, s) => current + (char.IsLower(s[0]) ? s : s + " ")).Trim()
                //    : descNode.InnerHtml.Clean();
            }
            else
            {
                description = descNode.InnerText.Clean();
            }

            // Following the example of Amazon, cut off desc around 1000 characters.
            // If conveniently trimmed at the end of the sentence, let it end with the punctuation.
            // If the sentence continues, cut it off and replace the space with an ellipsis
            //if (description.Length > 1000)
            //{
            //    description = description.Substring(0, 1000);
            //    var lastPunc = description.LastIndexOfAny(new[] { '.', '!', '?' });
            //    var lastSpace = description.LastIndexOf(' ');
            //    if (lastPunc > lastSpace)
            //        description = description.Substring(0, lastPunc + 1);
            //    else
            //        description = $"{description.Substring(0, lastSpace - 1)}{'\u2026'}";
            //}

            // Used to test output from Amazon description parsing
            //var descsCheckFile = AppDomain.CurrentDomain.BaseDirectory + @"\descs.txt";
            //File.AppendAllText(descsCheckFile, description + Environment.NewLine + Environment.NewLine);

            response.Description = description;
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
            throw new AggregateException($"Error finding book ratings. If you want, you can report the book's Amazon URL to help with parsing.\r\nError: {ex.Message}\r\n{ex.StackTrace}", ex);
        }

        #endregion

        #region Pages

        var pagesNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='aboutEbooksSection']/table/tr/td");
        if (!string.IsNullOrEmpty(pagesNode?.InnerText))
        {
            var match = _numbersRegex.Match(pagesNode.InnerText);
            if (!match.Success)
            {
                pagesNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='productDetailsTable']/tr/td");
                if (!string.IsNullOrEmpty(pagesNode?.InnerText))
                {
                    var lengthNode = pagesNode.SelectSingleNode(".//li[contains(text(),'pages')]");
                    if (lengthNode != null)
                        match = _numbersRegex.Match(lengthNode.InnerText);
                }
            }

            if (match.Success)
                response.Pages = int.Parse(match.Value);
        }

        if (pagesNode == null)
        {
            pagesNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='detailBullets_feature_div']/ul");
            var lengthNode = pagesNode?.SelectSingleNode(".//span[contains(text(),'pages')]");
            if (lengthNode != null)
            {
                var match = _numbersRegex.Match(lengthNode.InnerText);
                if (match.Success)
                    response.Pages = int.Parse(match.Value);
            }
        }

        #endregion

        return response;
    }
}