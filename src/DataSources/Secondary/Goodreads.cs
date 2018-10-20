using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.UI;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI.DataSources.Secondary
{
    public class Goodreads : ISecondarySource
    {
        private const string BookUrl = "https://www.goodreads.com/book/show/{0}";
        private const int MaxConcurrentRequests = 10;
        private HtmlDocument sourceHtmlDoc;

        private Properties.Settings settings = Properties.Settings.Default;

        private frmASIN frmAS = new frmASIN();

        public string Name => "Goodreads";

        public async Task<IEnumerable<BookInfo>> SearchBookAsync(string author, string title, CancellationToken cancellationToken = default)
        {
            var goodreadsSearchUrlBase = @"https://www.goodreads.com/search?q={0}%20{1}";

            title = Uri.EscapeDataString(title);
            author = Uri.EscapeDataString(Functions.FixAuthor(author));

            var goodreadsHtmlDoc = new HtmlDocument();
            goodreadsHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(String.Format(goodreadsSearchUrlBase, author, title)));
            return !goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results")
                ? ParseSearchResults(goodreadsHtmlDoc)
                : null;
        }

        private IEnumerable<BookInfo> ParseSearchResults(HtmlDocument goodreadsHtmlDoc, CancellationToken cancellationToken = default)
        {
            HtmlNodeCollection resultNodes = goodreadsHtmlDoc.DocumentNode.SelectNodes("//tr[@itemtype='http://schema.org/Book']");
            //Return a list of search results
            foreach (HtmlNode link in resultNodes)
            {
                //Skip audiobook results
                if (link.SelectSingleNode(".//span[@class='authorName greyText smallText role']")?.InnerText.Contains("Audiobook") ?? false)
                    continue;
                var coverNode = link.SelectSingleNode(".//img");
                var titleNode = link.SelectSingleNode(".//a[@class='bookTitle']");
                var authorNode = link.SelectSingleNode(".//a[@class='authorName']");

                var cleanTitle = titleNode.InnerText.Trim().Replace("&amp;", "&").Replace("%27", "'").Replace("%20", " ");

                BookInfo newBook = new BookInfo(cleanTitle, authorNode.InnerText.Trim(), null);

                Match matchId = Regex.Match(link.OuterHtml, @"./book/show/([0-9]+)");
                if (matchId.Success)
                {
                    newBook.goodreadsID = matchId.Groups[1].Value;
                    newBook.dataUrl = string.Format(BookUrl, matchId.Groups[1].Value);
                }

                newBook.bookImageUrl = coverNode.GetAttributeValue("src", "");

                var ratingText = link.SelectSingleNode(".//span[@class='greyText smallText uitext']")?.InnerText.Clean();
                if (ratingText != null)
                {
                    matchId = Regex.Match(ratingText, @"(\d+[\.,]?\d*) avg rating\s+(\d+(?:[\.,]?\d*)?).*\b(\d+) editions?");
                    if (matchId.Success)
                    {
                        newBook.amazonRating = float.Parse(matchId.Groups[1].Value);
                        // Try with current culture, then one that uses . as a separator, then US
                        var numReviews = matchId.Groups[2].Value.TryParseInt(NumberStyles.AllowThousands, CultureInfo.CurrentCulture)
                                         ?? matchId.Groups[2].Value.TryParseInt(NumberStyles.AllowThousands, new CultureInfo("nl-NL"))
                                         ?? matchId.Groups[2].Value.TryParseInt(NumberStyles.AllowThousands, new CultureInfo("en-US"));
                        newBook.numReviews = numReviews ?? 0;
                        newBook.editions = int.Parse(matchId.Groups[3].Value);
                    }
                }

                yield return newBook;
            }
        }

        /// <summary>
        /// Searches for the next and previous books in a series, if it is part of one.
        /// Modifies curBook.previousInSeries to contain the found book info.
        /// </summary>
        /// <returns>Next book in series</returns>
        public async Task<BookInfo> GetNextInSeriesAsync(BookInfo curBook, AuthorProfile authorProfile, string TLD, CancellationToken cancellationToken = default)
        {
            BookInfo nextBook = null;

            if (curBook.dataUrl == "") return null;
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }

            // Get title of next book
            Dictionary<string, BookInfo> seriesInfo = await GetNextInSeriesTitleAsync(curBook);
            if (seriesInfo.TryGetValue("Next", out var book))
            {
                // TODO: next and previous sections are the same...
                // Search author's other books for the book (assumes next in series was written by the same author...)
                // Returns the first one found, though there should probably not be more than 1 of the same name anyway
                nextBook = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, $@"^{book.title}(?: \(.*\))?$"));
                if (nextBook == null)
                {
                    // Attempt to search Amazon for the book instead
                    // TODO: This should be elsewhere
                    try
                    {
                        if (!string.IsNullOrEmpty(book.asin))
                        {
                            nextBook = book;
                            await nextBook.GetAmazonInfo($"https://www.amazon.{TLD}/dp/{book.asin}");
                        }
                        else
                            nextBook = await Amazon.Amazon.SearchBook(book.title, book.author, TLD);

                        if (nextBook == null && settings.promptASIN)
                        {
                            Logger.Log($"ASIN prompt for {book.title}...");
                            nextBook = new BookInfo(book.title, book.author, "");
                            frmAS.Text = "Next in Series";
                            frmAS.lblTitle.Text = book.title;
                            frmAS.tbAsin.Text = "";
                            frmAS.ShowDialog();
                            Logger.Log($"ASIN supplied: {frmAS.tbAsin.Text}");
                            string Url = $"https://www.amazon.{TLD}/dp/{frmAS.tbAsin.Text}";
                            await nextBook.GetAmazonInfo(Url);
                            nextBook.amazonUrl = Url;
                            nextBook.asin = frmAS.tbAsin.Text;
                        }
                    }
                    catch
                    {
                        Logger.Log($"Failed to find {book.title} on Amazon.{TLD}, trying again with Amazon.com.");
                        TLD = "com";
                        nextBook = await Amazon.Amazon.SearchBook(book.title, book.author, TLD);
                    }

                    if (nextBook != null)
                        await nextBook.GetAmazonInfo(nextBook.amazonUrl); //fill in desc, imageurl, and ratings
                }

                if (nextBook == null)
                {
                    Logger.Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n"
                        + "Please report this book and the Goodreads URL and output log to improve parsing (if it's a real book).");
                }
            }
            else if (curBook.totalInSeries == 0)
                Logger.Log("The book was not found to be part of a series.");
            else if (curBook.seriesPosition != curBook.totalInSeries.ToString() && !curBook.seriesPosition?.Contains(".") == true)
                Logger.Log("An error occurred finding the next book in series. The book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out book))
            {
                var prevBook = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, $@"^{book.title}(?: \(.*\))?$"));
                if (book.asin != null)
                {
                    prevBook = book;
                    await prevBook.GetAmazonInfo($"https://www.amazon.{TLD}/dp/{book.asin}");
                }
                else if(prevBook != null)
                    await prevBook.GetAmazonInfo(prevBook.amazonUrl);
                if (prevBook == null && settings.promptASIN)
                {
                    Logger.Log($"ASIN prompt for {book.title}...");
                    prevBook = new BookInfo(book.title, book.author, "");
                    frmAS.Text = "Previous in Series";
                    frmAS.lblTitle.Text = book.title;
                    frmAS.tbAsin.Text = "";
                    frmAS.ShowDialog();
                    Logger.Log($"ASIN supplied: {frmAS.tbAsin.Text}");
                    string Url = $"https://www.amazon.{TLD}/dp/{frmAS.tbAsin.Text}";
                    await prevBook.GetAmazonInfo(Url);
                    prevBook.amazonUrl = Url;
                    prevBook.asin = frmAS.tbAsin.Text;
                }
                if (prevBook == null)
                {
                    Logger.Log("Book was found to be part of a series, but an error occurred finding the previous book.\r\n" +
                        "Please report this book and the Goodreads URL and output log to improve parsing.");
                }
            }
            return nextBook;
        }

        /// <summary>
        /// Search Goodread for possible series info, returning the next title in the series.
        /// Modifies curBook.
        /// </summary>
        public async Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            var series = new SeriesInfo();
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(dataUrl));
            }

            //Search Goodreads for series info
            HtmlNode metaNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='metacol']");
            HtmlNode seriesNode = metaNode?.SelectSingleNode("//h2[@id='bookSeries']/a");
            if (seriesNode == null)
                return null;
            var match = Regex.Match(seriesNode.OuterHtml, @"/series/([0-9]*)");
            if (!match.Success)
                return null;
            series.Url = $"https://www.goodreads.com/series/{match.Groups[1].Value}";
            match = Regex.Match(seriesNode.InnerText, @"\((.*) #?([0-9]*([.,][0-9])?)\)");
            if (!match.Success)
                return null;

            Logger.Log($"Series Goodreads Page URL: {series.Url}");
            series.Name = match.Groups[1].Value.Trim();
            series.Position = match.Groups[2].Value;


            var seriesHtmlDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
            seriesHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(series.Url));

            seriesNode = seriesHtmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'responsiveSeriesHeader__subtitle')]");
            match = Regex.Match(seriesNode?.InnerText ?? "", @"([0-9]+) (?:primary )?works?");
            if (match.Success)
                series.Total = int.Parse(match.Groups[1].Value);

            int positionInt = (int)Convert.ToDouble(series.Position, CultureInfo.InvariantCulture.NumberFormat);
            int totalInt = (int)Convert.ToDouble(series.Total, CultureInfo.InvariantCulture.NumberFormat);

            Logger.Log($"This is book {series.Position} of {series.Total} in the {series.Name} series");

            var bookNodes = seriesHtmlDoc.DocumentNode.SelectNodes("//div[@itemtype='http://schema.org/Book']");
            if (bookNodes == null)
                return series;
            var prevSearch = series.Position.Contains(".")
                ? $"book {positionInt}"
                : $"book {positionInt - 1}";
            var nextSearch = $"book {positionInt + 1}";

            async Task<BookInfo> ParseSeriesBook(HtmlNode bookNode)
            {
                BookInfo book = new BookInfo("", "", "");
                var title = bookNode.SelectSingleNode(".//div[@class='u-paddingBottomXSmall']/a");
                book.title = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", "", RegexOptions.Compiled);
                book.goodreadsID = ParseBookId(title.GetAttributeValue("href", ""));
                // TODO: move this ASIN search somewhere else
                if (!string.IsNullOrEmpty(book.goodreadsID))
                    book.asin = await SearchBookASIN(book.goodreadsID, book.title, cancellationToken).ConfigureAwait(false);
                book.author = bookNode.SelectSingleNode(".//span[@itemprop='author']//a")?.InnerText.Trim() ?? "";
                return book;
            }

            foreach (var bookNode in bookNodes)
            {
                var bookIndex = bookNode.SelectSingleNode(".//div[@class='responsiveBook__header']")?.InnerText.ToLower();
                if (bookIndex == null) continue;
                if (bookIndex == prevSearch && series.Previous == null)
                {
                    series.Previous = await ParseSeriesBook(bookNode);
                    Logger.Log($"Preceded by: {series.Previous.title}");
                }
                else if (bookIndex == nextSearch)
                {
                    series.Next = await ParseSeriesBook(bookNode);
                    Logger.Log($"Followed by: {series.Next.title}");
                }
                if (series.Previous != null && (series.Next != null || positionInt == totalInt))
                    break; // next and prev found or prev found and latest in series
            }

            return series;
        }

        // Search Goodreads for possible kindle edition of book and return ASIN.
        public async Task<string> SearchBookASIN(string id, string title, CancellationToken cancellationToken = default)
        {
            string goodreadsBookUrl = String.Format("https://www.goodreads.com/book/show/{0}", id);
            try
            {
                HtmlDocument bookHtmlDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
                bookHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(BookUrl(id)));
                HtmlNode link = bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='otherEditionsActions']/a");
                Match match = Regex.Match(link.GetAttributeValue("href", ""), @"editions/([0-9]*)-");
                if (match.Success)
                {
                    string kindleEditionsUrl = String.Format("https://www.goodreads.com/work/editions/{0}?utf8=%E2%9C%93&sort=num_ratings&filter_by_format=Kindle+Edition", match.Groups[1].Value);
                    bookHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(kindleEditionsUrl));
                    HtmlNodeCollection bookNodes = bookHtmlDoc.DocumentNode.SelectNodes("//div[@class='elementList clearFix']");
                    if (bookNodes != null)
                    {
                        foreach (HtmlNode book in bookNodes)
                        {
                            match = Regex.Match(book.InnerHtml, "(B[A-Z0-9]{9})");
                            if (match.Success)
                                return match.Value;
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                Logger.Log(String.Format("An error occurred while searching for {0}s ASIN.\r\n", title) + ex.Message + "\r\n" + ex.StackTrace);
                return "";
            }
        }

        public async Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }
            HtmlNode pagesNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='details']");
            if (pagesNode == null)
                return false;
            Match match = Regex.Match(pagesNode.InnerText, @"((\d+)|(\d+,\d+)) pages");
            if (match.Success)
            {
                double minutes = int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                TimeSpan span = TimeSpan.FromMinutes(minutes);
                // Functions.Pluralize($"{BookList[i].editions:edition}")
                Logger.Log(String.Format("Typical time to read: {0}, {1}, and {2} ({3} pages)",
                    Functions.Pluralize($"{span.Days:day}"),
                    Functions.Pluralize($"{span.Hours:hour}"),
                    Functions.Pluralize($"{span.Minutes:minute}"),
                    match.Groups[1].Value));
                curBook.pagesInBook = int.Parse(match.Groups[1].Value);
                curBook.readingHours = span.Hours;
                curBook.readingMinutes = span.Minutes;
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<XRay.Term>> GetTermsAsync(string dataUrl, IProgressBar progress, CancellationToken cancellationToken)
        {
            if (sourceHtmlDoc == null)
            {
                Logger.Log("Downloading Goodreads page...");
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(dataUrl));
            }

            var charNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/a");
            if (charNodes == null) return new List<XRay.Term>();
            // Check if ...more link exists on Goodreads page
            var moreCharNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/span[@class='toggleContent']/a");
            var allChars = moreCharNodes == null ? charNodes : charNodes.Concat(moreCharNodes);
            var termCount = moreCharNodes == null ? charNodes.Count : charNodes.Count + moreCharNodes.Count;
            Logger.Log($"Gathering term information from Goodreads... ({termCount})");
            progress?.Set(0, termCount);
            if (termCount > 20)
                Logger.Log("More than 20 characters found. Consider using the 'download to XML' option if you need to build repeatedly.");
            var terms = new ConcurrentBag<XRay.Term>();
            await allChars.ParallelForEachAsync(async charNode =>
            {
                try
                {
                    terms.AddNotNull(await GetTermAsync(dataUrl, charNode.GetAttributeValue("href", "")).ConfigureAwait(false));
                    progress?.Add(1);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(404)"))
                        Logger.Log("Error getting page for character. URL: " + "https://www.goodreads.com" + charNode.GetAttributeValue("href", "")
                            + "\r\nMessage: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }, MaxConcurrentRequests, cancellationToken);
            return terms.ToList();
        }

        // Are there actually any goodreads pages that aren't at goodreads.com for other languages??
        private async Task<XRay.Term> GetTermAsync(string baseUrl, string relativeUrl, CancellationToken cancellationToken = default)
        {
            XRay.Term result = new XRay.Term("character");
            Uri tempUri = new Uri(baseUrl);
            tempUri = new Uri(new Uri(tempUri.GetLeftPart(UriPartial.Authority)), relativeUrl);
            result.DescSrc = "Goodreads";
            result.DescUrl = tempUri.ToString();
            HtmlDocument charDoc = new HtmlDocument();
            charDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(tempUri.ToString()));
            HtmlNode mainNode = charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat']")
                ?? charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat ']");
            result.TermName = mainNode.SelectSingleNode("./h1").InnerText;
            mainNode = mainNode.SelectSingleNode("//div[@class='grey500BoxContent']");
            HtmlNodeCollection tempNodes = mainNode.SelectNodes("//div[@class='floatingBox']");
            if (tempNodes == null) return result;
            foreach (HtmlNode tempNode in tempNodes)
            {
                if (tempNode.Id.Contains("_aliases")) // If present, add any aliases found
                {
                    string aliasStr = tempNode.InnerText.Replace("[close]", "").Trim();
                    result.Aliases.AddRange(aliasStr.Split(new [] { ", " }, StringSplitOptions.RemoveEmptyEntries));
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
                srcDoc = new HtmlDocument();
                srcDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(url));
            }
            HtmlNode quoteNode = srcDoc.DocumentNode.SelectSingleNode("//div[@class='h2Container gradientHeaderContainer']/h2/a[starts-with(.,'Quotes from')]");
            if (quoteNode == null) return null;
            string quoteURL = $"https://www.goodreads.com{quoteNode.GetAttributeValue("href", "")}?page={{0}}";
            progress?.Set(0, 1);

            var quoteBag = new ConcurrentBag<IEnumerable<NotableClip>>();
            var initialPage = new HtmlDocument();
            initialPage.LoadHtml(await HttpDownloader.GetPageHtmlAsync(string.Format(quoteURL, 1)));

            // check how many pages there are (find previous page button, get parent div, take all children of that, 2nd last one should be the max page count
            HtmlNode maxPageNode = initialPage.DocumentNode.SelectSingleNode("//span[contains(@class,'previous_page')]/parent::div/*[last()-1]");
            if (maxPageNode == null) return null;
            if (!int.TryParse(maxPageNode.InnerHtml, out var maxPages))
                maxPages = 1;

            IEnumerable<NotableClip> ParseQuotePage(HtmlDocument quoteDoc)
            {
                HtmlNodeCollection tempNodes = quoteDoc.DocumentNode.SelectNodes("//div[@class='quotes']/div[@class='quote']");
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
            await Enumerable.Range(2, maxPages).ParallelForEachAsync(async page =>
            {
                var quotePage = new HtmlDocument();
                quotePage.LoadHtml(await HttpDownloader.GetPageHtmlAsync(string.Format(quoteURL, page)));
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
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }

            if (curBook.notableClips == null)
            {
                curBook.notableClips = (await GetNotableClipsAsync("", sourceHtmlDoc, progress, cancellationToken).ConfigureAwait(false)).ToList();
            }

            //Add rating and reviews count if missing from Amazon book info
            HtmlNode metaNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
            if (metaNode != null && curBook.amazonRating == 0)
            {
                HtmlNode goodreadsRating = metaNode.SelectSingleNode("//span[@class='value rating']");
                if (goodreadsRating != null)
                    curBook.amazonRating = float.Parse(goodreadsRating.InnerText);
                HtmlNode passagesNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']")
                    ?? metaNode.SelectSingleNode(".//span[@class='count value-title']");
                if (passagesNode != null)
                {
                    Match match = Regex.Match(passagesNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                    if (match.Success)
                        curBook.numReviews = int.Parse(match.Value.Replace(",", "").Replace(".", ""));
                }
            }
        }
    }
}
