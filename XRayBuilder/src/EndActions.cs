using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using XRayBuilderGUI.DataSources;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Extras.Artifacts;
using XRayBuilderGUI.Extras.AuthorProfile;
using XRayBuilderGUI.Libraries.Enumerables.Extensions;
using XRayBuilderGUI.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI
{
    public class EndActions
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IAmazonClient _amazonClient;
        private readonly IAmazonInfoParser _amazonInfoParser;

        // TODO These paths shouldn't be here
        private string EaPath = "";
        public string SaPath = "";

        public List<BookInfo> custAlsoBought = new List<BookInfo>();

        public BookInfo curBook;
        private readonly AuthorProfileGenerator.Response _authorProfile;
        private readonly ISecondarySource _dataSource;
        private readonly long _erl;
        private readonly Settings _settings;
        private readonly Func<string, string, string> _asinPrompt;

        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        // TODO Move non-DI params from constructor to function
        public EndActions(
            AuthorProfileGenerator.Response authorProfile,
            BookInfo book,
            long erl,
            ISecondarySource dataSource,
            Settings settings,
            Func<string, string, string> asinPrompt,
            ILogger logger,
            IHttpClient httpClient,
            IAmazonClient amazonClient,
            IAmazonInfoParser amazonInfoParser)
        {
            _authorProfile = authorProfile;
            curBook = book;
            _erl = erl;
            _dataSource = dataSource;
            _settings = settings;
            _asinPrompt = asinPrompt;
            _logger = logger;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
            _amazonInfoParser = amazonInfoParser;
        }

        /// <summary>
        /// Generate the necessities for both old and new formats
        /// </summary>
        public async Task<bool> Generate(CancellationToken cancellationToken = default)
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
                return false;
            }
            _logger.Log("Book found on Amazon!");
            if (Properties.Settings.Default.saveHtml)
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
                return false;
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
                        await _amazonClient.EnhanceBookInfos(possibleBooks).ForEachAsync(book =>
                        {
                            // todo progress
                        }, cancellationToken);
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

                        await _amazonClient.EnhanceBookInfos(possibleBooks).ForEachAsync(book =>
                        {
                            // todo progress
                        }, cancellationToken);

                        custAlsoBought.AddRange(possibleBooks);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred parsing the book's amazon page: " + ex.Message + ex.StackTrace);
                return false;
            }
            SetPaths();
            return true;
        }

        public void GenerateOld()
        {
            //Create final EndActions.data.ASIN.asc
            var dt = DateTime.Now.ToString("s");
            var tz = DateTime.Now.ToString("zzz");
            var writer = new XmlTextWriter(EaPath, Encoding.UTF8);
            _logger.Log("Writing EndActions to file...");
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            writer.WriteStartElement("endaction");
            writer.WriteAttributeString("version", "0");
            writer.WriteAttributeString("guid", $"{curBook.Databasename}:{curBook.Guid}");
            writer.WriteAttributeString("key", curBook.Asin);
            writer.WriteAttributeString("type", "EBOK");
            writer.WriteAttributeString("timestamp", dt + tz);
            writer.WriteElementString("treatment", "d");
            writer.WriteStartElement("currentBook");
            writer.WriteElementString("imageUrl", curBook.ImageUrl);
            writer.WriteElementString("asin", curBook.Asin);
            writer.WriteElementString("hasSample", "false");
            writer.WriteEndElement();
            writer.WriteStartElement("customerProfile");
            writer.WriteElementString("penName", _settings.PenName);
            writer.WriteElementString("realName", _settings.RealName);
            writer.WriteEndElement();
            writer.WriteStartElement("recs");
            writer.WriteAttributeString("type", "author");
            for (var i = 0; i < Math.Min(_authorProfile.OtherBooks.Length, 5); i++)
            {
                writer.WriteStartElement("rec");
                writer.WriteAttributeString("hasSample", "false");
                writer.WriteAttributeString("asin", _authorProfile.OtherBooks[i].Asin);
                writer.WriteElementString("title", _authorProfile.OtherBooks[i].Title);
                writer.WriteElementString("author", curBook.Author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("recs");
            writer.WriteAttributeString("type", "purchase");
            for (var i = 0; i < Math.Min(custAlsoBought.Count, 5); i++)
            {
                writer.WriteStartElement("rec");
                writer.WriteAttributeString("hasSample", "false");
                writer.WriteAttributeString("asin", custAlsoBought[i].Asin);
                writer.WriteElementString("title", custAlsoBought[i].Title);
                writer.WriteElementString("author", custAlsoBought[i].Author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteElementString("booksMentionedPosition", "2");
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            _logger.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
        }

        private async Task<BookInfo> SearchOrPrompt(BookInfo book, CancellationToken cancellationToken = default)
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
                if (newBook == null && _settings.PromptAsin && _asinPrompt != null)
                {
                    _logger.Log($"ASIN prompt for {book.Title}...");
                    var asin = _asinPrompt(book.Title, book.Author);
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

        public async Task ExpandSeriesMetadata(SeriesInfo series, CancellationToken cancellationToken = default)
        {
            // Search author's other books for the book (assumes next in series was written by the same author...)
            // Returns the first one found, though there should probably not be more than 1 of the same name anyway
            // If not found there, try to get it using the asin from Goodreads or by searching Amazon
            // Swaps out the basic next/previous from Goodreads w/ full Amazon ones
            async Task<BookInfo> FromApOrSearch(BookInfo book, CancellationToken ct)
            {
                return _authorProfile.OtherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.Title, $@"^{book.Title}(?: \(.*\))?$"))
                    ?? await SearchOrPrompt(book, ct);
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

        public async Task GenerateNewFormatData(IProgressBar progress, CancellationToken token = default)
        {
            try
            {
                await _dataSource.GetExtrasAsync(curBook, progress, token);
                curBook.Series = await _dataSource.GetSeriesInfoAsync(curBook.DataUrl, token);

                if (curBook.Series == null || curBook.Series.Total == 0)
                    _logger.Log("The book was not found to be part of a series.");
                else if (curBook.Series.Next == null && curBook.Series.Position != curBook.Series.Total.ToString())// && !curBook.Series.Position?.Contains(".") == true)
                    _logger.Log("An error occurred finding the next book in series. The book may not be part of a series, or it is the latest release.");
                else
                    await ExpandSeriesMetadata(curBook.Series, token);
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
                    var seriesResult = await _amazonClient.DownloadNextInSeries(curBook.Asin, token);
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
                            var response = await _amazonInfoParser.GetAndParseAmazonDocument(curBook.Series.Next.AmazonUrl, token);
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
                if (!await _dataSource.GetPageCountAsync(curBook, token))
                {
                    if (!Properties.Settings.Default.pageCount)
                        _logger.Log("No page count found on Goodreads");
                    _logger.Log("Attempting to estimate page count...");
                    _logger.Log(Functions.GetPageCount(curBook.RawmlPath, curBook));
                }
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred while searching for or estimating the page count: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }
        }

        public async Task GenerateEndActionsFromBase(Extras.Artifacts.EndActions baseEndActions)
        {
            baseEndActions.BookInfo = new Extras.Artifacts.EndActions.BookInformation
            {
                Asin = curBook.Asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAATAAB",
                ImageUrl = curBook.ImageUrl,
                EmbeddedID = $"{curBook.Databasename}:{curBook.Guid}",
                Erl = _erl
            };
            baseEndActions.Data.FollowSubscriptions = new Extras.Artifacts.EndActions.AuthorSubscriptions
            {
                Subscriptions = new[]
                {
                    new Subscription
                    {
                        Asin = curBook.AuthorAsin,
                        Name = curBook.Author,
                        ImageUrl = curBook.AuthorImageUrl
                    }
                }
            };
            baseEndActions.Data.AuthorSubscriptions = baseEndActions.Data.FollowSubscriptions;
            baseEndActions.Data.NextBook = Extensions.BookInfoToBook(curBook.Series?.Next, false);
            baseEndActions.Data.PublicSharedRating = new Extras.Artifacts.EndActions.Rating
            {
                Class = "publicSharedRating",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                Value = Math.Round(curBook.AmazonRating, 1)
            };
            baseEndActions.Data.CustomerProfile = new Extras.Artifacts.EndActions.CustomerProfile
            {
                PenName = _settings.PenName,
                RealName = _settings.RealName
            };
            baseEndActions.Data.Rating = baseEndActions.Data.PublicSharedRating;
            baseEndActions.Data.AuthorBios = new AuthorBios
            {
                Authors = new[]
                {
                    new Author
                    {
                        // TODO: Check mismatched fields from curbook and authorprofile
                        Asin = _authorProfile.Asin,
                        Name = curBook.Author,
                        Bio = _authorProfile.Biography,
                        ImageUrl = _authorProfile.ImageUrl
                    }
                }
            };
            baseEndActions.Data.AuthorRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = _authorProfile.OtherBooks.Select(bk => Extensions.BookInfoToBook(bk, true)).ToArray()
            };
            baseEndActions.Data.CustomersWhoBoughtRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = custAlsoBought.Select(bk => Extensions.BookInfoToBook(bk, true)).ToArray()
            };

            //string goodReads = String.Format(@"""goodReadsReview"":{{""class"":""goodReadsReview"",""reviewId"":""NoReviewId"",""rating"":{0},""submissionDateMs"":{1}}}", ratingText, dateMs);

            string finalOutput;
            try
            {
                finalOutput = Functions.ExpandUnicode(JsonConvert.SerializeObject(baseEndActions));
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred creating the EndAction data template: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }

            _logger.Log("Writing EndActions to file...");
            using (var streamWriter = new StreamWriter(EaPath, false))
            {
                await streamWriter.WriteAsync(finalOutput);
                streamWriter.Flush();
            }
            _logger.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
        }

        public string GenerateStartActionsFromBase(StartActions baseStartActions)
        {
            baseStartActions.BookInfo = new StartActions.BookInformation
            {
                Asin = curBook.Asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAAgAAA",
                ImageUrl = curBook.ImageUrl,
                Erl = -1
            };
            if (!string.IsNullOrEmpty(curBook.Series?.Position))
            {
                baseStartActions.Data.SeriesPosition = new StartActions.SeriesPosition
                {
                    PositionInSeries = Convert.ToInt32(double.Parse(curBook.Series.Position)),
                    TotalInSeries = curBook.Series.Total,
                    SeriesName = curBook.Series.Name
                };
            }
            baseStartActions.Data.FollowSubscriptions = new StartActions.AuthorSubscriptions
            {
                Subscriptions = new []
                {
                    new Subscription
                    {
                        Asin = curBook.AuthorAsin,
                        Name = curBook.Author,
                        ImageUrl = curBook.AuthorImageUrl
                    }
                }
            };
            baseStartActions.Data.AuthorSubscriptions = baseStartActions.Data.FollowSubscriptions;
            baseStartActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMPASSAGES%", $"{curBook.notableClips?.Count ?? 0}");
            baseStartActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMHIGHLIGHTS%", $"{curBook.notableClips?.Sum(c => c.Likes) ?? 0}");
            baseStartActions.Data.GrokShelfInfo.Asin = curBook.Asin;
            baseStartActions.Data.BookDescription = Extensions.BookInfoToBook(curBook, true);
            baseStartActions.Data.CurrentBook = baseStartActions.Data.BookDescription;
            baseStartActions.Data.AuthorBios = new AuthorBios
            {
                Authors = new []
                {
                    new Author
                    {
                        // TODO: Check mismatched fields from curbook and authorprofile
                        Asin = _authorProfile.Asin,
                        Name = curBook.Author,
                        Bio = _authorProfile.Biography,
                        ImageUrl = _authorProfile.ImageUrl
                    }
                }
            };
            baseStartActions.Data.AuthorRecs = new Recs
            {
                Class = "recommendationList",
                Recommendations = _authorProfile.OtherBooks.Select(bk => Extensions.BookInfoToBook(bk, false)).ToArray()
            };
            baseStartActions.Data.ReadingTime.Hours = curBook.ReadingHours;
            baseStartActions.Data.ReadingTime.Minutes = curBook.ReadingMinutes;
            baseStartActions.Data.ReadingTime.FormattedTime.Replace("%HOURS%", curBook.ReadingHours.ToString());
            baseStartActions.Data.ReadingTime.FormattedTime.Replace("%MINUTES%", curBook.ReadingMinutes.ToString());
            baseStartActions.Data.PreviousBookInTheSeries = Extensions.BookInfoToBook(curBook.Series?.Previous, true);
            baseStartActions.Data.ReadingPages.PagesInBook = curBook.PagesInBook;

            try
            {
                return Functions.ExpandUnicode(JsonConvert.SerializeObject(baseStartActions));
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred creating the StartActions template: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return null;
        }

        // todo output directory logic should be the same as frmmain uses
        private void SetPaths()
        {
            string outputDir;
            try
            {
                if (_settings.Android)
                {
                    outputDir = _settings.OutDir + @"\Android\" + curBook.Asin;
                    Directory.CreateDirectory(outputDir);
                }
                else
                    outputDir = _settings.UseSubDirectories
                        ? Functions.GetBookOutputDirectory(curBook.Author, curBook.Title, true)
                        : _settings.OutDir;
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred creating the output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = _settings.OutDir;
            }

            if (_settings.OutputToSidecar)
                outputDir = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(curBook.FileName)}.sdr");

            EaPath = outputDir + @"\EndActions.data." + curBook.Asin + ".asc";
            SaPath = outputDir + @"\StartActions.data." + curBook.Asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(EaPath))
            {
                _logger.Log("Error: EndActions file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
            }
        }

        /// <summary>
        /// Settings required to generate an EndActions file
        /// TODO: Move to a JSON config
        /// </summary>
        public class Settings
        {
            public string OutDir { get; set; }
            public bool OutputToSidecar { get; set; }
            public bool Android { get; set; }
            public string PenName { get; set; }
            public string RealName { get; set; }
            public string AmazonTld { get; set; }
            public bool UseNewVersion { get; set; }
            public bool UseSubDirectories { get; set; }
            public bool PromptAsin { get; set; }
        }
    }
}
