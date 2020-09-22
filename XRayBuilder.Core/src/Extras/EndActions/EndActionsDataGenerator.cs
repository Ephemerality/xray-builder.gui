using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
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
    public sealed class EndActionsDataGenerator : IEndActionsDataGenerator
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;
        private readonly IAmazonInfoParser _amazonInfoParser;
        private readonly IRoentgenClient _roentgenClient;

        public EndActionsDataGenerator(
            ILogger logger,
            IHttpClient httpClient,
            IAmazonClient amazonClient,
            IAmazonInfoParser amazonInfoParser,
            IRoentgenClient roentgenClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
            _amazonInfoParser = amazonInfoParser;
            _roentgenClient = roentgenClient;
        }

        /// <summary>
        /// Generate the necessities for the old format
        /// TODO Remove anything that gets generated for the new version
        /// </summary>
        public async Task<Response> GenerateOld(BookInfo curBook, Settings settings, CancellationToken cancellationToken = default)
        {
            var custAlsoBought = new List<BookInfo>();

            _logger.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            var ebookLocation = $@"https://www.amazon.{settings.AmazonTld}/dp/{curBook.Asin}";

            // Search Amazon for book
            //_logger.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));

            HtmlDocument bookHtmlDoc;
            try
            {
                bookHtmlDoc = await _httpClient.GetPageAsync(ebookLocation, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occured while downloading book's Amazon page: {ex.Message}\r\nYour ASIN may not be correct.");
                return null;
            }
            _logger.Log("Book found on Amazon!");
            if (settings.SaveHtml)
            {
                try
                {
                    _logger.Log("Saving book's Amazon webpage...");
                    var path = $"{AppDomain.CurrentDomain.BaseDirectory}dmp/{curBook.Asin}.bookpageHtml.txt";
#if NETCOREAPP3_1
                    await File.WriteAllTextAsync(path, bookHtmlDoc.DocumentNode.InnerHtml, cancellationToken);
#else
                    File.WriteAllText(path, bookHtmlDoc.DocumentNode.InnerHtml);
#endif
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred saving bookpageHtml.txt: {ex.Message}");
                }
            }

            try
            {
                var response = _amazonInfoParser.ParseAmazonDocument(bookHtmlDoc);
                response.ApplyToBookInfo(curBook);
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred parsing Amazon info: {ex.Message}");
                return null;
            }

            string ReadDesc(string file)
            {
                try
                {
                    var fileText = Functions.ReadFromFile(file);
                    if (string.IsNullOrEmpty(fileText))
                        _logger.Log("Found description file, but it is empty!\r\n" + file);
                    else
                        _logger.Log("Using description from " + file + ".");

                    return fileText;
                }
                catch (Exception ex)
                {
                    _logger.Log("An error occurred while opening " + file + "\r\n" + ex.Message + "\r\n" +
                               ex.StackTrace);
                }

                return null;
            }

            var descFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext", $"{curBook.Asin}.desc");
            if (settings.EditDescription)
            {
                if (!File.Exists(descFile) || File.ReadAllText(descFile).Length == 0)
                    File.WriteAllText(descFile, curBook.Description);
                _logger.Log("Displaying book description for editing...");
                Functions.RunNotepad(descFile);
                curBook.Description = ReadDesc(descFile);
            }

            _logger.Log("Parsing related books...");
            //Parse Recommended Author titles and ASINs
            custAlsoBought.Clear();
            try
            {
                var recList = bookHtmlDoc.DocumentNode.SelectNodes(
                    "//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card a-float-left']")
                    ??
                    bookHtmlDoc.DocumentNode.SelectNodes(
                        "//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card aok-float-left']");
                if (recList != null)
                {
                    custAlsoBought.AddRange(ParseBookList(recList, curBook));
                }

                //Add sponsored related, if they exist...
                var otherItems =
                    bookHtmlDoc.DocumentNode.SelectSingleNode(
                        "//*[contains(@id, 'desktop-dp-sims_purchase-similarities-esp')]")
                    ??
                    bookHtmlDoc.DocumentNode.SelectSingleNode(
                        "//*[@id='desktop-dp-sims_purchase-similarities-sims-feature']")
                    ??
                    bookHtmlDoc.DocumentNode.SelectSingleNode(
                        "//*[contains(@id, 'dp-sims_OnlineDpSimsPurchaseStrategy-sims')]");
                if (otherItems != null)
                {
                    recList = otherItems.SelectNodes(".//li");
                    if (recList != null)
                        custAlsoBought.AddRange(ParseBookList(recList, curBook));
                }

                otherItems = bookHtmlDoc.DocumentNode.SelectSingleNode("//*[@id='view_to_purchase-sims-feature']")
                             ??
                             bookHtmlDoc.DocumentNode.SelectSingleNode(
                                 "//*[@id='desktop-dp-sims_vtp-60-sims-feature']");
                if (otherItems != null)
                {
                    recList = otherItems.SelectNodes(".//li");
                    if (recList != null)
                        custAlsoBought.AddRange(ParseBookList(recList, curBook));
                }

                otherItems = bookHtmlDoc.DocumentNode.SelectSingleNode(
                                 "//div[@id='desktop-dp-sims_session-similarities-sims-feature']") ??
                             bookHtmlDoc.DocumentNode.SelectSingleNode(
                                 "//div[@id='desktop-dp-sims_session-similarities-brand-protection-sims-feature']");
                if (otherItems != null)
                {
                    recList = otherItems.SelectNodes(".//li");
                    if (recList != null)
                        custAlsoBought.AddRange(ParseBookList(recList, curBook));
                }

                if (custAlsoBought.Count != 0)
                {
                    _logger.Log(custAlsoBought.Count > 1
                        ? $"Gathering metadata for {custAlsoBought.Count} related books..."
                        : "Gathering metadata for a related book...");
                }

                await foreach (var _ in _amazonClient.EnhanceBookInfos(custAlsoBought, cancellationToken))
                {
                    // todo progress
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

        private List<BookInfo> ParseBookList(HtmlAgilityPack.HtmlNodeCollection books, BookInfo curBook)
        {
            string asin = string.Empty, author = string.Empty, title = string.Empty;
            var results = new List<BookInfo>();

            foreach (var book in books.Where(item => item != null))
            {
                var asinNode = book.SelectSingleNode(".//a[@class='a-link-normal']");
                if (asinNode != null)
                {
                    var parsedAsin = _amazonClient.ParseAsinFromUrl(asinNode.GetAttributeValue("href", ""));
                    if (string.IsNullOrEmpty(parsedAsin)) continue;
                    
                    // Check for duplicate by ASIN and ASIN does match current book's ASIN
                    if (parsedAsin == curBook.Asin ||
                        results.Any(bk => bk.Asin.Contains(parsedAsin)))
                        continue;

                    asin = parsedAsin;
                    title = HtmlAgilityPack.HtmlEntity.DeEntitize(asinNode.InnerText.Trim());

                    // Check title is not an eSpecial, reading order, series etc...
                    if (!Functions.IsActualBook(title, curBook.Title)) continue;
                }

                var authorNode = book.SelectSingleNode(".//a[@class='a-size-small a-link-child']");
                if (authorNode != null)
                    author = authorNode.InnerText.Clean();

                if (string.IsNullOrEmpty(author) && string.IsNullOrEmpty(asin)) continue;

                results.Add(new BookInfo(title, author, asin));
            }
            return results;
        }

        private async Task<BookInfo> SearchOrPrompt(BookInfo book, Func<string, string, string> asinPrompt, Settings settings, CancellationToken cancellationToken = default)
        {
            // If the asin was available from another source, use it
            if (!string.IsNullOrEmpty(book.Asin))
            {
                var response = await _amazonInfoParser.GetAndParseAmazonDocument($"https://www.amazon.{settings.AmazonTld}/dp/{book.Asin}", cancellationToken);
                response.ApplyToBookInfo(book);

                return book;
            }

            BookInfo newBook;
            try
            {

                newBook = await _amazonClient.SearchBook(book.Title, book.Author, settings.AmazonTld, cancellationToken);
                if (newBook == null && settings.PromptAsin && asinPrompt != null)
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
                _logger.Log($"Failed to find {book.Title} on Amazon.{settings.AmazonTld}, trying again with Amazon.com.");
                newBook = await _amazonClient.SearchBook(book.Title, book.Author, "com", cancellationToken);
            }

            if (newBook != null)
            {
                var response = await _amazonInfoParser.GetAndParseAmazonDocument(newBook.AmazonUrl, cancellationToken);
                response.ApplyToBookInfo(newBook);
            }

            return newBook;
        }

        private async Task ExpandSeriesMetadata(AuthorProfileGenerator.Response authorProfile, SeriesInfo series, Settings settings, Func<string, string, string> asinPrompt, CancellationToken cancellationToken = default)
        {
            // Search author's other books for the book (assumes next in series was written by the same author...)
            // Returns the first one found, though there should probably not be more than 1 of the same name anyway
            // If not found there, try to get it using the asin from Goodreads or by searching Amazon
            // Swaps out the basic next/previous from Goodreads w/ full Amazon ones
            async Task<BookInfo> FromApOrSearch(BookInfo book, CancellationToken ct)
            {
                return authorProfile.OtherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.Title, $@"^{book.Title}(?: \(.*\))?$"))
                    ?? authorProfile.OtherBooks.FirstOrDefault(bk => book.Asin != null && bk.Asin == book.Asin)
                    ?? await SearchOrPrompt(book, asinPrompt, settings, ct);
            }

            // TODO: Don't juggle around bookinfos
            if (series.Next != null)
                series.Next = await FromApOrSearch(series.Next, cancellationToken);
            if (series.Previous != null)
                series.Previous = await FromApOrSearch(series.Previous, cancellationToken);

            if (series.Next == null)
            {
                _logger.Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n"
                    + "Please report this book, the URL, and output log to improve parsing (if it's a real book).");
            }
        }

        /// <summary>
        /// Generate necessities for the new format (which includes running <see cref="GenerateOld"/> automatically)
        /// </summary>
        [ItemCanBeNull]
        public async Task<Response> GenerateNewFormatData(
            BookInfo curBook,
            Settings settings,
            ISecondarySource dataSource,
            AuthorProfileGenerator.Response authorProfile,
            Func<string, string, string> asinPrompt,
            IMetadata metadata,
            IProgressBar progress,
            CancellationToken cancellationToken = default)
        {
            // Generate old stuff first, ignore response since curBook and custAlsoBought are shared
            // todo make them not shared
            var oldResponse = await GenerateOld(curBook, settings, cancellationToken);
            if (oldResponse == null)
                return null;

            try
            {
                await dataSource.GetExtrasAsync(curBook, progress, cancellationToken);
                curBook.Series = await dataSource.GetSeriesInfoAsync(curBook.DataUrl, cancellationToken);

                if (curBook.Series == null || curBook.Series.Total == 0)
                    _logger.Log("The book was not found to be part of a series.");
                else if (curBook.Series.Next == null && curBook.Series.Position != curBook.Series.Total.ToString())// && !curBook.Series.Position?.Contains(".") == true)
                    _logger.Log("An error occurred finding the next book in series. The book may not be part of a series, or it is the latest release.");
                else
                    await ExpandSeriesMetadata(authorProfile, curBook.Series, settings, asinPrompt, cancellationToken);
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
                    var seriesResult = await _roentgenClient.DownloadNextInSeriesAsync(curBook.Asin, cancellationToken);
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
                _logger.Log($"Series URL: {curBook.Series.Url}");
                if (!string.IsNullOrEmpty(curBook.Series.Name))
                    _logger.Log($"This is book {curBook.Series.Position} of {curBook.Series.Total} in the {curBook.Series.Name} series");
                if (curBook.Series.Previous != null)
                    _logger.Log($"Preceded by: {curBook.Series.Previous.Title}");
                if (curBook.Series.Next != null)
                    _logger.Log($"Followed by: {curBook.Series.Next.Title}");
            }

            try
            {
                if (!await dataSource.GetPageCountAsync(curBook, cancellationToken))
                {
                    var metadataCount = metadata.GetPageCount();
                    if (metadataCount.HasValue)
                        curBook.PagesInBook = metadataCount.Value;
                    else if (settings.EstimatePageCount)
                    {
                        _logger.Log($"No page count found on {dataSource.Name} or in metadata. Attempting to estimate page count...");
                        _logger.Log(Functions.GetPageCount(curBook.RawmlPath, curBook));
                    }
                    else
                        _logger.Log($"No page count found on {dataSource.Name} or in metadata");
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
                CustomerAlsoBought = oldResponse.CustomerAlsoBought
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
            public bool EditDescription { get; set; }
        }
    }
}
