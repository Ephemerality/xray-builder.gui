using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    class EndActions
    {
        private Properties.Settings settings = Properties.Settings.Default;
        private frmMain main;

        private string EaPath = "";
        private string SaPath = "";
        private long _erl = 0;

        public List<BookInfo> custAlsoBought = new List<BookInfo>();

        private AuthorProfile authorProfile = null;
        public BookInfo curBook = null;
        DataSources.DataSource dataSource = null;

        public bool complete = false; //Set if constructor succeeds in gathering data
        
        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        public EndActions(AuthorProfile ap, BookInfo book, long erl, DataSources.DataSource dataSource, frmMain frm)
        {
            authorProfile = ap;
            curBook = book;
            _erl = erl;
            this.dataSource = dataSource;
            main = frm;

            main.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            string ebookLocation = @"http://www.amazon.com/dp/" + book.asin;

            // Search Amazon for book
            main.Log("Book found on Amazon!");
            main.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));
            
            HtmlDocument bookHtmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            try
            {
                bookHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(ebookLocation));
            }
            catch (Exception ex)
            {
                main.Log(String.Format("An error ocurred while downloading book's Amazon page: {0}\r\nYour ASIN may not be correct.", ex.Message));
                return;
            }
            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    main.Log("Saving book's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.bookpageHtml.txt", curBook.asin),
                        bookHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving bookpageHtml.txt: {0}", ex.Message));
                }
            }

            try
            {
                curBook.GetAmazonInfo(bookHtmlDoc);
            }
            catch (Exception ex)
            {
                main.Log(String.Format("An error ocurred parsing Amazon info: {0}", ex.Message));
                return;
            }

            main.Log("Gathering recommended book metadata...");
            //Parse Recommended Author titles and ASINs
            try
            {
                HtmlNodeCollection recList = bookHtmlDoc.DocumentNode.SelectNodes("//li[@class='a-carousel-card a-float-left']");
                if (recList == null)
                    main.Log("An error occurred finding related book list page on Amazon.\r\nUnable to create End Actions.");
                if (recList != null)
                    foreach (HtmlNode item in recList.Where(item => item != null))
                    {
                        HtmlNode nodeTitle = item.SelectSingleNode(".//div/a");
                        string nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                        string nodeUrl = nodeTitle.GetAttributeValue("href", "");
                        string cleanAuthor = "";
                        if (nodeUrl != "")
                            nodeUrl = "http://www.amazon.com" + nodeUrl;
                        if (nodeTitleCheck == "")
                        {
                            nodeTitle = item.SelectSingleNode(".//div/a");
                            //Remove CR, LF and TAB
                            nodeTitleCheck = nodeTitle.InnerText.CleanString();
                        }
                        cleanAuthor = item.SelectSingleNode(".//div/div").InnerText.CleanString();
                        //Exclude the current book title from other books search
                        Match match = Regex.Match(nodeTitleCheck, curBook.title, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            continue;
                        }
                        match = Regex.Match(nodeTitleCheck, @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            nodeTitleCheck = "";
                            continue;
                        }
                        BookInfo newBook = new BookInfo(nodeTitleCheck, cleanAuthor,
                            item.SelectSingleNode(".//div").GetAttributeValue("data-asin", ""));
                        try
                        {
                            //Gather book desc, image url, etc, if using new format
                            if (settings.useNewVersion)
                                newBook.GetAmazonInfo(nodeUrl);
                            custAlsoBought.Add(newBook);
                        }
                        catch (Exception ex)
                        {
                            main.Log(String.Format("Error: {0}\r\n{1}", ex.Message, nodeUrl));
                            return;
                        }
                    }
            }
            catch (Exception ex)
            {
                main.Log("An error occurred parsing the book's amazon page: " + ex.Message);
                return;
            }

            SetPaths();
            complete = true;
        }

        public void GenerateOld()
        {
            //Create final EndActions.data.ASIN.asc
            string dt = DateTime.Now.ToString("s");
            string tz = DateTime.Now.ToString("zzz");
            XmlTextWriter writer = new XmlTextWriter(EaPath, Encoding.UTF8);
            try
            {
                main.Log("Writing EndActions to file...");
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
                main.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
                main.cmsPreview.Items[1].Enabled = true;
            }
            catch (Exception ex)
            {
                main.Log("An error occurred while writing the End Action file: " + ex.Message);
                return;
            }
        }

        public void GenerateNew()
        {
            string[] templates = GetBaseTemplates("BaseEndActions.txt", 3);
            if (templates == null) return;

            main.Log(String.Format("Gathering additional metadata for {0}...", curBook.title));
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
            string dataTemplate = @"""data"":{{""nextBook"":{0},{1},{2},{3},{4},{5},{6},{7}}}";
            string nextBook = "{}";
            string publicSharedRating = String.Format(@"""publicSharedRating"":{{""class"":""publicSharedRating"",""timestamp"":{0},""value"":{1}}}", dateMs, ratingText);
            string customerProfile = String.Format(@"""customerProfile"":{{""class"":""customerProfile"",""penName"":""{0}"",""realName"":""{1}""}}",
                settings.penName, settings.realName);
            string rating = String.Format(@"""rating"":{{""class"":""personalizationRating"",""timestamp"":{0},""value"":{1}}}", dateMs, ratingText);
            string authors = String.Format(@"""authorBios"":{{""class"":""authorBioList"",""authors"":[{0}]}}", authorProfile.ToJSON());
            string authorRecs = @"""authorRecs"":{{""class"":""featuredRecommendationList"",""recommendations"":[{0}]}}";
            string custRecs = @"""customersWhoBoughtRecs"":{{""class"":""featuredRecommendationList"",""recommendations"":[{0}]}}";
            string goodReads = String.Format(@"""goodReadsReview"":{{""class"":""goodReadsReview"",""reviewId"":""NoReviewId"",""rating"":{0},""submissionDateMs"":{1}}}", ratingText, dateMs);
            try
            {
                curBook.nextInSeries = dataSource.GetNextInSeries(curBook, authorProfile, main.Log);
                if (curBook.nextInSeries != null)
                    nextBook = curBook.nextInSeries.ToJSON("recommendation", false);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))
                    main.Log("An error occurred finding next book in series: Goodreads URL not found.\r\n" +
                        "If reading from a file, you can switch the source to Goodreads to specify a URL, then switch back to File.");
                else
                    main.Log("An error occurred finding next book in series: " + ex.Message + "\r\n" + ex.StackTrace);
            }
            try
            {
                if (!dataSource.GetPageCount(curBook, main.Log))
                {
                    if (!Properties.Settings.Default.pageCount)
                        main.Log("No page count found on Goodreads");
                    main.Log("Attempting to estimate page count...");
                    main.Log(Functions.GetPageCount(curBook.rawmlPath, curBook));
                }
            }
            catch (Exception ex)
            {
                main.Log("An error occurred while searching for or estimating the page count: " + ex.Message);
            }
            authorRecs = String.Format(authorRecs, String.Join(",", authorProfile.otherBooks.Select(bk => bk.ToJSON("featuredRecommendation", true)).ToArray()));
            custRecs = String.Format(custRecs, String.Join(",", custAlsoBought.Select(bk => bk.ToJSON("featuredRecommendation", true)).ToArray()));

            dataTemplate = String.Format(dataTemplate, nextBook, publicSharedRating, customerProfile,
                rating, authors, authorRecs, custRecs, goodReads);

            finalOutput = String.Format(finalOutput, bookInfoTemplate, widgetsTemplate, layoutsTemplate, dataTemplate);

            main.Log("Writing EndActions to file...");
            using (StreamWriter streamWriter = new StreamWriter(EaPath, false))
            {
                streamWriter.Write(finalOutput);
                streamWriter.Flush();
            }
            main.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
            main.cmsPreview.Items[1].Enabled = true;
        }

        public void GenerateStartActions()
        {
            string[] templates = GetBaseTemplates("BaseStartActions.txt", 4);
            if (templates == null) return;

            string bookInfoTemplate = templates[0];
            string widgetsTemplate = templates[1];
            string layoutsTemplate = templates[2];
            string welcomeTextTemplate = templates[3];
            string dataTemplate = "";

            string finalOutput = "{{{0},{1},{2},{3}}}"; //bookInfo, widgets, layouts, welcometext, data

            // Build bookInfo object
            TimeSpan timestamp = DateTime.Now - new DateTime(1970, 1, 1);
            bookInfoTemplate = String.Format(bookInfoTemplate, curBook.asin, Math.Round(timestamp.TotalMilliseconds), curBook.bookImageUrl);
            string seriesPosition = curBook.seriesPosition == "" ? "" :
                String.Format(@"""seriesPosition"":{{""class"":""seriesPosition"",""positionInSeries"":{0},""totalInSeries"":{1},""seriesName"":""{2}""}},",
                    curBook.seriesPosition, curBook.totalInSeries, curBook.seriesName);

            string popularHighlights = curBook.popularHighlights == "" ? "" :
                String.Format(@"""popularHighlightsText"":{{""class"":""dynamicText"",""localizedText"":{{""de"":""{0} Passagen wurden {1} mal markiert"",""en-US"":""{0} passages have been highlighted {1} times"",""ru"":""1\u00A0095 \u043E\u0442\u0440\u044B\u0432\u043A\u043E\u0432 \u0431\u044B\u043B\u043E \u0432\u044B\u0434\u0435\u043B\u0435\u043D\u043E 12\u00A0326 \u0440\u0430\u0437"",""pt-BR"":""{0} trechos foram destacados {1} vezes"",""ja"":""{0}\u7B87\u6240\u304C{1}\u56DE\u30CF\u30A4\u30E9\u30A4\u30C8\u3055\u308C\u307E\u3057\u305F"",""en"":""{0} passages have been highlighted {1} times"",""it"":""{0} brani sono stati evidenziati {1} volte"",""fr"":""{0}\u00A0095 passages ont \u00E9t\u00E9 surlign\u00E9s {1}\u00A0326 fois"",""zh-CN"":""{0} \u4E2A\u6BB5\u843D\u88AB\u6807\u6CE8\u4E86 {1} \u6B21"",""es"":""Se han subrayado {0} pasajes {1} veces"",""nl"":""{0} fragmenten zijn {1} keer gemarkeerd""}}}}", curBook.popularPassages, curBook.popularHighlights);
            string grokShelfInfo = String.Format(@"""grokShelfInfo"":{{""class"":""goodReadsShelfInfo"",""asin"":""{0}"",""shelves"":[""to-read""]}}", curBook.asin);
            string currentBook = curBook.ToExtraJSON("featuredRecommendation");
            string authors = String.Format(@"""authorBios"":{{""class"":""authorBioList"",""authors"":[{0}]}}", authorProfile.ToJSON());
            string authorRecs = @"""authorRecs"":{{""class"":""recommendationList"",""recommendations"":[{0}]}}";
            authorRecs = String.Format(authorRecs, String.Join(",", authorProfile.otherBooks.Select(bk => bk.ToJSON("recommendation", false)).ToArray()));
            string readingTime = String.Format(
                    @"""readingTime"":{{""class"":""time"",""hours"":{0},""minutes"":{1},""formattedTime"":{{""de"":""{0} Stunden und {1} Minuten"",""en-US"":""{0} hours and {1} minutes"",""ru"":""{0}\u00A0\u0447 \u043{0} {1}\u00A0\u043C\u043{0}\u043D"",""pt-BR"":""{0} horas e {1} minutos"",""ja"":""{0}\u6642\u9593{1}\u5206"",""en"":""{0} hours and {1} minutes"",""it"":""{0} ore e {1} minuti"",""fr"":""{0} heures et {1} minutes"",""zh-CN"":""{0} \u5C0F\u65F6 {1} \u5206\u949F"",""es"":""{0} horas y {1} minutos"",""nl"":""{0} uur en {1} minuten""}}}}",
                    curBook.readingHours, curBook.readingMinutes);
            string readingPages = String.Format(@"""readingPages"":{{""class"":""pages"",""pagesInBook"":{0}}}", curBook.pagesInBook);

            // Add previous book in the series if it exists
            string previousBookInSeries = curBook.previousInSeries == null ? "" : 
                String.Format(@"""previousBookInTheSeries"":{0},", curBook.previousInSeries.ToExtraJSON("featuredRecommendation"));
            dataTemplate = @"""data"":{{{0}{1},{2},{3},""bookDescription"":{4},{5},{6},""currentBook"":{7},{8},{9}{10}}}";
            dataTemplate = string.Format(dataTemplate, seriesPosition, welcomeTextTemplate, popularHighlights,
            grokShelfInfo, currentBook, authors, authorRecs, currentBook, readingTime, previousBookInSeries, readingPages);

            finalOutput = String.Format(finalOutput, bookInfoTemplate, widgetsTemplate, layoutsTemplate, dataTemplate);

            main.Log("Writing StartActions to file...");
            using (StreamWriter streamWriter = new StreamWriter(SaPath, false))//, Encoding.UTF8))
            {
                streamWriter.Write(finalOutput);
                streamWriter.Flush();
            }
            main.Log("StartActions file created successfully!\r\nSaved to " + SaPath);
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
                main.Log("An error occurred creating the output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            EaPath = outputDir + @"\EndActions.data." + curBook.asin + ".asc";
            SaPath = outputDir + @"\StartActions.data." + curBook.asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(EaPath))
            {
                main.Log("Error: EndActions file already exists... Skipping!\r\n" +
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
                        main.Log("An error occurred parsing " + baseFile + ". If you modified it, ensure you followed the specified format.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                main.Log("An error occurred while opening the " + baseFile + " file.\r\n" +
                    "Ensure you extracted it to the same directory as the program.\r\n" +
                    ex.Message);
            }
            return templates;
        }
    }
}
