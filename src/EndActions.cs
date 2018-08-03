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
using XRayBuilderGUI.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI
{
    class EndActions
    {
        private string EaPath = "";
        private string SaPath = "";

        public List<BookInfo> custAlsoBought = new List<BookInfo>();

        public BookInfo curBook;
        private readonly AuthorProfile _authorProfile;
        private readonly DataSource _dataSource;
        private readonly long _erl;
        private readonly Settings _settings;

        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        public EndActions(AuthorProfile authorProfile, BookInfo book, long erl, DataSource dataSource, Settings settings)
        {
            _authorProfile = authorProfile;
            curBook = book;
            _erl = erl;
            _dataSource = dataSource;
            _settings = settings;
        }

        /// <summary>
        /// Generate the necessities for both old and new formats
        /// </summary>
        public async Task<bool> Generate()
        {
            Logger.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            string ebookLocation = String.Format(@"https://www.amazon.{0}/dp/{1}", _settings.AmazonTld, curBook.asin);

            // Search Amazon for book
            //Logger.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));
            
            HtmlDocument bookHtmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            try
            {
                bookHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(ebookLocation));
            }
            catch (Exception ex)
            {
                Logger.Log(String.Format("An error ocurred while downloading book's Amazon page: {0}\r\nYour ASIN may not be correct.", ex.Message));
                return false;
            }
            Logger.Log("Book found on Amazon!");
            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    Logger.Log("Saving book's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.bookpageHtml.txt", curBook.asin),
                        bookHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("An error ocurred saving bookpageHtml.txt: {0}", ex.Message));
                }
            }

            try
            {
                curBook.GetAmazonInfo(bookHtmlDoc);
            }
            catch (Exception ex)
            {
                Logger.Log(String.Format("An error ocurred parsing Amazon info: {0}", ex.Message));
                return false;
            }

            Logger.Log("Gathering recommended book metadata...");
            //Parse Recommended Author titles and ASINs
            try
            {
                var nodeUrl = "";
                var recList = bookHtmlDoc.DocumentNode.SelectNodes("//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card a-float-left']");
                if (recList != null)
                {
                    var possibleBooks = new List<BookInfo>();
                    foreach (HtmlNode item in recList.Where(item => item != null))
                    {
                        HtmlNode nodeTitle = item.SelectSingleNode(".//div/a");
                        var nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                        nodeUrl = nodeTitle.GetAttributeValue("href", "");
                        if (nodeUrl != "")
                            nodeUrl = "https://www.amazon." + _settings.AmazonTld + nodeUrl;
                        if (nodeTitleCheck == "")
                        {
                            nodeTitle = item.SelectSingleNode(".//div/a");
                            //Remove CR, LF and TAB
                            nodeTitleCheck = nodeTitle.InnerText.Clean();
                        }

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
                            item.SelectSingleNode(".//div")?.GetAttributeValue("data-asin", null)));
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
                                await book.GetAmazonInfo(nodeUrl);
                            bookBag.Add(book);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error: {ex}\r\n{nodeUrl}");
                        }
                    });
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
                        string sponsTitle, sponsAsin = "", sponsUrl = "";
                        var possibleBooks = new List<BookInfo>();
                        // TODO: This entire foreach is pretty much the exact same as the one above...
                        foreach (HtmlNode result in recList.Where(result => result != null))
                        {
                            HtmlNode otherBook =
                                result.SelectSingleNode(".//div[@class='a-fixed-left-grid-col a-col-left']/a");
                            if (otherBook == null)
                                continue;
                            Match match = Regex.Match(otherBook.GetAttributeValue("href", ""),
                                "dp/(B[A-Z0-9]{9})");
                            if (!match.Success)
                                match = Regex.Match(otherBook.GetAttributeValue("href", ""),
                                    "gp/product/(B[A-Z0-9]{9})");
                            if (match.Success)
                            {
                                sponsAsin = match.Groups[1].Value;
                                sponsUrl = String.Format("https://www.amazon.{1}/dp/{0}", sponsAsin,
                                    _settings.AmazonTld);
                            }

                            otherBook = otherBook.SelectSingleNode(".//img");
                            match = Regex.Match(otherBook.GetAttributeValue("alt", ""),
                                @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)",
                                RegexOptions.IgnoreCase);
                            if (match.Success)
                                continue;
                            sponsTitle = otherBook.GetAttributeValue("alt", "");
                            //Check for duplicate by title
                            BookInfo repeat = custAlsoBought.FirstOrDefault(check => check.title.Contains(sponsTitle));
                            if (repeat != null)
                                continue;
                            otherBook =
                                result.SelectSingleNode(
                                    ".//a[@class='a-size-small a-link-child']")
                                ?? throw new Exception("Amazon format changed)");
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
                                    await book.GetAmazonInfo(sponsUrl);
                                bookBag.Add(book);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log($"Error: {ex.Message}\r\n{sponsUrl}");
                            }
                        });
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

        public async Task GenerateNewFormatData(IProgressBar progress, CancellationToken token)
        {
            try
            {
                await _dataSource.GetExtras(curBook, token, progress);
                curBook.nextInSeries = await _dataSource.GetNextInSeries(curBook, _authorProfile, _settings.AmazonTld);
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

            try
            {
                if (!(await _dataSource.GetPageCount(curBook)))
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

        public async Task GenerateEndActions(IProgressBar progress, CancellationToken token)
        {
            var template = File.ReadAllText(Environment.CurrentDirectory + @"\dist\BaseEndActions.txt", Encoding.UTF8);
            var endActions = JsonConvert.DeserializeObject<Model.EndActions>(template);
            endActions.BookInfo = new Model.EndActions.EndActionsBookInfo
            {
                Asin = curBook.asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAATAAB",
                ImageUrl = curBook.bookImageUrl,
                EmbeddedID = $"{curBook.databasename}:{curBook.Guid}",
                Erl = _erl
            };
            endActions.Data.FollowSubscriptions = new Model.EndActions.AuthorSubscriptions
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
            endActions.Data.AuthorSubscriptions = endActions.Data.FollowSubscriptions;
            endActions.Data.NextBook = Extensions.BookInfoToBook(curBook.nextInSeries, false);
            endActions.Data.PublicSharedRating = new Model.EndActions.Rating
            {
                Class = "publicSharedRating",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                Value = Math.Round(curBook.amazonRating, 1)
            };
            endActions.Data.CustomerProfile = new Model.EndActions.CustomerProfile
            {
                PenName = _settings.PenName,
                RealName = _settings.RealName
            };
            endActions.Data.Rating = endActions.Data.PublicSharedRating;
            endActions.Data.AuthorBios = new AuthorBios
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
            endActions.Data.AuthorRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = _authorProfile.otherBooks.Select(bk => Extensions.BookInfoToBook(bk, true)).ToArray()
            };
            endActions.Data.CustomersWhoBoughtRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = custAlsoBought.Select(bk => Extensions.BookInfoToBook(bk, true)).ToArray()
            };

            //string goodReads = String.Format(@"""goodReadsReview"":{{""class"":""goodReadsReview"",""reviewId"":""NoReviewId"",""rating"":{0},""submissionDateMs"":{1}}}", ratingText, dateMs);

            string finalOutput;
            try
            {
                finalOutput = Functions.ExpandUnicode(JsonConvert.SerializeObject(endActions));
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

        public void GenerateStartActions()
        {
            var template = File.ReadAllText(Environment.CurrentDirectory + @"\dist\BaseStartActions.txt", Encoding.UTF8);
            var startActions = JsonConvert.DeserializeObject<StartActions>(template);
            startActions.BookInfo = new StartActions.StartActionsBookInfo
            {
                Asin = curBook.asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAAgAAA",
                ImageUrl = curBook.bookImageUrl,
                Erl = -1
            };
            if (curBook.seriesPosition != null)
            {
                startActions.Data.SeriesPosition = new StartActions.SeriesPosition
                {
                    PositionInSeries = Convert.ToInt32(double.Parse(curBook.seriesPosition)),
                    TotalInSeries = curBook.totalInSeries,
                    SeriesName = curBook.seriesName
                };
            }
            startActions.Data.FollowSubscriptions = new StartActions.AuthorSubscriptions
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
            startActions.Data.AuthorSubscriptions = startActions.Data.FollowSubscriptions;
            startActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMPASSAGES%", curBook.notableClips.Count.ToString());
            startActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMHIGHLIGHTS%", curBook.notableClips.Sum(c => c.Likes).ToString());
            startActions.Data.GrokShelfInfo.Asin = curBook.asin;
            startActions.Data.BookDescription = Extensions.BookInfoToBook(curBook, true);
            startActions.Data.CurrentBook = startActions.Data.BookDescription;
            startActions.Data.AuthorBios = new AuthorBios
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
            startActions.Data.AuthorRecs = new Recs
            {
                Class = "recommendationList",
                Recommendations = _authorProfile.otherBooks.Select(bk => Extensions.BookInfoToBook(bk, false)).ToArray()
            };
            startActions.Data.ReadingTime.Hours = curBook.readingHours;
            startActions.Data.ReadingTime.Minutes = curBook.readingMinutes;
            startActions.Data.ReadingTime.FormattedTime.Replace("%HOURS%", curBook.readingHours.ToString());
            startActions.Data.ReadingTime.FormattedTime.Replace("%MINUTES%", curBook.readingMinutes.ToString());
            startActions.Data.PreviousBookInTheSeries = Extensions.BookInfoToBook(curBook.previousInSeries, true);
            startActions.Data.ReadingPages.PagesInBook = curBook.pagesInBook;

            string finalOutput;
            try
            {
                finalOutput = Functions.ExpandUnicode(JsonConvert.SerializeObject(startActions));
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating the StartActions template: " + ex.Message + "\r\n" + ex.StackTrace);
                return;
            }

            Logger.Log("Writing StartActions to file...");
            using (StreamWriter streamWriter = new StreamWriter(SaPath, false))
            {
                streamWriter.Write(finalOutput);
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
        }
    }
}
