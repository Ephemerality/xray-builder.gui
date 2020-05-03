using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilder.Core.Extras.EndActions
{
    public sealed class EndActionsDataGenerator
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;
        private readonly IAmazonInfoParser _amazonInfoParser;

        private List<BookInfo> custAlsoBought = new List<BookInfo>();
        private BookInfo curBook;

        private readonly ISecondarySource _dataSource;
        private readonly Settings _settings;

        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        // TODO Move non-DI params from constructor to function
        public EndActionsDataGenerator(
            BookInfo book,
            ISecondarySource dataSource,
            Settings settings,
            ILogger logger,
            IHttpClient httpClient,
            IAmazonClient amazonClient,
            IAmazonInfoParser amazonInfoParser)
        {
            curBook = book;
            _dataSource = dataSource;
            _settings = settings;
            _logger = logger;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
            _amazonInfoParser = amazonInfoParser;
        }

        /// <summary>
        /// Generate the necessities for the old format
        /// TODO Remove anything that gets generated for the new version
        /// </summary>
        public async Task<Response> GenerateOld(CancellationToken cancellationToken = default)
        {
            _logger.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            var ebookLocation = string.Format(@"https://www.amazon.{0}/dp/{1}", _settings.AmazonTld, curBook.Asin);

            // Search Amazon for book
            //_logger.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));

            HtmlDocument bookHtmlDoc;
            try
            {
                bookHtmlDoc = await _httpClient.GetPageAsync(ebookLocation, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("An error ocurred while downloading book's Amazon page: {0}\r\nYour ASIN may not be correct.", ex.Message));
                return null;
            }
            _logger.Log("Book found on Amazon!");
            if (_settings.SaveHtml)
            {
                try
                {
                    _logger.Log("Saving book's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory +
                                      string.Format(@"\dmp\{0}.bookpageHtml.txt", curBook.Asin),
                        bookHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("An error ocurred saving bookpageHtml.txt: {0}", ex.Message));
                }
            }

            try
            {
                var response = _amazonInfoParser.ParseAmazonDocument(bookHtmlDoc);
                response.ApplyToBookInfo(curBook);
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("An error ocurred parsing Amazon info: {0}", ex.Message));
                return null;
            }

            _logger.Log("Gathering recommended book metadata...");
            //Parse Recommended Author titles and ASINs
            try
            {
                var recList = bookHtmlDoc.DocumentNode.SelectNodes("//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card a-float-left']");
                if (recList != null)
                {
                    var possibleBooks = new List<BookInfo>();
                    foreach (var item in recList.Where(item => item != null))
                    {
                        var nodeTitle = item.SelectSingleNode(".//div/a");
                        var nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                        var nodeUrl = nodeTitle.GetAttributeValue("href", "");
                        if (nodeTitleCheck == "")
                        {
                            nodeTitle = item.SelectSingleNode(".//div/a");
                            //Remove CR, LF and TAB
                            nodeTitleCheck = nodeTitle.InnerText.Clean();
                        }
                        //Check for duplicate by title
                        if (possibleBooks.Any(bk => bk.Title.Contains(nodeTitleCheck)))
                            continue;

                        var cleanAuthor = item.SelectSingleNode(".//div/div").InnerText.Clean();
                        //Exclude the current book title from other books search
                        var match = Regex.Match(nodeTitleCheck, curBook.Title, RegexOptions.IgnoreCase);
                        if (match.Success)
                            continue;
                        match = Regex.Match(nodeTitleCheck,
                            @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)",
                            RegexOptions.IgnoreCase);
                        if (match.Success)
                            continue;
                        possibleBooks.Add(new BookInfo(nodeTitleCheck, cleanAuthor,
                            _amazonClient.ParseAsin(nodeUrl)));
                    }

                    if (_settings.UseNewVersion)
                    {
                        await foreach (var _ in _amazonClient.EnhanceBookInfos(possibleBooks, cancellationToken))
                        {
                            // todo progress
                        }
                    }

                    custAlsoBought.AddRange(possibleBooks);
                }
                //Add sponsored related, if they exist...
                var otherItems =
                    bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='view_to_purchase-sims-feature']");
                if (otherItems != null)
                {
                    recList = otherItems.SelectNodes(".//li[@class='a-spacing-medium p13n-sc-list-item']");
                    if (recList != null)
                    {
                        var possibleBooks = new List<BookInfo>();
                        // TODO: This entire foreach is pretty much the exact same as the one above...
                        foreach (var result in recList.Where(result => result != null))
                        {
                            var otherBook =
                                result.SelectSingleNode(".//div[@class='a-fixed-left-grid-col a-col-left']/a");
                            if (otherBook == null)
                                continue;
                            var sponsAsin = _amazonClient.ParseAsinFromUrl(otherBook.GetAttributeValue("href", ""));
                            if (sponsAsin == null)
                                continue;

                            otherBook = otherBook.SelectSingleNode(".//img");
                            var match = Regex.Match(otherBook.GetAttributeValue("alt", ""),
                                @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)",
                                RegexOptions.IgnoreCase);
                            if (match.Success)
                                continue;
                            var sponsTitle = otherBook.GetAttributeValue("alt", "");
                            //Check for duplicate by title
                            if (custAlsoBought.Any(bk => bk.Title.Contains(sponsTitle)) || possibleBooks.Any(bk => bk.Title.Contains(sponsTitle)))
                                continue;
                            otherBook = result.SelectSingleNode(".//a[@class='a-size-small a-link-child']")
                                ?? result.SelectSingleNode(".//span[@class='a-size-small a-color-base']")
                                ?? throw new FormatChangedException("Amazon", "Sponsored book author");
                            // TODO: Throw more format changed exceptions to make it obvious that the site changed
                            var sponsAuthor = otherBook.InnerText.Trim();
                            possibleBooks.Add(new BookInfo(sponsTitle, sponsAuthor, sponsAsin));
                        }

                        await foreach (var _ in _amazonClient.EnhanceBookInfos(possibleBooks, cancellationToken))
                        {
                            // todo progress
                        }

                        custAlsoBought.AddRange(possibleBooks);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred parsing the book's amazon page: " + ex.Message + ex.StackTrace);
                return null;
            }

            return new Response
            {
                Book = curBook,
                CustomerAlsoBought = custAlsoBought.ToArray()
            };
        }

        private async Task<BookInfo> SearchOrPrompt(BookInfo book, Func<string, string, string> asinPrompt, CancellationToken cancellationToken = default)
        {
            // If the asin was available from another source, use it
            if (!string.IsNullOrEmpty(book.Asin))
            {
                var response = await _amazonInfoParser.GetAndParseAmazonDocument($"https://www.amazon.{_settings.AmazonTld}/dp/{book.Asin}", cancellationToken);
                response.ApplyToBookInfo(book);

                return book;
            }

            BookInfo newBook;
            try
            {

                newBook = await _amazonClient.SearchBook(book.Title, book.Author, _settings.AmazonTld, cancellationToken);
                if (newBook == null && _settings.PromptAsin && asinPrompt != null)
                {
                    _logger.Log($"ASIN prompt for {book.Title}...");
                    var asin = asinPrompt(book.Title, book.Author);
                    if (string.IsNullOrWhiteSpace(asin))
                        return null;
                    _logger.Log($"ASIN supplied: {asin}");
                    newBook = new BookInfo(book.Title, book.Author, asin);
                }
            }
            catch
            {
                _logger.Log($"Failed to find {book.Title} on Amazon.{_settings.AmazonTld}, trying again with Amazon.com.");
                newBook = await _amazonClient.SearchBook(book.Title, book.Author, "com", cancellationToken);
            }

            if (newBook != null)
            {
                var response = await _amazonInfoParser.GetAndParseAmazonDocument(newBook.AmazonUrl, cancellationToken);
                response.ApplyToBookInfo(newBook);
            }

            return newBook;
        }

        private async Task ExpandSeriesMetadata(AuthorProfileGenerator.Response authorProfile, SeriesInfo series, Func<string, string, string> asinPrompt, CancellationToken cancellationToken = default)
        {
            // Search author's other books for the book (assumes next in series was written by the same author...)
            // Returns the first one found, though there should probably not be more than 1 of the same name anyway
            // If not found there, try to get it using the asin from Goodreads or by searching Amazon
            // Swaps out the basic next/previous from Goodreads w/ full Amazon ones
            async Task<BookInfo> FromApOrSearch(BookInfo book, CancellationToken ct)
            {
                return authorProfile.OtherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.Title, $@"^{book.Title}(?: \(.*\))?$"))
                    ?? await SearchOrPrompt(book, asinPrompt, ct);
            }

            // TODO: Don't juggle around bookinfos
            if (series.Next != null)
                series.Next = await FromApOrSearch(series.Next, cancellationToken);
            if (series.Previous != null)
                series.Previous = await FromApOrSearch(series.Previous, cancellationToken);

            if (series.Next == null)
            {
                _logger.Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n"
                    + "Please report this book and the Goodreads URL and output log to improve parsing (if it's a real book).");
            }
        }

        /// <summary>
        /// Generate necessities for the new format (which includes running <see cref="GenerateOld"/> automatically)
        /// </summary>
        [ItemCanBeNull]
        public async Task<Response> GenerateNewFormatData(
            AuthorProfileGenerator.Response authorProfile,
            Func<string, string, string> asinPrompt,
            IMetadata metadata,
            IProgressBar progress,
            CancellationToken cancellationToken = default)
        {
            // Generate old stuff first, ignore response since curBook and custAlsoBought are shared
            // todo make them not shared
            if (await GenerateOld(cancellationToken) == null)
                return null;

            try
            {
                await _dataSource.GetExtrasAsync(curBook, progress, cancellationToken);
                curBook.Series = await _dataSource.GetSeriesInfoAsync(curBook.DataUrl, cancellationToken);

                if (curBook.Series == null || curBook.Series.Total == 0)
                    _logger.Log("The book was not found to be part of a series.");
                else if (curBook.Series.Next == null && curBook.Series.Position != curBook.Series.Total.ToString())// && !curBook.Series.Position?.Contains(".") == true)
                    _logger.Log("An error occurred finding the next book in series. The book may not be part of a series, or it is the latest release.");
                else
                    await ExpandSeriesMetadata(authorProfile, curBook.Series, asinPrompt, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))
                    _logger.Log("An error occurred finding next book in series: Goodreads URL not found.\r\n" +
                               "If reading from a file, you can switch the source to Goodreads to specify a URL, then switch back to File.");
                else
                    _logger.Log("An error occurred finding next book in series: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }

            // TODO: Refactor next/previous series stuff
            if (curBook.Series?.Next == null)
            {
                try
                {
                    var seriesResult = await _amazonClient.DownloadNextInSeries(curBook.Asin, cancellationToken);
                    switch (seriesResult?.Error?.ErrorCode)
                    {
                        case "ERR004":
                            _logger.Log("According to Amazon, this book is not part of a series.");
                            break;
                        case "ERR000":
                            if (curBook.Series == null)
                                curBook.Series = new SeriesInfo();
                            curBook.Series.Next =
                                new BookInfo(seriesResult.NextBook.Title.TitleName,
                                    Functions.FixAuthor(seriesResult.NextBook.Authors.FirstOrDefault()?.AuthorName),
                                    seriesResult.NextBook.Asin);
                            var response = await _amazonInfoParser.GetAndParseAmazonDocument(curBook.Series.Next.AmazonUrl, cancellationToken);
                            response.ApplyToBookInfo(curBook.Series.Next);
                            break;
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            if (curBook.Series != null)
            {
                _logger.Log($"\nSeries URL: {curBook.Series.Url}");
                if (!string.IsNullOrEmpty(curBook.Series.Name))
                    _logger.Log($"This is book {curBook.Series.Position} of {curBook.Series.Total} in the {curBook.Series.Name} series");
                if (curBook.Series.Previous != null)
                    _logger.Log($"Preceded by: {curBook.Series.Previous.Title}");
                if (curBook.Series.Next != null)
                    _logger.Log($"Followed by: {curBook.Series.Next.Title}\n");
            }

            try
            {
                if (!await _dataSource.GetPageCountAsync(curBook, cancellationToken))
                {
                    var metadataCount = metadata.GetPageCount();
                    if (metadataCount.HasValue)
                        curBook.PagesInBook = metadataCount.Value;
                    else if (_settings.EstimatePageCount)
                    {
                        _logger.Log("No page count found on Goodreads or in metadata. Attempting to estimate page count...");
                        _logger.Log(Functions.GetPageCount(curBook.RawmlPath, curBook));
                    }
                    else
                        _logger.Log("No page count found on Goodreads or in metadata");
                }
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred while searching for or estimating the page count: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }

            return new Response
            {
                Book = curBook,
                CustomerAlsoBought = custAlsoBought.ToArray()
            };
        }

        public sealed class Response
        {
            public BookInfo Book { get; set; }
            public BookInfo[] CustomerAlsoBought { get; set; }
        }

        /// <summary>
        /// Settings required to generate an EndActions file
        /// TODO: Move to a JSON config
        /// </summary>
        public class Settings
        {
            public string AmazonTld { get; set; }
            public bool UseNewVersion { get; set; }
            public bool PromptAsin { get; set; }
            public bool EstimatePageCount { get; set; }
            public bool SaveHtml { get; set; }
        }
    }
}
