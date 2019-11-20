using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Newtonsoft.Json;
using XRayBuilder.Core.DataSources.Amazon.Model;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Parsing.Regex;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.DataSources.Amazon
{
    public class AuthorSearchResults
    {
        public string Asin { get; set; }
        public string Biography { get; set; }
        public string ImageUrl { get; set; }
        /// <summary>
        /// First page of books from author's page
        /// </summary>
        public BookInfo[] Books { get; set; }
    }

    // TODO: Calling SearchAuthor then using the search results for all subsequent calls is kinda weird
    public class AmazonClient : IAmazonClient
    {
        private readonly IHttpClient _httpClient;
        private readonly IAmazonInfoParser _amazonInfoParser;
        private readonly ILogger _logger;

        private const int MaxParallelism = 5;
        private readonly Regex _regexAsin = new Regex("(?<asin>B[A-Z0-9]{9})", RegexOptions.Compiled);
        private readonly Regex _regexAsinUrl = new Regex("(/e/(?<asin>B\\w+)[/?]|dp/(?<asin>B[A-Z0-9]{9})/|/gp/product/(?<asin>B[A-Z0-9]{9}))", RegexOptions.Compiled);
        private readonly Regex _regexIgnoreHeaders = new Regex(@"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public AmazonClient(IHttpClient httpClient, IAmazonInfoParser amazonInfoParser, ILogger logger)
        {
            _httpClient = httpClient;
            _amazonInfoParser = amazonInfoParser;
            _logger = logger;
        }

        public static bool IsAsin(string asin) => Regex.IsMatch(asin, "^B[A-Z0-9]{9}$");

        [CanBeNull]
        public string ParseAsin(string input) => _regexAsin.MatchOrNull(input)?.Groups["asin"].Value;

        [CanBeNull]
        public string ParseAsinFromUrl(string input) => _regexAsinUrl.MatchOrNull(input)?.Groups["asin"].Value;

        public string Url(string tld, string asin) => $"https://www.amazon.{tld}/dp/{asin}";

        public async Task<AuthorSearchResults> SearchAuthor(string author, string bookAsin, string TLD, bool saveHtml, CancellationToken cancellationToken = default)
        {
            //Generate Author search URL from author's name
            var newAuthor = Functions.FixAuthor(author);
            var plusAuthorName = newAuthor.Replace(" ", "+");
            //Updated to match Search "all" Amazon
            var amazonAuthorSearchUrl = $"https://www.amazon.{TLD}/s/ref=nb_sb_noss_2?url=search-alias%3Dstripbooks&field-keywords={plusAuthorName}";
            _logger.Log($"Searching for author's page on Amazon.{TLD}...");

            // Search Amazon for Author
            var authorSearchDoc = await _httpClient.GetPageAsync(amazonAuthorSearchUrl, cancellationToken);

            if (saveHtml)
            {
                try
                {
                    _logger.Log("Saving Amazon's author search webpage...");
                    File.WriteAllText(Environment.CurrentDirectory + $"\\dmp\\{bookAsin}.authorsearchHtml.txt",
                        authorSearchDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("An error ocurred saving authorsearchHtml.txt: {0}", ex.Message));
                }
            }

            // Check for captcha
            try
            {
                _amazonInfoParser.CheckCaptcha(authorSearchDoc);
            }
            catch (AmazonCaptchaException)
            {
                _logger.Log($"Warning: Amazon.{TLD} is requesting a captcha."
                    + $"You can try visiting Amazon.{TLD} in a real browser first, try another region, or try again later.");
                return null;
            }
            // Try to find Author's page from Amazon search
            var properAuthor = "";
            HtmlNode[] possibleNodes = null;
            var node = authorSearchDoc.DocumentNode.SelectSingleNode("//*[@id='result_1']");
            if (node == null || !node.OuterHtml.Contains("/e/B"))
            {
                // If the wrong format of search page is returned, try to find author in the small links under book titles
                possibleNodes = authorSearchDoc.DocumentNode.SelectNodes(".//a[@class='a-size-base a-link-normal']")
                    ?.Where(possibleNode => possibleNode.InnerText != null && possibleNode.InnerText.Trim().Equals(newAuthor, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
                if (possibleNodes == null || (node = possibleNodes.FirstOrDefault()) == null)
                {
                    _logger.Log($"An error occurred finding author's page on Amazon.{TLD}." +
                                "\r\nUnable to create Author Profile." +
                                "\r\nEnsure the author metadata field matches the author's name exactly." +
                                $"\r\nSearch results can be viewed at {amazonAuthorSearchUrl}" +
                                "\r\nSometimes Amazon just doesn't return the author and trying a few times will work.");
                    return null;
                }
            }

            string authorAsin = null;
            if (possibleNodes != null && possibleNodes.Any())
            {
                // TODO Present a list of these to choose from
                node = possibleNodes.First();
                properAuthor = node.GetAttributeValue("href", "");
                authorAsin = ParseAsinFromUrl(properAuthor);
            }
            // Check for typical search results, second item is the author page
            else if ((node = node.SelectSingleNode("//*[@id='result_1']/div/div/div/div/a")) != null)
            {
                properAuthor = node.GetAttributeValue("href", "");
                authorAsin = node.GetAttributeValue("data-asin", null)
                    ?? ParseAsinFromUrl(properAuthor);
            }
            // otherwise check for "by so-and-so" text beneath the titles for a possible match
            else if ((node = authorSearchDoc.DocumentNode.SelectSingleNode($"//div[@id='resultsCol']//li[@class='s-result-item celwidget  ']//a[text()=\"{newAuthor}\"]")) != null)
            {
                properAuthor = node.GetAttributeValue("href", "");
                authorAsin = ParseAsinFromUrl(properAuthor);
            }

            if (node == null || string.IsNullOrEmpty(properAuthor) || properAuthor.IndexOf('/', 1) < 3 || string.IsNullOrEmpty(authorAsin))
            {
                _logger.Log("Unable to parse author's page URL properly. Try again later or report this URL on the MobileRead thread: " + amazonAuthorSearchUrl);
                return null;
            }
            properAuthor = properAuthor.Substring(1, properAuthor.IndexOf('/', 1) - 1);
            var authorAmazonWebsiteLocationLog = $"https://www.amazon.{TLD}/{properAuthor}/e/{authorAsin}";
            var authorAmazonWebsiteLocation = $"https://www.amazon.{TLD}{node.GetAttributeValue("href", "")}";

            _logger.Log($"Author page found on Amazon!\r\nAuthor's Amazon Page URL: {authorAmazonWebsiteLocationLog}");

            // Load Author's Amazon page
            var authorHtmlDoc = new HtmlDocument();
            var authorPage = await _httpClient.GetStreamAsync(authorAmazonWebsiteLocation, cancellationToken);
            authorHtmlDoc.Load(authorPage);

            // Try to find the Kindle Edition link
            // Either use the new one w/ only Kindle editions or the original
            // TODO: don't handle individual regions here...
            if (TLD == "com")
            {
                var kindleNode = authorHtmlDoc.DocumentNode.SelectSingleNode(".//a[@class='a-link-normal formatSelector']");
                if (kindleNode != null && kindleNode.InnerText.Trim() == "Kindle Edition")
                {
                    authorPage = await _httpClient.GetStreamAsync($"https://www.amazon.com{kindleNode.GetAttributeValue("href", "")}", cancellationToken);
                    authorHtmlDoc.Load(authorPage);
                }
            }
            else if (TLD == "co.uk")
            {
                var kindleNode = authorHtmlDoc.DocumentNode.SelectSingleNode(".//a[@class='a-link-normal']");
                if (kindleNode != null && kindleNode.InnerText.Trim() == "Kindle Books")
                {
                    authorPage = await _httpClient.GetStreamAsync($"https://www.amazon.co.uk{kindleNode.GetAttributeValue("href", "")}", cancellationToken);
                    authorHtmlDoc.Load(authorPage);
                }
            }

            string biography = null;
            try
            {
                biography = GetAuthorBiography(authorHtmlDoc);
            }
            catch (FormatChangedException)
            {
                _logger.Log("Warning: Amazon biography format changed or no biography available for this author.", LogLevel.Warn);
            }

            string authorImageUrl = null;
            try
            {
                authorImageUrl = GetAuthorImageUrl(authorHtmlDoc);
            }
            catch (FormatChangedException)
            {
                _logger.Log("Warning: Amazon author image format changed or no image is available for this author.", LogLevel.Warn);
            }

            var bookList = GetAuthorBooks(authorHtmlDoc, author, TLD);
            if (!bookList.Any())
                bookList = GetAuthorBooksNew(authorHtmlDoc, author, TLD);

            return new AuthorSearchResults
            {
                Asin = authorAsin,
                Biography = biography,
                ImageUrl = authorImageUrl,
                Books = bookList.ToArray()
            };
        }

        // Get biography from results page; TLD included in case different Amazon sites have different formatting
        private string GetAuthorBiography(HtmlDocument authorHtmlDoc)
        {
            var bioNode = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-bio' and @class='a-row']/div/div/span")
                   ?? authorHtmlDoc.DocumentNode.SelectSingleNode("//span[@id='author_biography']")
                   ?? throw new FormatChangedException(nameof(AmazonClient), "author bio");

            return bioNode.InnerText;
        }

        private string GetAuthorImageUrl(HtmlDocument authorHtmlDoc)
        {
            var imageNode = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-image']/img")
                   ?? authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='authorImage']/img")
                   ?? throw new FormatChangedException(nameof(AmazonClient), "author image");

            var authorImageUrl = Regex.Replace(imageNode.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

            // cleanup to match retail file image links
            if (authorImageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                authorImageUrl = authorImageUrl.Replace(@"https://images-na.ssl-images-amazon", @"http://ecx.images-amazon");

            return authorImageUrl;
        }

        /// <summary>
        /// As of 2018-07-31, format changed. For some amount of time, keep both just in case.
        /// </summary>
        private List<BookInfo> GetAuthorBooksNew(HtmlDocument authorHtmlDoc, string author, string tld)
        {
            var resultsNodes = authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='searchWidget']/div");
            if (resultsNodes == null) return null;
            var bookList = new List<BookInfo>(resultsNodes.Count);
            foreach (var result in resultsNodes)
            {
                if (result.InnerHtml.Contains("a-pagination"))
                    continue;
                var bookNodes = result.SelectNodes(".//div[@class='a-fixed-right-grid-inner']/div/div")
                    ?? throw new FormatChangedException(nameof(AmazonClient), "book results - title nodes");
                var name = bookNodes.FirstOrDefault()?.SelectSingleNode("./a")?.InnerText.Trim()
                    ?? throw new FormatChangedException(nameof(AmazonClient), "book results - title");

                // Skip known-bad things like lists and series and stuff
                if (_regexIgnoreHeaders.IsMatch(name))
                    continue;

                // Get first Kindle ASIN
                var asin = bookNodes.Select(bookNode => _regexAsinUrl.Match(bookNode.OuterHtml))
                    .FirstOrDefault(match => match.Success)
                    ?.Groups["asin"].Value;

                // TODO: This should be removable when the Kindle Only page is parsed instead
                if (string.IsNullOrEmpty(asin))
                    continue; //throw new DataSource.FormatChangedException(nameof(Amazon), "book results - kindle edition asin");
                bookList.Add(new BookInfo(name, author, asin)
                {
                    Tld = tld
                });
            }
            return bookList;
        }

        [NotNull]
        private List<BookInfo> GetAuthorBooks(HtmlDocument authorHtmlDoc, string author, string tld)
        {
            var bookList = new List<BookInfo>();

            var resultsNodes = authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='mainResults']/ul/li");
            if (resultsNodes == null)
                return bookList;
            // TODO: Reduce this duplication
            foreach (var result in resultsNodes)
            {
                if (!result.Id.StartsWith("result_")) continue;
                var otherBook = result.SelectSingleNode(".//div[@class='a-row a-spacing-small']/div/a/h2");
                if (otherBook == null) continue;
                // Skip known-bad things like lists and series and stuff
                if (_regexIgnoreHeaders.IsMatch(otherBook.InnerText))
                    continue;
                var name = otherBook.InnerText.Trim();
                otherBook = result.SelectSingleNode(".//*[@title='Kindle Edition']");
                var asin = ParseAsinFromUrl(otherBook.OuterHtml);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(asin))
                    bookList.Add(new BookInfo(name, author, asin));
            }

            // If no kindle books returned, try the top carousel
            if (bookList.Count == 0)
            {
                resultsNodes = authorHtmlDoc.DocumentNode.SelectNodes("//ol[@class='a-carousel' and @role ='list']/li");
                if (resultsNodes == null)
                    return bookList;
                foreach (var result in resultsNodes)
                {
                    var otherBook = result.SelectSingleNode(".//a/img");
                    if (otherBook == null) continue;
                    var name = otherBook.GetAttributeValue("alt", "");
                    // Skip known-bad things like lists and series and stuff
                    if (_regexIgnoreHeaders.IsMatch(name))
                        continue;
                    otherBook = result.SelectSingleNode(".//a");
                    if (otherBook == null) continue;
                    var asin = ParseAsinFromUrl(otherBook.OuterHtml);
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(asin))
                        bookList.Add(new BookInfo(name, author, asin)
                        {
                            Tld = tld
                        });
                }
            }
            return bookList;
        }

        // TODO: All calls to Amazon should check for the captcha page (or ideally avoid it somehow)
        public async Task<BookInfo> SearchBook(string title, string author, string TLD, CancellationToken cancellationToken = default)
        {
            BookInfo result = null;

            if (title.IndexOf(" (") >= 0)
                title = title.Substring(0, title.IndexOf(" ("));
            //Search "all" Amazon
            var searchUrl = string.Format(@"https://www.amazon.{0}/s/ref=nb_sb_noss?url=search-alias%3Daps&field-keywords={1}",
                TLD, Uri.EscapeDataString(title + " " + author));
            var searchDoc = await _httpClient.GetPageAsync(searchUrl, cancellationToken);
            var node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_0']");
            var nodeASIN = node?.SelectSingleNode(".//a[@title='Kindle Edition']");
            if (nodeASIN == null)
            {
                node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_1']");
                nodeASIN = node?.SelectSingleNode(".//a[@title='Kindle Edition']");
            }
            if (nodeASIN == null)
            {
                node = searchDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 's-result-item')]");
                nodeASIN = node?.SelectSingleNode(".//a[contains(text(),'Kindle')]");
            }
            //At least attempt to verify it might be the same book?
            // TODO improve the detection here
            if (node != null && nodeASIN != null
                             && (node.InnerText.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0
                                 || node.InnerText.IndexOf(WebUtility.HtmlEncode(title), StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var foundAsin = ParseAsinFromUrl(nodeASIN.OuterHtml);
                var titleNode = node.SelectSingleNode(".//div/div/div/div[@class='a-fixed-left-grid-col a-col-right']/div/a")
                    ?? node.SelectSingleNode(".//h2");
                if (titleNode != null)
                    result = new BookInfo(WebUtility.HtmlDecode(titleNode.InnerText.Trim()), author, foundAsin);
            }
            return result;
        }

        public async IAsyncEnumerable<BookInfo> EnhanceBookInfos(IEnumerable<BookInfo> books, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var book in books.Where(book => book != null))
            {
                try
                {
                    var infoResponse = await _amazonInfoParser.GetAndParseAmazonDocument(book.AmazonUrl, cancellationToken);
                    infoResponse.ApplyToBookInfo(book);
                }
                catch (Exception ex)
                {
                    throw new AggregateException($"Book: {book.Title}\r\nURL: {book.AmazonUrl}\r\nError: {ex.Message}", ex);
                }

                yield return book;
            }
        }

        public Task<string> DownloadStartActions(string asin, CancellationToken cancellationToken = default)
            => _httpClient.GetStringAsync($"https://www.revensoftware.com/amazon/sa/{asin}", cancellationToken);

        public async Task<NextBookResult> DownloadNextInSeries(string asin, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetStringAsync($"https://www.revensoftware.com/amazon/next/{asin}", cancellationToken);
            return JsonConvert.DeserializeObject<NextBookResult>(response);
        }
    }
}
