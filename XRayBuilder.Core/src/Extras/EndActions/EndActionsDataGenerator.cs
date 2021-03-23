using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Logic.PageCount;
using XRayBuilder.Core.Logic.ReadingTime;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Extras.EndActions
{
    public sealed class EndActionsDataGenerator : IEndActionsDataGenerator
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;
        private readonly IAmazonInfoParser _amazonInfoParser;
        private readonly IRoentgenClient _roentgenClient;
        private readonly IReadingTimeService _readingTimeService;
        private readonly IPageCountService _pageCountService;

        private readonly Regex _invalidBookTitleRegex = new Regex(@"(Series|Reading) Order|Complete Series|Checklist|Edition|eSpecial|Box ?Set|[0-9]+ Book Series|Books? \d+ ?(to|—|\\u2013|–) ?\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public EndActionsDataGenerator(
            ILogger logger,
            IHttpClient httpClient,
            IAmazonClient amazonClient,
            IAmazonInfoParser amazonInfoParser,
            IRoentgenClient roentgenClient,
            IReadingTimeService readingTimeService,
            IPageCountService pageCountService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
            _amazonInfoParser = amazonInfoParser;
            _roentgenClient = roentgenClient;
            _readingTimeService = readingTimeService;
            _pageCountService = pageCountService;
        }

        /// <summary>
        /// Generate the necessities for the old format
        /// TODO Remove anything that gets generated for the new version
        /// </summary>
        public async Task<Response> GenerateOld(BookInfo curBook, Settings settings, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            _logger.Log($"Attempting to find book on Amazon.{settings.AmazonTld}…");
            //Generate Book search URL from book's ASIN
            var ebookLocation = $@"https://www.amazon.{settings.AmazonTld}/dp/{curBook.Asin}";

            HtmlDocument bookHtmlDoc;
            try
            {
                bookHtmlDoc = await _httpClient.GetPageAsync(ebookLocation, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred while downloading book's Amazon page: {ex.Message}\r\nYour ASIN may not be correct.");
                return null;
            }
            _logger.Log("Book found on Amazon!");

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
                        _logger.Log($"Found description file, but it is empty!\r\n{file}");
                    if (!string.Equals(curBook.Description, fileText))
                        _logger.Log($"Using biography from {file}.");

                    return fileText;
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred while opening {file}\r\n{ex.Message}\r\n{ex.StackTrace}");
                }

                return null;
            }

            var descFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory, "ext", $"{curBook.Asin}.desc");
            if (settings.EditDescription)
            {
                if (!File.Exists(descFile) || new FileInfo(descFile).Length == 0)
#if NETFRAMEWORK
                    File.WriteAllText(descFile, curBook.Description);
#else
                    await File.WriteAllTextAsync(descFile, curBook.Description, cancellationToken);
#endif
                _logger.Log("Displaying book description for editing...");
                Functions.RunNotepad(descFile);
                curBook.Description = ReadDesc(descFile);
            }

            try
            {
                var listSelectors = new[]
                {
                    "//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card']",
                    "//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card a-float-left']",
                    "//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card aok-float-left']",
                    "//*[contains(@id, 'desktop-dp-sims_purchase-similarities-esp')]/li",
                    "//*[contains(@id, 'dp-sims_OnlineDpSimsPurchaseStrategy-sims')]/li",
                    "//*[@id='desktop-dp-sims_purchase-similarities-sims-feature']/li",
                    "//*[@id='desktop-dp-sims_vtp-60-sims-feature']/li",
                    "//div[@id='desktop-dp-sims_session-similarities-brand-protection-sims-feature']/li",
                    "//div[@id='desktop-dp-sims_session-similarities-sims-feature']/li",
                    "//*[@id='view_to_purchase-sims-feature']/li"
                };

                var relatedBooks = listSelectors.SelectMany(selector =>
                    {
                        var listNodes = bookHtmlDoc.DocumentNode.SelectNodes(selector);
                        return listNodes != null
                            ? ParseBookList(listNodes)
                            : Enumerable.Empty<BookInfo>();
                    }).Where(list => list != null)
                    .Distinct()
                    .Where(book => !book.Title.ToLower().Contains(curBook.Title.ToLower()) && book.Asin != curBook.Asin && !_invalidBookTitleRegex.IsMatch(curBook.Title))
                    .ToArray();

                if (settings.UseNewVersion && relatedBooks.Any())
                {
                    _logger.Log($"Gathering metadata for {relatedBooks.Length} related book(s)…");
                    progress?.Set(0, relatedBooks.Length);
                    await foreach (var _ in _amazonClient.EnhanceBookInfos(relatedBooks, cancellationToken))
                        progress?.Add(1);
                }

                return new Response
                {
                    Book = curBook,
                    CustomerAlsoBought = relatedBooks
                };
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred parsing the book's amazon page: {ex.Message}{ex.StackTrace}");
                return null;
            }
        }

        private IEnumerable<BookInfo> ParseBookList(HtmlNodeCollection books)
        {
            foreach (var book in books.Where(item => item != null))
            {
                string author = null;
                string title = null;

                var asinNode = book.SelectSingleNode(".//comment()[contains(., 'Title')]/following-sibling::a");
                if (asinNode != null)
                {
                    title = HtmlEntity.DeEntitize(asinNode.InnerText).Clean();
                    var aNode = book.SelectSingleNode(".//div[@class='a-row']/a");
                    if (aNode != null)
                        author = HtmlEntity.DeEntitize(aNode.InnerText.Clean());
                }
                else
                {
                    asinNode = book.SelectSingleNode(".//a[@class='a-link-normal']/div/span[@class='a-color-secondary series-book-number']")
                               ?? book.SelectSingleNode(".//a[@class='a-link-normal']/div[@class='a-section a-spacing-mini']/img");
                    if (asinNode != null)
                    {
                        title = HtmlEntity.DeEntitize(asinNode.Attributes["alt"]?.Value).Clean();
                        if (string.IsNullOrEmpty(title))
                            asinNode = book.SelectSingleNode(".//a[@class='a-link-normal'][2]");
                    }
                    asinNode ??= book.SelectSingleNode(".//a[@class='a-link-normal' and @title]");
                    if (asinNode == null)
                        continue;

                    if (asinNode.InnerText.IndexOf("Just released", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        var titleNode = book.SelectSingleNode(".//a[@class='a-link-normal' and @title]");
                        if (titleNode != null)
                            title = HtmlEntity.DeEntitize(titleNode.GetAttributeValue("title", "")).Clean();
                    }
                    if (string.IsNullOrEmpty(title))
                        title = HtmlEntity.DeEntitize(asinNode.InnerText).Clean();

                    var authorNode = book.SelectSingleNode(".//a[@class='a-size-small a-link-child']")
                                     ??
                                     book.SelectSingleNode(".//div[@class='a-row a-size-small sp-dp-ellipsis sp-dp-author']/a/span");
                    if (authorNode == null)
                        continue;
                    author = authorNode.InnerText.Clean().ToTitleCase();
                }

                var asin = _amazonClient.ParseAsinFromUrl(asinNode.GetAttributeValue("href", ""))
                           ?? _amazonClient.ParseAsinFromUrl(book.InnerHtml);

                if (string.IsNullOrEmpty(author) || string.IsNullOrEmpty(asin) || string.IsNullOrEmpty(title) || _invalidBookTitleRegex.IsMatch(title))
                    continue;

                yield return new BookInfo(title, author, asin);
            }
        }

        private async Task<BookInfo> SearchOrPrompt(BookInfo book, Func<string, string, string> asinPrompt, Settings settings, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(book.Asin))
            {
                var response = await _amazonInfoParser.GetAndParseAmazonDocument($"https://www.amazon.{settings.AmazonTld}/dp/{book.Asin}", cancellationToken);
                if (string.IsNullOrEmpty(response.Description))
                {
                    var localAmazonBook = await _amazonClient.SearchBook(book.Title, book.Author, settings.AmazonTld,
                        cancellationToken);
                    if (localAmazonBook != null)
                    {
                        response = await _amazonInfoParser.GetAndParseAmazonDocument(localAmazonBook.AmazonUrl, cancellationToken);
                        response.ApplyToBookInfo(localAmazonBook);
                        localAmazonBook.Title = book.Title;
                        return localAmazonBook;
                    }
                    response = await _amazonInfoParser.GetAndParseAmazonDocument($"https://www.amazon.com/dp/{book.Asin}", cancellationToken);
                }
                response.ApplyToBookInfo(book);

                return book;
            }

            BookInfo newBook;
            try
            {

                newBook = await _amazonClient.SearchBook(book.Title, book.Author, settings.AmazonTld, cancellationToken);
                if (newBook == null && settings.PromptAsin && asinPrompt != null)
                {
                    _logger.Log($"ASIN prompt for {book.Title}…");
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
            // Search author's other books for the book (assumes next in series was written by the same author…)
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

            if (series.Next == null && int.TryParse(series.Position, out var seriesPosition) && seriesPosition != series.Total)
            {
                _logger.Log($@"Book was found to be part of a series, but an error occurred finding the next book.{Environment.NewLine}Please report this book, the URL, and output log to improve parsing (if it's a real book).");
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
            var oldResponse = await GenerateOld(curBook, settings, progress, cancellationToken);
            if (oldResponse == null)
                return null;

            try
            {
                await dataSource.GetExtrasAsync(curBook, progress, cancellationToken);
                progress?.Set(0);
                _logger.Log("Checking if this book is part of a series…");
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
                    _logger.Log($"An error occurred finding next book in series: {ex.Message}\r\n{ex.StackTrace}");
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
                            curBook.Series ??= new SeriesInfo();
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
                {
                    _logger.Log((int) Convert.ToDouble(curBook.Series.Position) == curBook.Series.Total
                        ? $"This is the latest book in the {curBook.Series.Name} series."
                        : $"This is book {curBook.Series.Position} of {curBook.Series.Total} in the {curBook.Series.Name} series.");
                }
                if (curBook.Series.Previous != null)
                    _logger.Log($"Preceded by: {curBook.Series.Previous.Title}");
                if (curBook.Series.Next != null)
                    _logger.Log($"Followed by: {curBook.Series.Next.Title}");
            }

            // If the page count isn't known, attempt to locate or estimate it (if the setting is enabled)
            if (curBook.PageCount == 0)
            {
                try
                {
                    if (!await dataSource.GetPageCountAsync(curBook, cancellationToken))
                    {
                        var metadataCount = metadata.GetPageCount();
                        if (metadataCount.HasValue)
                            curBook.PageCount = metadataCount.Value;
                        else if (settings.EstimatePageCount)
                        {
                            _logger.Log($"No page count found on {dataSource.Name} or in metadata. An estimation will be made, if possible.");
                            curBook.PageCount = _pageCountService.EstimatePageCount(metadata);
                        }
                        else
                            _logger.Log($"No page count found on {dataSource.Name} or in metadata");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred while searching for or estimating the page count: {ex.Message}\r\n{ex.StackTrace}");
                    throw;
                }
            }

            if (curBook.PageCount != 0)
            {
                var readingTime = _readingTimeService.GetReadingTime(curBook.PageCount);
                curBook.ReadingHours = readingTime.Hours;
                curBook.ReadingMinutes = readingTime.Minutes;
                var readingTimeFromPageCount = _readingTimeService.GetFormattedReadingTime(curBook.PageCount);
                if (readingTimeFromPageCount != null)
                    _logger.Log(readingTimeFromPageCount);
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
