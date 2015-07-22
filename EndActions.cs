using System;
using System.Collections.Generic;
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
        private Properties.Settings settings = XRayBuilderGUI.Properties.Settings.Default;
        private frmMain main;

        private string EaPath = "";
        private string EaDest = "";
        private string bookImageUrl = "";

        public List<BookInfo> custAlsoBought = new List<BookInfo>();

        private AuthorProfile authorProfile = null;
        private BookInfo curBook = null;

        public bool complete = false; //Set if constructor succeeds in gathering data
        
        //Requires an already-built AuthorProfile and the BaseEndActions.txt file
        public EndActions(AuthorProfile ap, BookInfo book, frmMain frm)
        {
            authorProfile = ap;
            curBook = book;
            main = frm;

            main.Log("Building End Actions...");
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
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.bookHtml.txt", book.asin),
                        ap.authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving bookHtml.txt: {0}", ex.Message));
                    return;
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

            main.Log("Gathering recommended book info...");
            //Parse Recommended Author titles and ASINs
            try
            {
                HtmlNodeCollection recList = bookHtmlDoc.DocumentNode.SelectNodes("//li[@class='a-carousel-card a-float-left']");
                if (recList == null)
                    main.Log("Could not find related book list page on Amazon.\r\nUnable to create End Actions.");
                foreach (HtmlNode item in recList.Where(item => item != null))
                {
                    HtmlNode nodeTitle = item.SelectSingleNode(".//div/a");
                    string nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                    string nodeUrl = nodeTitle.GetAttributeValue("href", "");
                    if (nodeUrl != "")
                        nodeUrl = "http://www.amazon.com" + nodeUrl;
                    if (nodeTitleCheck == "")
                    {
                        nodeTitle = item.SelectSingleNode(".//div/a");
                        //Remove CR, LF and TAB
                        nodeTitleCheck = Regex.Replace(nodeTitle.InnerText, @"\t|\n|\r", String.Empty);
                        nodeTitleCheck = nodeTitleCheck.Trim();
                        nodeTitleCheck = Regex.Replace(nodeTitleCheck, @"&#133;", "...");
                    }
                    BookInfo newBook = new BookInfo(nodeTitleCheck, item.SelectSingleNode(".//div/div").InnerText.Trim(),
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
                        main.Log(String.Format("{0}\r\n{1}\r\nContinuing anyway...", ex.Message, nodeUrl));
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
            XmlTextWriter writer = new XmlTextWriter(EaPath, System.Text.Encoding.UTF8);
            try
            {
                main.Log("Writing End Actions to file...");
                //writer.Formatting = Formatting.Indented;
                //writer.Indentation = 4;
                writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
                writer.WriteStartElement("endaction");
                writer.WriteAttributeString("version", "0");
                writer.WriteAttributeString("guid", curBook.databasename + ":" + curBook.guid);
                writer.WriteAttributeString("key", curBook.asin);
                writer.WriteAttributeString("type", "EBOK");
                writer.WriteAttributeString("timestamp", dt + tz);
                writer.WriteElementString("treatment", "d");
                writer.WriteStartElement("currentBook");
                writer.WriteElementString("imageUrl", bookImageUrl);
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
            string[] templates = GetBaseEndActions();
            if (templates == null) return;

            string bookInfoTemplate = templates[0];
            string widgetsTemplate = templates[1];
            string layoutsTemplate = templates[2];
            string finalOutput = "{{{0},{1},{2},{3}}}"; //bookInfo, widgets, layouts, data
            
            // Build bookInfo object
            // "bookInfo":{"class":"bookInfo","asin":"{0}","contentType":"EBOK","timestamp":{1},"refTagSuffix":"AAAgAAB","imageUrl":"{2}","embeddedID":"{3}:{4}","erl":{5}}
            TimeSpan timestamp = DateTime.Now - new DateTime(1970, 1, 1);
            bookInfoTemplate = string.Format(bookInfoTemplate, curBook.asin, Math.Round(timestamp.TotalMilliseconds), bookImageUrl, curBook.databasename, curBook.guid, 0);

            // Build data object
            string dataTemplate = @"""data"":{{""nextBook"":{0},{1},{2},""currentBookFeatured"":{3},{4},{5},{6},{7}}}";
            string nextBook = "";
            string publicSharedRating = @"""publicSharedRating"":{""class"":""publicSharedRating"",""timestamp"":1430656509000,""value"":4}";
            string rating = @"""rating"":{""class"":""personalizationRating"",""timestamp"":1430656509000,""value"":4}";
            string customerProfile = string.Format(@"""customerProfile"":{{""class"":""customerProfile"",""penName"":""{0}"",""realName"":""{1}""}}",
                settings.penName, settings.realName);
            string authors = string.Format(@"""authorBios"":{{""class"":""authorBioList"",""authors"":[{0}]}}", authorProfile.ToJSON());
            string authorRecs = @"""authorRecs"":{{""class"":""featuredRecommendationList"",""recommendations"":[{0}]}}";
            string custRecs = @"""customersWhoBoughtRecs"":{{""class"":""featuredRecommendationList"",""recommendations"":[{0}]}}";
            authorRecs = String.Format(authorRecs, String.Join(",", authorProfile.otherBooks.Select(bk => bk.ToJSON("featuredRecommendation", true)).ToArray()));
            custRecs = String.Format(custRecs, String.Join(",", custAlsoBought.Select(bk => bk.ToJSON("featuredRecommendation", true)).ToArray()));
            //string goodReadsReview = @"""goodReadsReview"":{""class"":""goodReadsReview"",""reviewId"":""NoReviewId"",""rating"":4,""submissionDateMs"":1436900445878}";
            dataTemplate = string.Format(dataTemplate, nextBook, publicSharedRating, customerProfile, curBook.ToJSON("featuredRecommendation", true),
                rating, authors, authorRecs, custRecs);

            finalOutput = String.Format(finalOutput, bookInfoTemplate, widgetsTemplate, layoutsTemplate, dataTemplate);

            using (StreamWriter streamWriter = new StreamWriter(EaPath, false, Encoding.UTF8))
            {
                streamWriter.Write(finalOutput);
                streamWriter.Flush();
                streamWriter.Close();
            }
            main.Log("EndActions file created successfully!\r\nSaved to " + EaPath);
            main.cmsPreview.Items[1].Enabled = true;
        }

        private void SetPaths()
        {
            string outputDir;
            try
            {
                outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectory(curBook.author, curBook.sidecarName) : settings.outDir;
            }
            catch (Exception ex)
            {
                main.Log("Failed to create output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            EaDest = settings.docDir + @"\" + curBook.author + @"\" + curBook.title + @".sdr" + @"\EndActions.data." + curBook.asin + ".asc";
            EaPath = outputDir + @"\EndActions.data." + curBook.asin + ".asc";

            if (!XRayBuilderGUI.Properties.Settings.Default.overwrite && File.Exists(EaPath))
            {
                main.Log("EndActions file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return;
            }
        }

        /// <summary>
        /// Retrieve EndActions templates from BaseEndActions.txt.
        /// Array will always be length 3. Index 0 will always be the bookInfo template.
        /// </summary>
        private string[] GetBaseEndActions()
        {
            string[] templates = null;
            try
            {
                using (StreamReader streamReader = new StreamReader("BaseEndActions.txt", Encoding.UTF8))
                {
                    templates = streamReader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    templates = templates.Where(r => !r.StartsWith("//")).ToArray(); //Remove commented lines
                    if (templates == null || templates.Length != 3 || !templates[0].StartsWith(@"""bookInfo"""))
                    {
                        main.Log("Error parsing BaseEndActions.txt. If you modified it, ensure you followed the specified format.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                main.Log("An error occurred while opening the BaseEndActions.txt file.\r\n" +
                    "Ensure you extracted it to the same directory as the program.\r\n" +
                    ex.Message);
            }
            return templates;
        }
    }
}
