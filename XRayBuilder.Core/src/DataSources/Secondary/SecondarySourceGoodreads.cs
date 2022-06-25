using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using Ephemerality.Unpack;
using HtmlAgilityPack;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Language.Pluralization;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Logic.ReadingTime;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Artifacts;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceGoodreads : SecondarySource
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;
        private readonly IReadingTimeService _readingTimeService;

        private const int MaxConcurrentRequests = 10;

        public override string Name => "Goodreads";
        public override bool SearchEnabled { get; } = true;
        public override int UrlLabelPosition { get; } = 9;
        public override bool SupportsNotableClips { get; } = true;

        private readonly Regex _regexBookId = new Regex(@"/book/show/(?<id>[0-9]+)", RegexOptions.Compiled);
        private readonly Regex _regexEditions = new(@"editions/(?<editions>[0-9]*)-", RegexOptions.Compiled);
        private readonly Regex _regexPages = new(@"(?<pages>(\d+)|(\d+,\d+)) pages", RegexOptions.Compiled);

        public SecondarySourceGoodreads(ILogger logger, IHttpClient httpClient, IAmazonClient amazonClient, IReadingTimeService readingTimeService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
            _readingTimeService = readingTimeService;
        }

        private string ParseBookIdFromUrl(string input) => _regexBookId.Match(input).Groups["id"].Value;
        private string BookUrl(string id) => string.IsNullOrEmpty(id) ? null : $"https://www.goodreads.com/book/show/{id}";
        private string SearchUrl(string author, string title) => $"https://www.goodreads.com/search?q={author}%20{title}";
        private string SearchUrlAsin(string asin) => $"https://www.goodreads.com/search?q={asin}";

        public override bool IsMatchingUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.ToLowerInvariant().EndsWith("goodreads.com");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override async Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default)
        {
            var title = Uri.EscapeDataString(metadata.Title);
            var author = Uri.EscapeDataString(Functions.FixAuthor(metadata.Author));

            // Try by ASIN first, then fall back on author/title
            if (!string.IsNullOrEmpty(metadata.Asin))
            {
                _logger.Log($"Searching by ASIN...");
                var bookList = ParseSearchResults(await _httpClient.GetPageAsync(SearchUrlAsin(Uri.EscapeDataString(metadata.Asin)), cancellationToken));
                var searchBookAsync = bookList as BookInfo[] ?? bookList.ToArray();
                if (searchBookAsync.Any())
                    return searchBookAsync;
            }

            _logger.Log($"Searching by Author and Title...");
            return ParseSearchResults(await _httpClient.GetPageAsync(SearchUrl(author, title), cancellationToken));
        }

        private IEnumerable<BookInfo> ParseSearchResults(HtmlDocument goodreadsHtmlDoc)
        {
            if (goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
                yield break;

            var resultNodes = goodreadsHtmlDoc.DocumentNode.SelectNodes("//tr[@itemtype='http://schema.org/Book']");
            if (resultNodes == null)
                yield break;

            //Return a list of search results
            foreach (var link in resultNodes)
            {
                //Skip audiobook results
                if (link.SelectSingleNode(".//span[@class='authorName greyText smallText role']")?.InnerText.Contains("Audiobook") ?? false)
                    continue;
                var coverNode = link.SelectSingleNode(".//img");
                var titleNode = link.SelectSingleNode(".//a[@class='bookTitle']");
                var authorNode = link.SelectSingleNode(".//a[@class='authorName']");

                var cleanTitle = titleNode.InnerText.Trim().Replace("&amp;", "&").Replace("%27", "'").Replace("%20", " ");

                var newBook = new BookInfo(cleanTitle, authorNode.InnerText.Trim(), null)
                {
                    GoodreadsId = ParseBookIdFromUrl(link.OuterHtml)
                };

                newBook.DataUrl = BookUrl(newBook.GoodreadsId);

                newBook.ImageUrl = coverNode.GetAttributeValue("src", "");

                var ratingText = link.SelectSingleNode(".//span[@class='greyText smallText uitext']")?.InnerText.Clean();
                if (ratingText != null)
                {
                    var matchId = Regex.Match(ratingText, @"(\d+[\.,]?\d*) avg rating\s+(\d+(?:[\.,]?\d*)?).*\b(\d+) editions?");
                    if (matchId.Success)
                    {
                        newBook.AmazonRating = float.Parse(matchId.Groups[1].Value);
                        // Try with current culture, then one that uses . as a separator, then US
                        newBook.Reviews = matchId.Groups[2].Value.TryParseInt() ?? 0;
                        newBook.Editions = int.Parse(matchId.Groups[3].Value);
                    }
                }

                yield return newBook;
            }
        }

        /// <summary>
        /// Searches for the next and previous books in a series, if it is part of one.
        /// </summary>
        public override async Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            var series = new SeriesInfo();
            //Search Goodreads for series info
            var seriesPage = await _httpClient.GetPageAsync(dataUrl, cancellationToken);
            var metaNode = seriesPage.DocumentNode.SelectSingleNode("//div[@id='metacol']");
            var seriesNode = metaNode?.SelectSingleNode("//h2[@id='bookSeries']/a");
            if (seriesNode == null)
                return null;
            var match = Regex.Match(seriesNode.OuterHtml, @"/series/([0-9]*)");
            if (!match.Success)
                return null;
            series.Url = $"https://www.goodreads.com/series/{match.Groups[1].Value}";
            match = Regex.Match(seriesNode.InnerText, @"\((.*) #?([0-9]*([.,][0-9])?)\)");
            if (!match.Success)
                return null;

            series.Name = match.Groups[1].Value.Trim();
            series.Position = match.Groups[2].Value;

            var seriesHtmlDoc = await _httpClient.GetPageAsync(series.Url, cancellationToken);

            var bookNodes = seriesHtmlDoc.DocumentNode.SelectNodes("//div[@itemtype='http://schema.org/Book']");
            if (bookNodes == null)
                return series;

            series.Total = bookNodes
                .Select(node => node.PreviousSibling)
                .Count(node => Regex.IsMatch(node?.InnerText.ToUpper() ?? "", @"^BOOK ([0-9]+)$"));

            if (series.Total == 0)
                return series;

            var positionInt = (int)Convert.ToDouble(series.Position, CultureInfo.InvariantCulture.NumberFormat);

            var prevSearch = $"book {positionInt - 1}";
            var nextSearch = $"book {positionInt + 1}";

            async Task<BookInfo> ParseSeriesBook(HtmlNode bookNode)
            {
                var titleNode = bookNode.SelectSingleNode(".//div[@class='u-paddingBottomXSmall']/a");
                if (titleNode == null)
                    return null;
                var title = Regex.Replace(titleNode.InnerText.Trim(), @" \(.*\)", "", RegexOptions.Compiled);
                title = WebUtility.HtmlDecode(title);
                var goodreadsId = ParseBookIdFromUrl(titleNode.GetAttributeValue("href", ""));
                // TODO: move this ASIN search somewhere else
                if (string.IsNullOrEmpty(goodreadsId))
                    return null;
                var asin = await SearchBookASINById(goodreadsId, cancellationToken).ConfigureAwait(false);
                var author = bookNode.SelectSingleNode(".//span[@itemprop='author']//a")?.InnerText.Trim() ?? "";
                return new BookInfo(title, author, asin)
                {
                    GoodreadsId = goodreadsId
                };
            }

            foreach (var bookNode in bookNodes)
            {
                var bookIndex = bookNode.SelectSingleNode(".//div[@class='responsiveBook__header']")
                    ?? bookNode.ParentNode.FirstChild;
                if (bookIndex == null) continue;

                var bookIndexText = bookIndex.InnerText.ToLower();

                if (bookIndexText == prevSearch && series.Previous == null)
                    series.Previous = await ParseSeriesBook(bookNode);
                else if (bookIndexText == nextSearch)
                    series.Next = await ParseSeriesBook(bookNode);

                if (series.Previous != null && (series.Next != null || positionInt == series.Total))
                    break; // next and prev found or prev found and latest in series
            }

            return series;
        }

        // Search Goodreads for possible kindle edition of book and return ASIN.
        public async Task<string> SearchBookASINById(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var bookHtmlDoc = await _httpClient.GetPageAsync(BookUrl(id), cancellationToken);
                var link = bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='otherEditionsActions']/a");
                var match = _regexEditions.Match(link.GetAttributeValue("href", ""));
                if (match.Success)
                {
                    var kindleEditionsUrl = $"https://www.goodreads.com/work/editions/{match.Groups["editions"].Value}?utf8=%E2%9C%93&sort=num_ratings&filter_by_format=Kindle+Edition";
                    bookHtmlDoc = await _httpClient.GetPageAsync(kindleEditionsUrl, cancellationToken);
                    var bookNodes = bookHtmlDoc.DocumentNode.SelectNodes("//div[@class='elementList clearFix']");
                    var asin = bookNodes?.Select(book => _amazonClient.ParseAsin(book.InnerHtml))
                        .FirstOrDefault(possibleAsin => !string.IsNullOrEmpty(possibleAsin));

                    if (asin != null)
                        return asin;
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred while searching for {id}'s ASIN.\r\n{ex.Message}\r\n{ex.StackTrace}");
                return "";
            }
        }

        // TODO: This shouldn't modify curbook
        public override async Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            var bookPage = await _httpClient.GetPageAsync(curBook.DataUrl, cancellationToken);
            var pagesNode = bookPage.DocumentNode.SelectSingleNode("//div[@id='details']");
            if (pagesNode == null)
                return false;
            var match = _regexPages.Match(pagesNode.InnerText);
            if (!match.Success)
                return false;

            var pages = int.Parse(match.Groups["pages"].Value, NumberStyles.AllowThousands);
            if (pages != 0)
            {
                var readingTime = _readingTimeService.GetReadingTime(pages);

                curBook.PageCount = pages;
                curBook.ReadingHours = readingTime.Hours;
                curBook.ReadingMinutes = readingTime.Minutes;

                _logger.Log(_readingTimeService.GetFormattedReadingTime(pages));
            }

            return true;
        }

        public override async Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            _logger.Log("Downloading Goodreads page...");
            var grDoc = await _httpClient.GetPageAsync(dataUrl, cancellationToken);
            var charNodes = grDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/a");
            if (charNodes == null) return new List<Term>();
            // Check if ...more link exists on Goodreads page
            var moreCharNodes = grDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/span[@class='toggleContent']/a");
            var allChars = moreCharNodes == null ? charNodes : charNodes.Concat(moreCharNodes);
            var termCount = moreCharNodes == null ? charNodes.Count : charNodes.Count + moreCharNodes.Count;
            _logger.Log($"Gathering term information from Goodreads... ({termCount})");
            progress?.Set(0, termCount);
            if (termCount > 20)
                _logger.Log("More than 20 characters found. Consider using the 'download to XML' option if you need to build repeatedly.");
            var terms = new ConcurrentBag<Term>();
            await allChars.ParallelForEachAsync(async charNode =>
            {
                try
                {
                    terms.AddNotNull(await GetTermAsync(dataUrl, charNode.GetAttributeValue("href", ""), cancellationToken).ConfigureAwait(false));
                    progress?.Add(1);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(404)"))
                        _logger.Log($"Error getting page for character. URL: https://www.goodreads.com{charNode.GetAttributeValue("href", "")}\r\nMessage: {ex.Message}\r\n{ex.StackTrace}");
                }
            }, MaxConcurrentRequests, cancellationToken);
            return terms.ToList();
        }

        // Are there actually any goodreads pages that aren't at goodreads.com for other languages??
        private async Task<Term> GetTermAsync(string baseUrl, string relativeUrl, CancellationToken cancellationToken = default)
        {
            var result = new Term
            {
                Type = "character"
            };
            var tempUri = new Uri(baseUrl);
            tempUri = new Uri(new Uri(tempUri.GetLeftPart(UriPartial.Authority)), relativeUrl);
            result.DescSrc = "Goodreads";
            result.DescUrl = tempUri.ToString();
            var charDoc = await _httpClient.GetPageAsync(tempUri.ToString(), cancellationToken);
            var mainNode = charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat']")
                ?? charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat ']");
            result.TermName = mainNode.SelectSingleNode("./h1").InnerText;
            mainNode = mainNode.SelectSingleNode("//div[@class='grey500BoxContent']");
            var tempNodes = mainNode.SelectNodes("//div[@class='floatingBox']");
            if (tempNodes == null) return result;
            foreach (var tempNode in tempNodes)
            {
                if (tempNode.Id.Contains("_aliases")) // If present, add any aliases found
                {
                    var aliasStr = tempNode.InnerText.Replace("[close]", "").Trim();
                    var newAliases = aliasStr.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)
                        .Where(alias => alias != result.TermName)
                        .ToHashSet();
                    result.Aliases.AddRange(newAliases);
                }
                else
                    result.Desc = tempNode.InnerText.Replace("[close]", "").Trim();
            }
            return result;
        }

        /// <summary>
        /// Gather the list of quotes & number of times they've been liked -- close enough to "x paragraphs have been highlighted y times" from Amazon
        /// </summary>
        public override async Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            srcDoc ??= await _httpClient.GetPageAsync(url, cancellationToken);
            var quoteNode = srcDoc.DocumentNode.SelectSingleNode("//div[@class='h2Container gradientHeaderContainer']/h2/a[starts-with(.,'Quotes from')]");
            if (quoteNode == null) return null;
            var quoteURL = $"https://www.goodreads.com{quoteNode.GetAttributeValue("href", "")}?page={{0}}";
            progress?.Set(0, 1);

            var quoteBag = new ConcurrentBag<IEnumerable<NotableClip>>();
            var initialPage = await _httpClient.GetPageAsync(string.Format(quoteURL, 1), cancellationToken);

            // check how many pages there are (find previous page button, get parent div, take all children of that, 2nd last one should be the max page count
            var maxPageNode = initialPage.DocumentNode.SelectSingleNode("//span[contains(@class,'previous_page')]/parent::div/*[last()-1]");
            if (!int.TryParse(maxPageNode?.InnerHtml, out var maxPages))
                maxPages = 1;

            IEnumerable<NotableClip> ParseQuotePage(HtmlDocument quoteDoc)
            {
                var tempNodes = quoteDoc.DocumentNode.SelectNodes("//div[@class='quotes']/div[@class='quote']");
                return tempNodes?.Select(node =>
                {
                    var quoteMatch = Regex.Match(node.InnerText, "&ldquo;(.*?)&rdquo;", RegexOptions.Compiled);
                    var likesMatch = Regex.Match(node.SelectSingleNode(".//div[@class='right']/a")?.InnerText ?? "",
                        @"(\d+) likes", RegexOptions.Compiled);
                    if (!quoteMatch.Success || !likesMatch.Success) return null;
                    return new NotableClip
                    {
                        Text = quoteMatch.Groups[1].Value,
                        Likes = int.Parse(likesMatch.Groups[1].Value)
                    };
                }).Where(quote => quote != null);
            }

            quoteBag.Add(ParseQuotePage(initialPage));
            progress?.Set(1, maxPages);
            await Enumerable.Range(2, maxPages - 1).ParallelForEachAsync(async page =>
            {
                var quotePage = await _httpClient.GetPageAsync(string.Format(quoteURL, page), cancellationToken);
                quoteBag.Add(ParseQuotePage(quotePage));
                progress?.Add(1);
            }, MaxConcurrentRequests, cancellationToken);

            return quoteBag.Where(quotes => quotes != null && quotes.Any()).SelectMany(quotes => quotes).ToList();
        }

        /// <summary>
        /// Scrape any notable quotes from Goodreads and grab ratings if missing from book info
        /// Modifies curBook.
        /// </summary>
        public override async Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            var grDoc = await _httpClient.GetPageAsync(curBook.DataUrl, cancellationToken);
            curBook.NotableClips ??= (await GetNotableClipsAsync("", grDoc, progress, cancellationToken).ConfigureAwait(false))?.ToList();

            //Add rating and reviews count if missing from Amazon book info
            var metaNode = grDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
            if (metaNode != null && curBook.AmazonRating is 0 or null)
            {
                var ratingNode = metaNode.SelectSingleNode("//span[@class='value rating']")
                    ?? metaNode.SelectSingleNode(".//span[@itemprop='ratingValue']");
                if (ratingNode != null)
                    curBook.AmazonRating = Math.Round(float.Parse(ratingNode.InnerText), 2);
                var reviewsNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']")
                    ?? metaNode.SelectSingleNode(".//span[@class='count value-title']")
                    ?? metaNode.SelectSingleNode(".//a[@href='#other_reviews']");
                if (reviewsNode != null)
                {
                    var match = Regex.Match(reviewsNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                    if (match.Success)
                        curBook.Reviews = int.Parse(match.Value.Replace(",", "").Replace(".", ""));
                }
                reviewsNode = metaNode.SelectSingleNode(".//meta[@itemprop='reviewCount']");
                if (reviewsNode != null && int.TryParse(reviewsNode.GetAttributeValue("content", null), out var reviews))
                {
                    curBook.Reviews += reviews;
                }
            }
        }

        /// <summary>
        /// Scrape author info from Goodreads
        /// </summary>
        public async Task<AuthorSearchResults> GetAuthorAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            var result = new AuthorSearchResults();

            var grDoc = await _httpClient.GetPageAsync(dataUrl, cancellationToken);

            var aboutNode = grDoc.DocumentNode.SelectSingleNode("//div[@class='bookAuthorProfile']");
            if (aboutNode == null)
                return result;

            var bioNodes = aboutNode.SelectNodes("//div[@class='bookAuthorProfile__about']/span/text()[normalize-space(.) != '']");
            if (bioNodes != null)
            {
                var nodes = bioNodes.Select(n => HtmlEntity.DeEntitize(n.InnerText.Trim())).ToList();
                result.Biography = string.Join(" ", nodes);
            }

            var imgNode = aboutNode.SelectSingleNode(".//div[@class='bookAuthorProfile__photo']");
            if (imgNode != null)
            {
                var attr = imgNode.GetAttributeValue("style", "");
                var match = Regex.Match(attr, @"background-image: url\((.*)\)");
                if (match.Success)
                    result.ImageUrl = match.Groups[1].Value.Replace("p3/", "p8/");
            }

            return result;
        }
    }
}
