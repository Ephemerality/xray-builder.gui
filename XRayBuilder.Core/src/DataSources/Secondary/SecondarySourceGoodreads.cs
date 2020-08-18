﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
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
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Artifacts;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceGoodreads : ISecondarySource
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;

        private const int MaxConcurrentRequests = 10;

        public string Name => "Goodreads";
        public bool SearchEnabled { get; } = true;
        public int UrlLabelPosition { get; } = 6;
        public bool SupportsNotableClips { get; } = true;

        private readonly Regex _regexBookId = new Regex(@"/book/show/(?<id>[0-9]+)", RegexOptions.Compiled);

        public SecondarySourceGoodreads(ILogger logger, IHttpClient httpClient, IAmazonClient amazonClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
        }

        private string ParseBookIdFromUrl(string input) => _regexBookId.Match(input).Groups["id"].Value;
        private string BookUrl(string id) => string.IsNullOrEmpty(id) ? null : $"https://www.goodreads.com/book/show/{id}";
        private string SearchUrl(string author, string title) => $"https://www.goodreads.com/search?q={author}%20{title}";
        private string SearchUrlAsin(string asin) => $"https://www.goodreads.com/search?q={asin}";

        public async Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default)
        {
            // Try by ASIN first, then fall back on author/title
            if (!string.IsNullOrEmpty(metadata.Asin))
            {
                return ParseSearchResults(await _httpClient.GetPageAsync(SearchUrlAsin(Uri.EscapeDataString(metadata.Asin)), cancellationToken));
            }

            var title = Uri.EscapeDataString(metadata.Title);
            var author = Uri.EscapeDataString(Functions.FixAuthor(metadata.Author));

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

                var newBook = new BookInfo(cleanTitle, authorNode.InnerText.Trim(), null);

                newBook.GoodreadsId = ParseBookIdFromUrl(link.OuterHtml);
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
        public async Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
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

            seriesNode = seriesHtmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'responsiveSeriesHeader__subtitle')]");
            match = Regex.Match(seriesNode?.InnerText ?? "", @"([0-9]+) (?:primary )?works?");
            if (match.Success)
                series.Total = int.Parse(match.Groups[1].Value);

            var positionInt = (int)Convert.ToDouble(series.Position, CultureInfo.InvariantCulture.NumberFormat);
            var totalInt = (int)Convert.ToDouble(series.Total, CultureInfo.InvariantCulture.NumberFormat);

            var bookNodes = seriesHtmlDoc.DocumentNode.SelectNodes("//div[@itemtype='http://schema.org/Book']");
            if (bookNodes == null)
                return series;
            var prevSearch = series.Position.Contains(".")
                ? $"book {positionInt}"
                : $"book {positionInt - 1}";
            var nextSearch = $"book {positionInt + 1}";

            async Task<BookInfo> ParseSeriesBook(HtmlNode bookNode)
            {
                var book = new BookInfo("", "", "");
                var title = bookNode.SelectSingleNode(".//div[@class='u-paddingBottomXSmall']/a");
                book.Title = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", "", RegexOptions.Compiled);
                book.Title = WebUtility.HtmlDecode(book.Title);
                book.GoodreadsId = ParseBookIdFromUrl(title.GetAttributeValue("href", ""));
                // TODO: move this ASIN search somewhere else
                if (!string.IsNullOrEmpty(book.GoodreadsId))
                    book.Asin = await SearchBookASINById(book.GoodreadsId, cancellationToken).ConfigureAwait(false);
                book.Author = bookNode.SelectSingleNode(".//span[@itemprop='author']//a")?.InnerText.Trim() ?? "";
                return book;
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

                if (series.Previous != null && (series.Next != null || positionInt == totalInt))
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
                var match = Regex.Match(link.GetAttributeValue("href", ""), @"editions/([0-9]*)-");
                if (match.Success)
                {
                    var kindleEditionsUrl = string.Format("https://www.goodreads.com/work/editions/{0}?utf8=%E2%9C%93&sort=num_ratings&filter_by_format=Kindle+Edition", match.Groups[1].Value);
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
        public async Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            var bookPage = await _httpClient.GetPageAsync(curBook.DataUrl, cancellationToken);
            var pagesNode = bookPage.DocumentNode.SelectSingleNode("//div[@id='details']");
            if (pagesNode == null)
                return false;
            var match = Regex.Match(pagesNode.InnerText, @"((\d+)|(\d+,\d+)) pages");
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                var span = TimeSpan.FromMinutes(minutes);
                // Functions.Pluralize($"{BookList[i].editions:edition}")
                _logger.Log(string.Format("Typical time to read: {0}, {1}, and {2} ({3} pages)",
                    PluralUtil.Pluralize($"{span.Days:day}"),
                    PluralUtil.Pluralize($"{span.Hours:hour}"),
                    PluralUtil.Pluralize($"{span.Minutes:minute}"),
                    match.Groups[1].Value));
                curBook.PagesInBook = int.Parse(match.Groups[1].Value);
                curBook.ReadingHours = span.Hours;
                curBook.ReadingMinutes = span.Minutes;
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
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
                        _logger.Log("Error getting page for character. URL: " + "https://www.goodreads.com" + charNode.GetAttributeValue("href", "")
                            + "\r\nMessage: " + ex.Message + "\r\n" + ex.StackTrace);
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
        public async Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            if (srcDoc == null)
            {
                srcDoc = await _httpClient.GetPageAsync(url, cancellationToken);
            }
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
        public async Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            var grDoc = await _httpClient.GetPageAsync(curBook.DataUrl, cancellationToken);
            if (curBook.NotableClips == null)
            {
                curBook.NotableClips = (await GetNotableClipsAsync("", grDoc, progress, cancellationToken).ConfigureAwait(false))?.ToList();
            }

            //Add rating and reviews count if missing from Amazon book info
            var metaNode = grDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
            if (metaNode != null && !curBook.AmazonRating.HasValue)
            {
                var goodreadsRating = metaNode.SelectSingleNode("//span[@class='value rating']")
                    ?? metaNode.SelectSingleNode(".//span[@itemprop='ratingValue']");
                if (goodreadsRating != null)
                    curBook.AmazonRating = Math.Round(float.Parse(goodreadsRating.InnerText), 2);
                var passagesNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']")
                    ?? metaNode.SelectSingleNode(".//span[@class='count value-title']");
                if (passagesNode != null)
                {
                    var match = Regex.Match(passagesNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                    if (match.Success)
                        curBook.Reviews = int.Parse(match.Value.Replace(",", "").Replace(".", ""));
                }
                passagesNode = metaNode.SelectSingleNode(".//meta[@itemprop='reviewCount']");
                if (passagesNode != null && int.TryParse(passagesNode.GetAttributeValue("content", null), out var reviews))
                {
                    curBook.Reviews = reviews;
                }
            }
        }
    }
}
