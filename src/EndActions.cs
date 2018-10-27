using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using Newtonsoft.Json;
using XRayBuilderGUI.DataSources;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI
{
    public class EndActions
    {
        private string EaPath = "";
        private string SaPath = "";

        public List<BookInfo> custAlsoBought = new List<BookInfo>();

        public BookInfo curBook;
        private readonly AuthorProfile _authorProfile;
        private readonly ISecondarySource _dataSource;
        private readonly long _erl;
        private readonly Settings _settings;
        private readonly Func<string, string, string> _asinPrompt;

        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        public EndActions(AuthorProfile authorProfile, BookInfo book, long erl, ISecondarySource dataSource, Settings settings, Func<string, string, string> asinPrompt)
        {
            _authorProfile = authorProfile;
            curBook = book;
            _erl = erl;
            _dataSource = dataSource;
            _settings = settings;
            _asinPrompt = asinPrompt;
        }

        /// <summary>
        /// Generate the necessities for both old and new formats
        /// </summary>
        public async Task<bool> Generate(CancellationToken cancellationToken = default)
        {
            Logger.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            string ebookLocation = string.Format(@"https://www.amazon.{0}/dp/{1}", _settings.AmazonTld, curBook.asin);

            // Search Amazon for book
            //Logger.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));

            HtmlDocument bookHtmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            try
            {
                bookHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(ebookLocation, cancellationToken));
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("An error ocurred while downloading book's Amazon page: {0}\r\nYour ASIN may not be correct.", ex.Message));
                return false;
            }
            Logger.Log("Book found on Amazon!");
            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    Logger.Log("Saving book's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory +
                                      string.Format(@"\dmp\{0}.bookpageHtml.txt", curBook.asin),
                        bookHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    Logger.Log(string.Format("An error ocurred saving bookpageHtml.txt: {0}", ex.Message));
                }
            }

            try
            {
                curBook.GetAmazonInfo(bookHtmlDoc);
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("An error ocurred parsing Amazon info: {0}", ex.Message));
                return false;
            }

            Logger.Log("Gathering recommended book metadata...");
            //Parse Recommended Author titles and ASINs
            try
            {
                var recList = bookHtmlDoc.DocumentNode.SelectNodes("//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card a-float-left']");
                if (recList != null)
                {
                    var possibleBooks = new List<BookInfo>();
                    foreach (HtmlNode item in recList.Where(item => item != null))
                    {
                        HtmlNode nodeTitle = item.SelectSingleNode(".//div/a");
                        var nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                        var nodeUrl = nodeTitle.GetAttributeValue("href", "");
                        if (nodeTitleCheck == "")
                        {
                            nodeTitle = item.SelectSingleNode(".//div/a");
                            //Remove CR, LF and TAB
                            nodeTitleCheck = nodeTitle.InnerText.Clean();
                        }
                        //Check for duplicate by title
                        if (possibleBooks.Any(bk => bk.title.Contains(nodeTitleCheck)))
                            continue;

                        var cleanAuthor = item.SelectSingleNode(".//div/div").InnerText.Clean();
                        //Exclude the current book title from other books search
                        Match match = Regex.Match(nodeTitleCheck, curBook.title, RegexOptions.IgnoreCase);
                        if (match.Success)
                            continue;
                        match = Regex.Match(nodeTitleCheck,
                            @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)",
                            RegexOptions.IgnoreCase);
                        if (match.Success)
                            continue;
                        possibleBooks.Add(new BookInfo(nodeTitleCheck, cleanAuthor,
                            Amazon.ParseAsin(nodeUrl)));
                    }
                    var bookBag = new ConcurrentBag<BookInfo>();
                    await possibleBooks.ParallelForEachAsync(async book =>
                    {
                        if (book == null) return;
                        // TODO: Make a separate function for this, duplicate here and AuthorProfile
                        try
                        {
                            //Gather book desc, image url, etc, if using new format
                            if (_settings.UseNewVersion)
                                await book.GetAmazonInfo(book.amazonUrl, cancellationToken);
                            bookBag.Add(book);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error: {ex}\r\n{book.amazonUrl}");
                        }
                    }, 5, cancellationToken);
                    custAlsoBought.AddRange(bookBag);
                }
                //Add sponsored related, if they exist...
                HtmlNode otherItems =
                    bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='view_to_purchase-sims-feature']");
                if (otherItems != null)
                {
                    recList = otherItems.SelectNodes(".//li[@class='a-spacing-medium p13n-sc-list-item']");
                    if (recList != null)
                    {
                        var possibleBooks = new List<BookInfo>();
                        // TODO: This entire foreach is pretty much the exact same as the one above...
                        foreach (HtmlNode result in recList.Where(result => result != null))
                        {
                            HtmlNode otherBook =
                                result.SelectSingleNode(".//div[@class='a-fixed-left-grid-col a-col-left']/a");
                            if (otherBook == null)
                                continue;
                            var sponsAsin = Amazon.ParseAsinFromUrl(otherBook.GetAttributeValue("href", ""));

                            otherBook = otherBook.SelectSingleNode(".//img");
                            var match = Regex.Match(otherBook.GetAttributeValue("alt", ""),
                                @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)",
                                RegexOptions.IgnoreCase);
                            if (match.Success)
                                continue;
                            var sponsTitle = otherBook.GetAttributeValue("alt", "");
                            //Check for duplicate by title
                            if (custAlsoBought.Any(bk => bk.title.Contains(sponsTitle)) || possibleBooks.Any(bk => bk.title.Contains(sponsTitle)))
                                continue;
                            otherBook = result.SelectSingleNode(".//a[@class='a-size-small a-link-child']")
                                ?? result.SelectSingleNode(".//span[@class='a-size-small a-color-base']")
                                ?? throw new FormatChangedException("Amazon", "Sponsored book author");
                            // TODO: Throw more format changed exceptions to make it obvious that the site changed
                            var sponsAuthor = otherBook.InnerText.Trim();
                            possibleBooks.Add(new BookInfo(sponsTitle, sponsAuthor, sponsAsin));
                        }

                        var bookBag = new ConcurrentBag<BookInfo>();
                        await possibleBooks.ParallelForEachAsync(async book =>
                        {
                            //Gather book desc, image url, etc, if using new format
                            try
                            {
                                if (_settings.UseNewVersion)
                                    await book.GetAmazonInfo(book.amazonUrl, cancellationToken);
                                bookBag.Add(book);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log($"Error: {ex.Message}\r\n{book.amazonUrl}");
                            }
                        }, 5, cancellationToken);
                        custAlsoBought.AddRange(bookBag);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred parsing the book's amazon page: " + ex.Message + ex.StackTrace);
                return false;
            }
            SetPaths();
            return true;
        }

        public void GenerateOld()
        {
            //Create final EndActions.data.ASIN.asc
            string dt = DateTime.Now.ToString("s");
            string tz = DateTime.Now.ToString("zzz");
            XmlTextWriter writer = new XmlTextWriter(EaPath, Encoding.UTF8);
            Logger.Log("Writing EndActions to file...");
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            writer.WriteStartElement("endaction");
            writer.WriteAttributeString("version", "0");
            writer.WriteAttributeString("guid", $"{curBook.databasename}:{curBook.Guid}");
            writer.WriteAttributeString("key", curBook.asin);
            writer.WriteAttributeString("type", "EBOK");
            writer.WriteAttributeString("timestamp", dt + tz);
            writer.WriteElementString("treatment", "d");
            writer.WriteStartElement("currentBook");
            writer.WriteElementString("imageUrl", curBook.bookImageUrl);
            writer.WriteElementString("asin", curBook.asin);
            writer.WriteElementString("hasSample", "false");
            writer.WriteEndElement();
            writer.WriteStartElement("customerProfile");
            writer.WriteElementString("penName", _settings.PenName);
            writer.WriteElementString("realName", _settings.RealName);
            writer.WriteEndElement();
            writer.WriteStartElement("recs");
            writer.WriteAttributeString("type", "author");
            for (int i = 0; i < Math.Min(_authorProfile.otherBooks.Count, 5); i++)
            {
                writer.WriteStartElement("rec");
                writer.WriteAttributeString("hasSample", "false");
                writer.WriteAttributeString("asin", _authorProfile.otherBooks[i].asin);
                writer.WriteElementString("title", _authorProfile.otherBooks[i].title);
                writer.WriteElementString("author", curBook.author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("recs");
            writer.WriteAttributeString("type", "purchase");
            for (int i = 0; i < Math.Min(custAlsoBought.Count, 5); i++)
            {
                writer.WriteStartElement("rec");
                writer.WriteAttributeString("hasSample", "false");
                writer.WriteAttributeString("asin", custAlsoBought[i].asin);
                writer.WriteElementString("title", custAlsoBought[i].title);
                writer.WriteElementString("author", custAlsoBought[i].author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteElementString("booksMentionedPosition", "2");
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            Logger.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
        }

        private async Task<BookInfo> SearchOrPrompt(BookInfo book, CancellationToken cancellationToken = default)
        {
            // If the asin was available from another source, use it
            if (!string.IsNullOrEmpty(book.asin))
            {
                await book.GetAmazonInfo($"https://www.amazon.{_settings.AmazonTld}/dp/{book.asin}", cancellationToken);
                return book;
            }

            BookInfo newBook;
            try
            {

                newBook = await Amazon.SearchBook(book.title, book.author, _settings.AmazonTld, cancellationToken);
                if (newBook == null && _settings.PromptAsin && _asinPrompt != null)
                {
                    Logger.Log($"ASIN prompt for {book.title}...");
                    var asin = _asinPrompt(book.title, book.author);
                    if (string.IsNullOrWhiteSpace(asin))
                        return null;
                    Logger.Log($"ASIN supplied: {asin}");
                    newBook = new BookInfo(book.title, book.author, asin);
                }
            }
            catch
            {
                Logger.Log($"Failed to find {book.title} on Amazon.{_settings.AmazonTld}, trying again with Amazon.com.");
                newBook = await Amazon.SearchBook(book.title, book.author, "com", cancellationToken);
            }

            if (newBook != null)
                await newBook.GetAmazonInfo(newBook.amazonUrl, cancellationToken); //fill in desc, imageurl, and ratings

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
                return _authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, $@"^{book.title}(?: \(.*\))?$"))
                    ?? await SearchOrPrompt(book, ct);
            }

            // TODO: Don't juggle around bookinfos
            if (series.Next != null)
                series.Next = await FromApOrSearch(series.Next, cancellationToken);
            if (series.Previous != null)
                series.Previous = await FromApOrSearch(series.Previous, cancellationToken);

            if (series.Next == null)
            {
                Logger.Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n"
                    + "Please report this book and the Goodreads URL and output log to improve parsing (if it's a real book).");
            }
        }

        public async Task GenerateNewFormatData(IProgressBar progress, CancellationToken token = default)
        {
            try
            {
                await _dataSource.GetExtrasAsync(curBook, progress, token);
                curBook.Series = await _dataSource.GetSeriesInfoAsync(curBook.dataUrl, token);

                if (curBook.Series == null || curBook.Series.Total == 0)
                    Logger.Log("The book was not found to be part of a series.");
                else if (curBook.Series.Next == null && curBook.Series.Position != curBook.Series.Total.ToString())// && !curBook.Series.Position?.Contains(".") == true)
                    Logger.Log("An error occurred finding the next book in series. The book may not be part of a series, or it is the latest release.");
                else
                    await ExpandSeriesMetadata(curBook.Series, token);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))
                    Logger.Log("An error occurred finding next book in series: Goodreads URL not found.\r\n" +
                               "If reading from a file, you can switch the source to Goodreads to specify a URL, then switch back to File.");
                else
                    Logger.Log("An error occurred finding next book in series: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }

            // TODO: Refactor next/previous series stuff
            if (curBook.Series?.Next == null)
            {
                try
                {
                    var seriesResult = await Amazon.DownloadNextInSeries(curBook.asin, token);
                    switch (seriesResult?.Error?.ErrorCode)
                    {
                        case "ERR004":
                            Logger.Log("According to Amazon, this book is not part of a series.");
                            break;
                        case "ERR000":
                            if (curBook.Series == null)
                                curBook.Series = new SeriesInfo();
                            curBook.Series.Next =
                                new BookInfo(seriesResult.NextBook.Title.TitleName,
                                    Functions.FixAuthor(seriesResult.NextBook.Authors.FirstOrDefault()?.AuthorName),
                                    seriesResult.NextBook.Asin);
                            await curBook.Series.Next.GetAmazonInfo(curBook.Series.Next.amazonUrl, token);
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
                Logger.Log($"\nSeries URL: {curBook.Series.Url}");
                if (!string.IsNullOrEmpty(curBook.Series.Name))
                    Logger.Log($"This is book {curBook.Series.Position} of {curBook.Series.Total} in the {curBook.Series.Name} series");
                if (curBook.Series.Previous != null)
                    Logger.Log($"Preceded by: {curBook.Series.Previous.title}");
                if (curBook.Series.Next != null)
                    Logger.Log($"Followed by: {curBook.Series.Next.title}\n");
            }

            try
            {
                if (!await _dataSource.GetPageCountAsync(curBook, token))
                {
                    if (!Properties.Settings.Default.pageCount)
                        Logger.Log("No page count found on Goodreads");
                    Logger.Log("Attempting to estimate page count...");
                    Logger.Log(Functions.GetPageCount(curBook.rawmlPath, curBook));
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while searching for or estimating the page count: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }
        }

        public async Task GenerateEndActionsFromBase(Model.EndActions baseEndActions)
        {
            baseEndActions.BookInfo = new Model.EndActions.EndActionsBookInfo
            {
                Asin = curBook.asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAATAAB",
                ImageUrl = curBook.bookImageUrl,
                EmbeddedID = $"{curBook.databasename}:{curBook.Guid}",
                Erl = _erl
            };
            baseEndActions.Data.FollowSubscriptions = new Model.EndActions.AuthorSubscriptions
            {
                Subscriptions = new[]
                {
                    new Subscription
                    {
                        Asin = curBook.authorAsin,
                        Name = curBook.author,
                        ImageUrl = curBook.authorImageUrl
                    }
                }
            };
            baseEndActions.Data.AuthorSubscriptions = baseEndActions.Data.FollowSubscriptions;
            baseEndActions.Data.NextBook = Extensions.BookInfoToBook(curBook.Series?.Next, false);
            baseEndActions.Data.PublicSharedRating = new Model.EndActions.Rating
            {
                Class = "publicSharedRating",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                Value = Math.Round(curBook.amazonRating, 1)
            };
            baseEndActions.Data.CustomerProfile = new Model.EndActions.CustomerProfile
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
                        Asin = _authorProfile.authorAsin,
                        Name = curBook.author,
                        Bio = _authorProfile.BioTrimmed,
                        ImageUrl = _authorProfile.authorImageUrl
                    }
                }
            };
            baseEndActions.Data.AuthorRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = _authorProfile.otherBooks.Select(bk => Extensions.BookInfoToBook(bk, true)).ToArray()
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
                Logger.Log("An error occurred creating the EndAction data template: " + ex.Message + "\r\n" + ex.StackTrace);
                throw;
            }

            Logger.Log("Writing EndActions to file...");
            using (StreamWriter streamWriter = new StreamWriter(EaPath, false))
            {
                await streamWriter.WriteAsync(finalOutput);
                streamWriter.Flush();
            }
            Logger.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
        }

        public string GenerateStartActionsFromBase(StartActions baseStartActions)
        {
            baseStartActions.BookInfo = new StartActions.StartActionsBookInfo
            {
                Asin = curBook.asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAAgAAA",
                ImageUrl = curBook.bookImageUrl,
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
                        Asin = curBook.authorAsin,
                        Name = curBook.author,
                        ImageUrl = curBook.authorImageUrl
                    }
                }
            };
            baseStartActions.Data.AuthorSubscriptions = baseStartActions.Data.FollowSubscriptions;
            baseStartActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMPASSAGES%", curBook.notableClips.Count.ToString());
            baseStartActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMHIGHLIGHTS%", curBook.notableClips.Sum(c => c.Likes).ToString());
            baseStartActions.Data.GrokShelfInfo.Asin = curBook.asin;
            baseStartActions.Data.BookDescription = Extensions.BookInfoToBook(curBook, true);
            baseStartActions.Data.CurrentBook = baseStartActions.Data.BookDescription;
            baseStartActions.Data.AuthorBios = new AuthorBios
            {
                Authors = new []
                {
                    new Author
                    {
                        // TODO: Check mismatched fields from curbook and authorprofile
                        Asin = _authorProfile.authorAsin,
                        Name = curBook.author,
                        Bio = _authorProfile.BioTrimmed,
                        ImageUrl = _authorProfile.authorImageUrl
                    }
                }
            };
            baseStartActions.Data.AuthorRecs = new Recs
            {
                Class = "recommendationList",
                Recommendations = _authorProfile.otherBooks.Select(bk => Extensions.BookInfoToBook(bk, false)).ToArray()
            };
            baseStartActions.Data.ReadingTime.Hours = curBook.readingHours;
            baseStartActions.Data.ReadingTime.Minutes = curBook.readingMinutes;
            baseStartActions.Data.ReadingTime.FormattedTime.Replace("%HOURS%", curBook.readingHours.ToString());
            baseStartActions.Data.ReadingTime.FormattedTime.Replace("%MINUTES%", curBook.readingMinutes.ToString());
            baseStartActions.Data.PreviousBookInTheSeries = Extensions.BookInfoToBook(curBook.Series?.Previous, true);
            baseStartActions.Data.ReadingPages.PagesInBook = curBook.pagesInBook;

            try
            {
                return Functions.ExpandUnicode(JsonConvert.SerializeObject(baseStartActions));
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating the StartActions template: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return null;
        }

        public void WriteStartActions(string saContent)
        {
            if (string.IsNullOrEmpty(saContent)) return;
            Logger.Log("Writing StartActions to file...");
            using (var streamWriter = new StreamWriter(SaPath, false))
            {
                streamWriter.Write(saContent);
                streamWriter.Flush();
            }
            Logger.Log("StartActions file created successfully!\r\nSaved to " + SaPath);
        }

        private void SetPaths()
        {
            string outputDir;
            try
            {
                if (_settings.Android)
                {
                    outputDir = _settings.OutDir + @"\Android\" + curBook.asin;
                    Directory.CreateDirectory(outputDir);
                }
                else
                    outputDir = _settings.UseSubDirectories ? Functions.GetBookOutputDirectory(curBook.author, curBook.sidecarName, true) : _settings.OutDir;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating the output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = _settings.OutDir;
            }
            EaPath = outputDir + @"\EndActions.data." + curBook.asin + ".asc";
            SaPath = outputDir + @"\StartActions.data." + curBook.asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(EaPath))
            {
                Logger.Log("Error: EndActions file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
            }
        }

        public class Settings
        {
            public string OutDir { get; set; }
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
