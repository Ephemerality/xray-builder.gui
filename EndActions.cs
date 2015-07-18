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

        public List<string> PurchAlsoBoughtTitles = new List<string>();
        public List<string> PurchAlsoBoughtAsinNumbers = new List<string>();
        public List<string> PurchAlsoBoughtAuthorNames = new List<string>();

        AuthorProfile authorProfile = null;
        BookInfo bookInfo = null;
        
        //Requires an already-built AuthorProfile
        public EndActions(AuthorProfile ap, BookInfo book, frmMain frm)
        {
            authorProfile = ap;
            bookInfo = book;
            main = frm;

            main.Log("Building End Actions...");
            main.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            string ebookLocation = @"http://www.amazon.com/dp/" + book.asin;

            // Search Amazon for book
            main.Log("Book found on Amazon!");
            main.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));
            
            HtmlDocument bookHlmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            try
            {
                bookHlmlDoc.LoadHtml(HttpDownloader.GetPageHtml(ebookLocation));
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

            // Parse Book image URL
            HtmlNode bookImageLoc2 = bookHlmlDoc.DocumentNode.SelectSingleNode("//*[@id='imgBlkFront']");
            if (bookImageLoc2 == null)
                main.Log("Error finding book image. If you want, you can report the book's Amazon URL to help with parsing.");
            else
                bookImageUrl = Regex.Replace(bookImageLoc2.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

            // Generate random book image URL because Amazon keep changing format!
            if (bookImageUrl == "")
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new Random();
                string result = new string(
                    Enumerable.Repeat(chars, 11)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
                bookImageUrl = String.Format("http://ecx.images-amazon.com/images/I/{0}.jpg",
                    Uri.EscapeDataString(result));
            }

            //Parse Recommended Author titles and ASINs
            try
            {
                HtmlNodeCollection recomendationList = bookHlmlDoc.DocumentNode.SelectNodes("//li[@class='a-carousel-card a-float-left']");
                if (recomendationList == null)
                    main.Log("Could not find related book list page on Amazon.\r\nUnable to create End Actions.");
                foreach (HtmlNode item in recomendationList.Where(item => item != null))
                {
                    HtmlNode nodeTitle = item.SelectSingleNode(".//div/a");
                    string nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                    if (nodeTitleCheck == "")
                    {
                        nodeTitle = item.SelectSingleNode(".//div/a");
                        //Remove CR, LF and TAB
                        string cleanTitle = Regex.Replace(nodeTitle.InnerText, @"\t|\n|\r", String.Empty);
                        cleanTitle = cleanTitle.Trim();
                        cleanTitle = Regex.Replace(cleanTitle, @"&#133;", "...");
                        PurchAlsoBoughtTitles.Add(cleanTitle);
                    }
                    else
                    {
                        PurchAlsoBoughtTitles.Add(nodeTitle.GetAttributeValue("title", ""));
                    }

                    PurchAlsoBoughtAsinNumbers.Add(item.SelectSingleNode(".//div").GetAttributeValue("data-asin", ""));
                    PurchAlsoBoughtAuthorNames.Add(item.SelectSingleNode(".//div/div").InnerText.Trim());
                }
            }
            catch (Exception ex)
            {
                main.Log("An error occurred parsing the book's amazon page: " + ex.Message);
                return;
            }

            SetPaths();
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
                writer.WriteAttributeString("guid", bookInfo.databasename + ":" + bookInfo.guid);
                writer.WriteAttributeString("key", bookInfo.asin);
                writer.WriteAttributeString("type", "EBOK");
                writer.WriteAttributeString("timestamp", dt + tz);
                writer.WriteElementString("treatment", "d");
                writer.WriteStartElement("currentBook");
                writer.WriteElementString("imageUrl", bookImageUrl);
                writer.WriteElementString("asin", bookInfo.asin);
                writer.WriteElementString("hasSample", "false");
                writer.WriteEndElement();
                writer.WriteStartElement("customerProfile");
                writer.WriteElementString("penName", settings.penName);
                writer.WriteElementString("realName", settings.realName);
                writer.WriteEndElement();
                writer.WriteStartElement("recs");
                writer.WriteAttributeString("type", "author");
                for (int i = 0; i < Math.Min(authorProfile.AuthorsOtherBookNames.Count - 1, 5); i++)
                {
                    writer.WriteStartElement("rec");
                    writer.WriteAttributeString("hasSample", "false");
                    writer.WriteAttributeString("asin", authorProfile.AuthorsOtherBookAsins[i]);
                    writer.WriteElementString("title", authorProfile.AuthorsOtherBookNames[i]);
                    writer.WriteElementString("author", bookInfo.author);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("recs");
                writer.WriteAttributeString("type", "purchase");
                for (int i = 0; i < Math.Min(PurchAlsoBoughtTitles.Count - 1, 5); i++)
                {
                    writer.WriteStartElement("rec");
                    writer.WriteAttributeString("hasSample", "false");
                    writer.WriteAttributeString("asin", PurchAlsoBoughtAsinNumbers[i]);
                    writer.WriteElementString("title", PurchAlsoBoughtTitles[i]);
                    writer.WriteElementString("author", PurchAlsoBoughtAuthorNames[i]);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteElementString("booksMentionedPosition", "2");
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
                main.Log("End Action file created successfully!\r\nSaved to " + EaPath);
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

        }

        private void SetPaths()
        {
            string outputDir;
            try
            {
                outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectory(bookInfo.author, bookInfo.sidecarName) : settings.outDir;
            }
            catch (Exception ex)
            {
                main.Log("Failed to create output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            EaDest = settings.docDir + @"\" + bookInfo.author + @"\" + bookInfo.title + @".sdr" + @"\EndActions.data." + bookInfo.asin + ".asc";
            EaPath = outputDir + @"\EndActions.data." + bookInfo.asin + ".asc";

            if (!XRayBuilderGUI.Properties.Settings.Default.overwrite && File.Exists(EaPath))
            {
                main.Log("EndActions file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return;
            }
        }
    }
}
