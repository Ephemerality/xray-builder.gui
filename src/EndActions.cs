using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI
{
    class EndActions
    {
        private Properties.Settings settings = Properties.Settings.Default;
        private frmMain main;

        private string EaPath = "";
        private string SaPath = "";
        private long _erl;

        public List<BookInfo> custAlsoBought = new List<BookInfo>();

        private AuthorProfile authorProfile;
        public BookInfo curBook;
        DataSources.DataSource dataSource;

        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        public EndActions(AuthorProfile ap, BookInfo book, long erl, DataSources.DataSource dataSource, frmMain frm)
        {
            authorProfile = ap;
            curBook = book;
            _erl = erl;
            this.dataSource = dataSource;
            main = frm;
        }

        public async Task<bool> Generate()
        {
            Logger.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            string ebookLocation = String.Format(@"https://www.amazon.{0}/dp/{1}", settings.amazonTLD, curBook.asin);

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
                string nodeTitleCheck = "", nodeUrl = "", cleanAuthor = "";
                HtmlNodeCollection recList =
                    bookHtmlDoc.DocumentNode.SelectNodes(
                        "//ol[@class='a-carousel' and @role='list']/li[@class='a-carousel-card a-float-left']");
                if (recList != null)
                {
                    foreach (HtmlNode item in recList.Where(item => item != null))
                    {
                        HtmlNode nodeTitle = item.SelectSingleNode(".//div/a");
                        nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                        nodeUrl = nodeTitle.GetAttributeValue("href", "");
                        if (nodeUrl != "")
                            nodeUrl = "https://www.amazon." + settings.amazonTLD + nodeUrl;
                        if (nodeTitleCheck == "")
                        {
                            nodeTitle = item.SelectSingleNode(".//div/a");
                            //Remove CR, LF and TAB
                            nodeTitleCheck = nodeTitle.InnerText.Clean();
                        }
                        cleanAuthor = item.SelectSingleNode(".//div/div").InnerText.Clean();
                        //Exclude the current book title from other books search
                        Match match = Regex.Match(nodeTitleCheck, curBook.title, RegexOptions.IgnoreCase);
                        if (match.Success)
                            continue;
                        match = Regex.Match(nodeTitleCheck,
                            @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)",
                            RegexOptions.IgnoreCase);
                        if (match.Success)
                            continue;
                        BookInfo newBook = new BookInfo(nodeTitleCheck, cleanAuthor,
                            item.SelectSingleNode(".//div").GetAttributeValue("data-asin", ""));
                        try
                        {
                            //Gather book desc, image url, etc, if using new format
                            if (settings.useNewVersion)
                                await newBook.GetAmazonInfo(nodeUrl);
                            custAlsoBought.Add(newBook);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(String.Format("Error: {0}\r\n{1}", ex.Message, nodeUrl));
                            return false;
                        }
                    }
                }
                //Add sponsored related, if they exist...
                HtmlNode otherItems =
                    bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='view_to_purchase-sims-feature']");
                if (otherItems != null)
                {
                    recList = otherItems.SelectNodes(".//li[@class='a-spacing-medium p13n-sc-list-item']");
                    if (recList != null)
                    {
                        string sponsTitle = "", sponsAuthor = "", sponsAsin = "", sponsUrl = "";
                        foreach (HtmlNode result in recList.Where(result => result != null))
                        {
                            HtmlNode otherBook = result.SelectSingleNode(".//div[@class='a-fixed-left-grid-col a-col-left']/a");
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
                                sponsUrl = String.Format("https://www.amazon.{1}/dp/{0}", sponsAsin, settings.amazonTLD);
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
                                    ".//div[@class='a-row a-size-small']");
                            sponsAuthor = otherBook.InnerText.Trim();
                            BookInfo newBook = new BookInfo(sponsTitle, sponsAuthor, sponsAsin);
                            //Gather book desc, image url, etc, if using new format
                            try
                            {
                                if (settings.useNewVersion)
                                    await newBook.GetAmazonInfo(sponsUrl);
                                custAlsoBought.Add(newBook);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(String.Format("Error: {0}\r\n{1}", ex.Message, sponsUrl));
                                return false;
                            }
                        }
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
            try
            {
                Logger.Log("Writing EndActions to file...");
                writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
                writer.WriteStartElement("endaction");
                writer.WriteAttributeString("version", "0");
                writer.WriteAttributeString("guid", curBook.databasename + ":" + curBook.guid);
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
                writer.WriteElementString("penName", settings.penName);
                writer.WriteElementString("realName", settings.realName);
                writer.WriteEndElement();
                writer.WriteStartElement("recs");
                writer.WriteAttributeString("type", "author");
                for (int i = 0; i < Math.Min(authorProfile.otherBooks.Count, 5); i++)
                {
                    writer.WriteStartElement("rec");
                    writer.WriteAttributeString("hasSample", "false");
                    writer.WriteAttributeString("asin", authorProfile.otherBooks[i].asin);
                    writer.WriteElementString("title", authorProfile.otherBooks[i].title);
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
                main.cmsPreview.Items[1].Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while writing the End Action file: " + ex.Message + "\r\n" + ex.StackTrace);
                return;
            }
        }

        public async Task GenerateEndActions(CancellationToken token)
        {
            string[] templates = GetBaseTemplates(Environment.CurrentDirectory + @"\dist\BaseEndActions.txt", 3);
            if (templates == null) return;

            Logger.Log(String.Format("Gathering additional metadata for {0}...", curBook.title));
            string bookInfoTemplate = templates[0];
            string widgetsTemplate = templates[1];
            string layoutsTemplate = templates[2];
            string finalOutput = "{{{0},{1},{2},{3}}}"; //bookInfo, widgets, layouts, data
            
            // Build bookInfo object
            TimeSpan timestamp = DateTime.Now - new DateTime(1970, 1, 1);
            bookInfoTemplate = String.Format(bookInfoTemplate, curBook.asin, Math.Round(timestamp.TotalMilliseconds), curBook.bookImageUrl, curBook.databasename, curBook.guid, _erl);
            double dateMs = Math.Round(timestamp.TotalMilliseconds);
            string ratingText = Math.Floor(curBook.amazonRating).ToString();

            // Build data object
            string dataTemplate = "";
            string nextBook = "{}";

            string followSubscriptions = String.Format(@"""followSubscriptions"":{{""class"":""authorSubscriptionInfoList"",""subscriptions"":[{{""class"":""authorSubscriptionInfo"",""asin"":""{0}"",""name"":""{1}"",""subscribed"":false,""imageUrl"":""{2}""}}]}}", curBook.authorAsin, curBook.author, curBook.authorImageUrl);
            string authorSubscriptions = String.Format(@"""authorSubscriptions"":{{""class"":""authorSubscriptionInfoList"",""subscriptions"":[{{""class"":""authorSubscriptionInfo"",""asin"":""{0}"",""name"":""{1}"",""subscribed"":false,""imageUrl"":""{2}""}}]}}", curBook.authorAsin, curBook.author, curBook.authorImageUrl);
            string publicSharedRating = String.Format(@"""publicSharedRating"":{{""class"":""publicSharedRating"",""timestamp"":{0},""value"":{1}}}", dateMs, ratingText);
            string customerProfile = String.Format(@"""customerProfile"":{{""class"":""customerProfile"",""penName"":""{0}"",""realName"":""{1}""}}", settings.penName, settings.realName);
            string rating = String.Format(@"""rating"":{{""class"":""personalizationRating"",""timestamp"":{0},""value"":{1}}}", dateMs, ratingText);
            string authorBios = String.Format(@"""authorBios"":{{""class"":""authorBioList"",""authors"":[{0}]}}", authorProfile.ToJSON());
            string authorRecs = @"""authorRecs"":{{""class"":""featuredRecommendationList"",""recommendations"":[{0}]}}";
            string customersWhoBoughtRecs = @"""customersWhoBoughtRecs"":{{""class"":""featuredRecommendationList"",""recommendations"":[{0}]}}";
            string goodReads = String.Format(@"""goodReadsReview"":{{""class"":""goodReadsReview"",""reviewId"":""NoReviewId"",""rating"":{0},""submissionDateMs"":{1}}}", ratingText, dateMs);

            try
            {
                Progress<Tuple<int, int>> progress = new Progress<Tuple<int, int>>(main.UpdateProgressBar);
                await dataSource.GetExtras(curBook, token, progress);
                curBook.nextInSeries = await dataSource.GetNextInSeries(curBook, authorProfile, settings.amazonTLD);
                nextBook = curBook.nextInSeries != null ? curBook.nextInSeries.ToJSON("recommendation", false) : "";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))
                    Logger.Log("An error occurred finding next book in series: Goodreads URL not found.\r\n" +
                        "If reading from a file, you can switch the source to Goodreads to specify a URL, then switch back to File.");
                else
                    Logger.Log("An error occurred finding next book in series: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            try
            {
                if (!(await dataSource.GetPageCount(curBook)))
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
            }

            if (authorProfile.otherBooks.Count > 0)
                authorRecs = String.Format(authorRecs,
                    String.Join(",",
                        authorProfile.otherBooks.Select(bk => bk.ToJSON("featuredRecommendation", true)).ToArray()));
            if (custAlsoBought.Count > 0)
                customersWhoBoughtRecs = String.Format(customersWhoBoughtRecs,
                    String.Join(",", custAlsoBought.Select(bk => bk.ToJSON("featuredRecommendation", true)).ToArray()));
            try
            {
                if (nextBook != "")
                {
                    dataTemplate = @"""data"":{{{0},""nextBook"":{1},{2},{3},{4},{5},{6},{7},{8}}}";
                    dataTemplate = String.Format(dataTemplate,
                        followSubscriptions,
                        nextBook,
                        publicSharedRating,
                        customerProfile,
                        rating,
                        authorBios,
                        authorRecs,
                        customersWhoBoughtRecs,
                        authorSubscriptions);
                    dataTemplate = dataTemplate.Replace(",,", ",");
                }
                else
                {
                    dataTemplate = @"""data"":{{{0},{1},{2},{3},{4},{5},{6},{7}}}";
                    dataTemplate = String.Format(dataTemplate,
                        followSubscriptions,
                        publicSharedRating,
                        customerProfile,
                        rating,
                        authorBios,
                        authorRecs,
                        customersWhoBoughtRecs,
                        authorSubscriptions);
                    dataTemplate = dataTemplate.Replace(",,", ",");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating the EndAction data template: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            finalOutput = String.Format(finalOutput, bookInfoTemplate, widgetsTemplate, layoutsTemplate, dataTemplate);

            Logger.Log("Writing EndActions to file...");
            using (StreamWriter streamWriter = new StreamWriter(EaPath, false))
            {
                streamWriter.Write(finalOutput);
                streamWriter.Flush();
            }
            Logger.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
            main.cmsPreview.Items[1].Enabled = true;
        }

        public void GenerateStartActions()
        {
            string[] templates = GetBaseTemplates(Environment.CurrentDirectory + @"\dist\BaseStartActions.txt", 4);
            if (templates == null) return;

            string bookInfoTemplate = templates[0];
            string widgetsTemplate = templates[1];
            string layoutsTemplate = templates[2];
            string welcomeTextTemplate = templates[3];

            string finalOutput = "{{{0},{1},{2},{3}}}"; //bookInfo, widgets, layouts, welcometext, data

            // Build bookInfo object
            TimeSpan timestamp = DateTime.Now - new DateTime(1970, 1, 1);
            bookInfoTemplate = String.Format(bookInfoTemplate, curBook.asin, Math.Round(timestamp.TotalMilliseconds), curBook.bookImageUrl);

            // Build data object
            string dataTemplate = "";
            string authorRecsTemplate = @"""authorRecs"":{{""class"":""recommendationList"",""recommendations"":[{0}]}}";

            string seriesPosition = curBook.seriesPosition == "" ? "" : String.Format(@"""seriesPosition"":{{""class"":""seriesPosition"",""positionInSeries"":{0},""totalInSeries"":{1},""seriesName"":""{2}""}}", curBook.seriesPosition, curBook.totalInSeries, curBook.seriesName);
            string followSubscriptions = String.Format(@"""followSubscriptions"":{{""class"":""authorSubscriptionInfoList"",""subscriptions"":[{{""class"":""authorSubscriptionInfo"",""asin"":""{0}"",""name"":""{1}"",""subscribed"":false,""imageUrl"":""{2}""}}]}}", curBook.authorAsin, curBook.author, curBook.authorImageUrl);
            string popularHighlightsText = curBook.notableClips == null ? "" : String.Format(@"""popularHighlightsText"":{{""class"":""dynamicText"",""localizedText"":{{""de"":""{0} Passagen wurden {1} mal markiert"",""en-US"":""{0} passages have been highlighted {1} times"",""ru"":""1\u00A0902 \u043E\u0442\u0440\u044B\u0432\u043A\u043E\u0432 \u0431\u044B\u043B\u043E \u0432\u044B\u0434\u0435\u043B\u0435\u043D\u043E 18\u00A0660 \u0440\u0430\u0437"",""pt-BR"":""{0} trechos foram destacados {1} vezes"",""ja"":""{0}\u7B87\u6240\u304C{1}\u56DE\u30CF\u30A4\u30E9\u30A4\u30C8\u3055\u308C\u307E\u3057\u305F"",""en"":""{0} passages have been highlighted {1} times"",""it"":""{0} brani sono stati evidenziati {1} volte"",""fr"":""{0}\u00A0902 passages ont \u00E9t\u00E9 surlign\u00E9s {1}\u00A0660 fois"",""zh-CN"":""{0} \u4E2A\u6BB5\u843D\u88AB\u6807\u6CE8\u4E86 {1} \u6B21"",""es"":""Se han subrayado {0} pasajes {1} veces"",""nl"":""{0} fragmenten zijn {1} keer gemarkeerd""}}}}", curBook.notableClips.Count, curBook.notableClips.Sum(c => c.Item2));
            string grokShelfInfo = String.Format(@"""grokShelfInfo"":{{""class"":""goodReadsShelfInfo"",""asin"":""{0}"",""shelves"":[""to-read""],""is_sensitive"":false,""is_autoshelving_enabled"":true}}", curBook.asin);
            string bookDescription = String.Format(@"""bookDescription"":{0}", curBook.ToExtraJSON("featuredRecommendation"));
            string authorBios = String.Format(@"""authorBios"":{{""class"":""authorBioList"",""authors"":[{0}]}}", authorProfile.ToJSON());
            string authorRecs = authorProfile.otherBooks.Count > 0 ? String.Format(authorRecsTemplate, String.Join(",", authorProfile.otherBooks.Select(bk => bk.ToJSON("recommendation", false)).ToArray())) : "";
            string currentBook = String.Format(@"""currentBook"":{0}", curBook.ToExtraJSON("featuredRecommendation"));
            string readingTime = String.Format(@"""readingTime"":{{""class"":""time"",""hours"":{0},""minutes"":{1},""formattedTime"":{{""de"":""{0} Stunden und {1} Minuten"",""en-US"":""{0} hours and {1} minutes"",""ru"":""{0}\u00A0\u0447 \u043{0} {1}\u00A0\u043C\u043{0}\u043D"",""pt-BR"":""{0} horas e {1} minutos"",""ja"":""{0}\u6642\u9593{1}\u5206"",""en"":""{0} hours and {1} minutes"",""it"":""{0} ore e {1} minuti"",""fr"":""{0} heures et {1} minutes"",""zh-CN"":""{0} \u5C0F\u65F6 {1} \u5206\u949F"",""es"":""{0} horas y {1} minutos"",""nl"":""{0} uur en {1} minuten""}}}}", curBook.readingHours, curBook.readingMinutes);
            string previousBookInSeries = curBook.previousInSeries == null ? "" : String.Format(@"""previousBookInTheSeries"":{0}", curBook.previousInSeries.ToExtraJSON("featuredRecommendation"));
            string authorSubscriptions = String.Format(@"""authorSubscriptions"":{{""class"":""authorSubscriptionInfoList"",""subscriptions"":[{{""class"":""authorSubscriptionInfo"",""asin"":""{0}"",""name"":""{1}"",""subscribed"":false,""imageUrl"":""{2}""}}]}}", curBook.authorAsin, curBook.author, curBook.authorImageUrl);
            string readingPages = String.Format(@"""readingPages"":{{""class"":""pages"",""pagesInBook"":{0}}}", curBook.pagesInBook);

            try
            {
                dataTemplate = @"""data"":{{{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}}}";

                dataTemplate = string.Format(dataTemplate,
                    seriesPosition,
                    followSubscriptions,
                    welcomeTextTemplate,
                    popularHighlightsText,
                    grokShelfInfo,
                    bookDescription,
                    authorBios,
                    authorRecs,
                    currentBook,
                    readingTime,
                    previousBookInSeries,
                    authorSubscriptions,
                    readingPages);
                dataTemplate = dataTemplate.Replace(",,", ",");

                finalOutput = String.Format(finalOutput, bookInfoTemplate, widgetsTemplate, layoutsTemplate, dataTemplate);
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating the StartAction data template: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            Logger.Log("Writing StartActions to file...");
            using (StreamWriter streamWriter = new StreamWriter(SaPath, false))
            {
                streamWriter.Write(finalOutput);
                streamWriter.Flush();
            }
            Logger.Log("StartActions file created successfully!\r\nSaved to " + SaPath);
            main.cmsPreview.Items[3].Enabled = true;
        }

        private void SetPaths()
        {
            string outputDir;
            try
            {
                if (settings.android)
                {
                    outputDir = settings.outDir + @"\Android\" + curBook.asin;
                    Directory.CreateDirectory(outputDir);
                }
                else
                    outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectory(curBook.author, curBook.sidecarName) : settings.outDir;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating the output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            EaPath = outputDir + @"\EndActions.data." + curBook.asin + ".asc";
            SaPath = outputDir + @"\StartActions.data." + curBook.asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(EaPath))
            {
                Logger.Log("Error: EndActions file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return;
            }
        }

        /// <summary>
        /// Retrieve templates from specified file.
        /// Array will always have the length of templateCount. Index 0 will always be the bookInfo template.
        /// </summary>
        private string[] GetBaseTemplates(string baseFile, int templateCount)
        {
            string[] templates = null;
            try
            {
                using (StreamReader streamReader = new StreamReader(baseFile, Encoding.UTF8))
                {
                    templates = streamReader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    templates = templates.Where(r => !r.StartsWith("//")).ToArray(); //Remove commented lines
                    if (templates == null || templates.Length != templateCount || !templates[0].StartsWith(@"""bookInfo"""))
                    {
                        Logger.Log("An error occurred parsing " + baseFile + ". If you modified it, ensure you followed the specified format.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while opening the " + baseFile + " file.\r\n" +
                    "Ensure you extracted it to the same directory as the program.\r\n" +
                    ex.Message);
            }
            return templates;
        }
    }
}
