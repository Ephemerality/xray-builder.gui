using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI
{
    public class AuthorProfile
    {
        private Properties.Settings settings = XRayBuilderGUI.Properties.Settings.Default;
        private frmMain main;

        private string OutFolder = "";
        private string ApPath = "";
        private string EaPath = "";
        private string ApDest = "";
        private string EaDest = "";
        private bool OverwriteFile;

        public string ApTitle = null;
        public Image ApAuthorImage = null;
        public string ApSubTitle = null;
        public string BioTrimmed = null;
        public List<string> AuthorsOtherBookList = new List<string>();
        public List<string> AuthorsOtherBookNames = new List<string>();
        public List<string> PurchAlsoBoughtTitles = new List<string>();
        public List<string> PurchAlsoBoughtAsinNumbers = new List<string>();
        public List<string> PpurchAlsoBoughtAuthorNames = new List<string>();

        public string EaSubTitle = null;

        public AuthorProfile(string title, string author, string asin, string guid, string databasename, string path,
            frmMain frm)
        {
            this.main = frm;
            if (settings.useSubDirectories)
            {
                OutFolder = Functions.GetBookOutputDirectory(author, title);
                ApPath = OutFolder + @"\AuthorProfile.profile." + asin + ".asc";
                EaPath = OutFolder + @"\EndActions.data." + asin + ".asc";
            }
            else
            {
                ApPath = settings.outDir + @"\AuthorProfile.profile." + asin + ".asc";
                EaPath = settings.outDir + @"\EndActions.data." + asin + ".asc";
            }
            OverwriteFile = XRayBuilderGUI.Properties.Settings.Default.overwrite;
            if (!OverwriteFile &&
                File.Exists(ApPath) &&
                File.Exists(EaPath))
            {
                main.Log("AuthorProfile and EndActions files already exist... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite and existing files.");
                return;
            }
            ApDest = settings.docDir + @"\" + author + @"\" + title + @".sdr" + @"\AuthorProfile.profile." + asin +
                     ".asc";
            EaDest = settings.docDir + @"\" + author + @"\" + title + @".sdr" + @"\EndActions.data." + asin + ".asc";

            //Process GUID. If in decimal form, convert to hex.
            if (Regex.IsMatch(guid, "/[a-zA-Z]/"))
                guid = guid.ToUpper();
            else
            {
                long guidDec;
                long.TryParse(guid, out guidDec);
                guid = guidDec.ToString("X");
            }
            if (guid == "0")
            {
                main.Log("Something bad happened while converting the GUID.");
                return;
            }

            //Generate Author search URL from author's name
            var percAuthorName = Regex.Replace(author, " ", "%20", RegexOptions.IgnoreCase);
            var dashAuthorName = Regex.Replace(author, " ", "-", RegexOptions.IgnoreCase);
            var amazonAuthorSearchUrl = @"http://www.amazon.com/s/?url=search-alias%3Dstripbooks&field-keywords=" +
                                        percAuthorName;
            main.Log("Searching for Author's page on Amazon...");

            // Search Amazon for Author
            var authorHtmlDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
            var authorsearchHtml = Functions.GetPageHtml(amazonAuthorSearchUrl);
            authorHtmlDoc.LoadHtml(authorsearchHtml);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.authorsearchHtml.txt", asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving authorsearchHtml.txt: {0}", ex.Message));
                    return;
                }
            }

            // Try to find Author's page from Amazon search
            var node = authorHtmlDoc.DocumentNode.SelectSingleNode("//*[@id='result_1']");
            if (node == null)
                main.Log("Could not find Author's page on Amazon.\r\nUnable to create Author Profile.");
            var authorAsin = node.OuterHtml;
            var index1 = authorAsin.IndexOf("data-asin");
            if (index1 > 0)
                authorAsin = authorAsin.Substring(index1 + 11, 10);
            //var authorAmazonWebsiteLocation = @"http://www.amazon.com/" + dashAuthorName + "/e/" + authorAsin;
            var authorAmazonWebsiteLocationLog = @"http://www.amazon.com/" + dashAuthorName + "/e/" + authorAsin;
            var authorAmazonWebsiteLocation = @"http://www.amazon.com/" + percAuthorName + "/e/" + authorAsin +
                                              "/ref=la_" + authorAsin +
                                              "_rf_p_n_feature_browse-b_2?fst=as%3Aoff&rh=n%3A283155%2Cp_82%3A" +
                                              authorAsin +
                                              "%2Cp_n_feature_browse-bin%3A618073011&bbn=283155&ie=UTF8&qid=1432378570&rnid=618072011";

            //http://www.amazon.com/{0}/e/{1}/ref=la_{1}_rf_p_n_feature_browse-b_2?fst=as%3Aoff&rh=n%3A283155%2Cp_82%3A{1}%2Cp_n_feature_browse-bin%3A618073011&bbn=283155&ie=UTF8&qid=1432378570&rnid=618072011
            main.Log("Author page found on Amazon!");
            main.Log(String.Format("Author's Amazon Page URL: {0}", authorAmazonWebsiteLocationLog));

            // Load Author's Amazon page
            var authorpageHtml = Functions.GetPageHtml(authorAmazonWebsiteLocation);
            authorHtmlDoc.LoadHtml(authorpageHtml);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.authorpageHtml.txt", asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving authorpageHtml.txt: {0}", ex.Message));
                    return;
                }
            }

            // Try to find Author's Biography
            var bio = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-bio' and @class='a-row']/div/div/span");
            //Trim authour biography to less than 1500 characters and/or replace " -> '
            if (bio.InnerText.Trim().Length != 0)
            {
                if (bio.InnerHtml.Length > 1500)
                {
                    var bioTrim = bio.InnerHtml.Substring(0, 1500);
                    BioTrimmed = bioTrim.Substring(0, bioTrim.LastIndexOf(".") + 1);
                    BioTrimmed = Regex.Replace(BioTrimmed, "\"", "'", RegexOptions.None);
                    BioTrimmed = Regex.Replace(BioTrimmed, @"<br><br>", " ", RegexOptions.IgnoreCase);
                    main.Log("Author biography found on Amazon!");
                }
                else
                {
                    BioTrimmed = Regex.Replace(bio.InnerHtml, "\"", "'", RegexOptions.None);
                    BioTrimmed = Regex.Replace(BioTrimmed, @"<br><br>", " ", RegexOptions.IgnoreCase);
                    main.Log("Author biography found on Amazon!");
                }
                main.Log("Attempting to create Author Profile...");
            }
            else
            {
                main.Log("No Author biography found on Amazon!\r\nUnable to create Author Profile.");
            }

            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"\t", " ", RegexOptions.IgnoreCase);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @" <br>", " ", RegexOptions.IgnoreCase);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"<br>", " ", RegexOptions.IgnoreCase);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"&#169;", "©", RegexOptions.None);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"\s+", " ", RegexOptions.Multiline);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"&amp;#133;", "...", RegexOptions.Multiline);

            // Try to download Author image
            var imageXpath = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-image']/img");
            //Full size image (overkill?)
            //var imageUrl = Regex.Replace(imageXpath.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);
            var imageUrl = imageXpath.GetAttributeValue("src", "");
            var downloadedAuthorImage = path + @"\DownloadedAuthorImage.jpg";
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(imageUrl), downloadedAuthorImage);
                    webClient.Dispose();
                    main.Log("Downloading Author image...");
                }
            }
            catch (Exception ex)
            {
                main.Log(String.Format("Failed to download Author image: {0}", ex.Message));
                return;
            }

            main.Log("Resizing and cropping Author image...");
            //Resize and Crop Author image
            var o = (Bitmap) Image.FromFile(downloadedAuthorImage);
            var nb = new Bitmap(o, o.Width, o.Height);

            var sourceWidth = o.Width;
            var sourceHeight = o.Height;
            float nPercent;
            var nPercentW = (185/(float) sourceWidth);
            var nPercentH = (278/(float) sourceHeight);

            //nPercent = nPercentH > nPercentW ? nPercentH : nPercentW;
            if (nPercentH > nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            var destWidth = (int) (sourceWidth*nPercent);
            var destHeight = (int) (sourceHeight*nPercent);

            var b = new Bitmap(destWidth, destHeight);
            var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;

            var ia = new ImageAttributes();
            ia.SetWrapMode(WrapMode.TileFlipXY);

            g.DrawImage(nb, 0, 0, destWidth, destHeight);
            b.Save(path + @"\ResizedAuthorImage.jpg");
            b.Dispose();
            g.Dispose();
            o.Dispose();
            nb.Dispose();

            var target = new Bitmap(185, destHeight);
            var cropRect = new Rectangle(((destWidth - 185)/2), 0, 185, destHeight);
            using (g = Graphics.FromImage(target))
            {
                g.DrawImage(Image.FromFile(path + @"\ResizedAuthorImage.jpg"),
                    new Rectangle(0, 0, target.Width, target.Height),
                    cropRect, GraphicsUnit.Pixel);
            }
            target.Save(path + @"\CroppedAuthorImage.jpg");
            target.Dispose();
            g.Dispose();
            var bc = new Bitmap(path + @"\CroppedAuthorImage.jpg");

            //Convert Author image to Grayscale and save as jpeg
            var bgs = Functions.MakeGrayscale3(bc);

            var availableCodecs = ImageCodecInfo.GetImageEncoders();
            var jpgCodec = availableCodecs.FirstOrDefault(codec => codec.MimeType == "image/jpeg");
            if (jpgCodec == null)
                throw new NotSupportedException("Encoder for JPEG not found.");
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.ColorDepth, 8L);
            bgs.Save(path + @"\FinalImage.jpg", jpgCodec, encoderParams);
            //190
            var authorImageHeight = bgs.Height;
            bc.Dispose();

            //Convert final grayscale Author image to Base64 Format String
            var base64ImageString = Functions.ImageToBase64(bgs, ImageFormat.Jpeg);
            main.Log("Grayscale Base-64 encoded Author image created!");
            bgs.Dispose();

            main.Log("Writing Author Profile to file...");
            //Parse Authors other Kindle titles names.
            var authorsOtherBooksTitle =
                authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='mainResults']/div/div/h3/a/@href");
            foreach (var book in authorsOtherBooksTitle)
                AuthorsOtherBookNames.Add(book.InnerText);

            //Parse Authors other Kindle titles ASINs.
            var authorsOtherBookAsins = new List<string>();
            var authorsOtherBooksAsin = authorHtmlDoc.DocumentNode.SelectNodes("//*[@class='tpType']");
            foreach (var book in authorsOtherBooksAsin)
            {
                var index = book.OuterHtml.IndexOf("/dp/B");
                if (index != -1)
                {
                    authorsOtherBookAsins.Add(book.OuterHtml.Substring(index + 4, 10));
                }
            }

            //Create list of Asin numbers and titles
            //var authorsOtherBookList = new List<string>();
            //List<String> controlsToChange = new List<String> { "lblPreviewBook1", "lblPreviewBook2","lblPreviewBook3", "lblPreviewBook4" };

            for (var i = 0; i < AuthorsOtherBookNames.Count; i++)
            {
                AuthorsOtherBookList.Add(string.Format(@"{{""e"":1,""a"":""{0}"",""t"":""{1}""}}",
                    authorsOtherBookAsins[i], AuthorsOtherBookNames[i]));
            }

            //Create finalAuthorProfile.profile.ASIN.asc
            var unixTimestamp = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            try
            {
                var authorProfileOutput = @"{""u"":[{""y"":" + authorImageHeight + @",""l"":[""" +
                                          string.Join(@""",""", authorsOtherBookAsins.ToArray()) + @"""],""n"":""" +
                                          author + @""",""a"":""" + authorAsin + @""",""b"":""" + BioTrimmed +
                                          @""",""i"":""" + base64ImageString + @"""}],""a"":""" +
                                          String.Format(@"{0}"",""d"":{1},""o"":[", asin, unixTimestamp) +
                                          string.Join(",", AuthorsOtherBookList.ToArray()) + "]}";
                File.WriteAllText(ApPath, authorProfileOutput);
                main.btnPreview.Enabled = true;
                main.cmsPreview.Items[0].Enabled = true;
                main.Log("Author Profile file created successfully!\r\nSaved to " + ApPath);
            }
            catch (Exception ex)
            {
                main.Log("An error occurred while writing the Author Profile file: " + ex.Message);
                return;
            }

            ApTitle = "About " + author;
            ApSubTitle = "Kindle books by " + author;
            ApAuthorImage = Image.FromFile(path + @"\FinalImage.jpg");
            EaSubTitle = "More books by " + author;

            main.Log("Attempting to find book on Amazon...");
            //Generate Book search URL from book's ASIN
            var ebookLocation = @"http://www.amazon.com/dp/" + asin;

            // Search Amazon for book
            main.Log("Book found on Amazon!");
            main.Log(String.Format("Book's Amazon page URL: {0}", ebookLocation));
            
            var bookHlmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            bookHlmlDoc.LoadHtml(Functions.GetPageHtml(ebookLocation));

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.bookHlml.txt", asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving bookHtml.txt: {0}", ex.Message));
                    return;
                }
            }

            // Parse Book image URL
            //var bookImageLoc = bookHlmlDoc.DocumentNode.SelectSingleNode("//*[@id='main-image']");
            //var bookImageUrl = Regex.Replace(bookImageLoc.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

            // Generate random book image URL because Amazon keep changing format!
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 11)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            var bookImageUrl = String.Format("http://ecx.images-amazon.com/images/I/{0}.jpg",
                Uri.EscapeDataString(result));

            //Parse Recommended Author titles and ASINs
            try
            {
                var recomendationList = bookHlmlDoc.DocumentNode.SelectNodes("//li[@class='a-carousel-card a-float-left']");
                if (recomendationList == null)
                    main.Log("Could not find related book list page on Amazon.\r\nUnable to create End Actions.");
                foreach (var item in recomendationList.Where(item => item != null))
                {
                    var nodeTitle = item.SelectSingleNode(".//div/a");
                    var nodeTitleCheck = nodeTitle.GetAttributeValue("title", "");
                    if (nodeTitleCheck == "")
                    {
                        nodeTitle = item.SelectSingleNode(".//div/a");
                        PurchAlsoBoughtTitles.Add(Regex.Replace(nodeTitle.InnerText, @"&#133;", "...",
                            RegexOptions.Multiline));
                    }
                    else
                    {
                        PurchAlsoBoughtTitles.Add(nodeTitle.GetAttributeValue("title", ""));
                    }

                    var nodeAsin = item.SelectSingleNode(".//div");
                    PurchAlsoBoughtAsinNumbers.Add(nodeAsin.GetAttributeValue("data-asin", ""));

                    var nodeAuthor = item.SelectSingleNode(".//div/div");
                    PpurchAlsoBoughtAuthorNames.Add(nodeAuthor.InnerText.Trim());
                }
            }
            catch (Exception ex)
            {
                main.Log("An error occurred parsing the book's amazon page: " + ex.Message);
                return;
            }

            //Create final EndActions.data.ASIN.asc
            var dt = DateTime.Now.ToString("s");
            var tz = DateTime.Now.ToString("zzz");
            var writer = new XmlTextWriter(EaPath, null);
            try
            {
                main.Log("Writing End Actions to file...");
                //writer.Formatting = Formatting.Indented;
                //writer.Indentation = 4;
                writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
                writer.WriteStartElement("endaction");
                writer.WriteAttributeString("version", "0");
                writer.WriteAttributeString("guid", databasename + ":" + guid);
                writer.WriteAttributeString("key", asin);
                writer.WriteAttributeString("type", "EBOK");
                writer.WriteAttributeString("timestamp", dt + tz);
                writer.WriteElementString("treatment", "d");
                writer.WriteStartElement("currentBook");
                writer.WriteElementString("imageUrl", bookImageUrl);
                writer.WriteElementString("asin", asin);
                writer.WriteElementString("hasSample", "false");
                writer.WriteEndElement();
                writer.WriteStartElement("customerProfile");
                writer.WriteElementString("penName", settings.penName);
                writer.WriteElementString("realName", settings.realName);
                writer.WriteEndElement();
                writer.WriteStartElement("recs");
                writer.WriteAttributeString("type", "author");
                for (var i = 0; i < 5; i++)
                {
                    writer.WriteStartElement("rec");
                    writer.WriteAttributeString("hasSample", "false");
                    writer.WriteAttributeString("asin", authorsOtherBookAsins[i]);
                    writer.WriteElementString("title", AuthorsOtherBookNames[i]);
                    writer.WriteElementString("author", author);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("recs");
                writer.WriteAttributeString("type", "purchase");
                for (var i = 0; i < 5; i++)
                {
                    writer.WriteStartElement("rec");
                    writer.WriteAttributeString("hasSample", "false");
                    writer.WriteAttributeString("asin", PurchAlsoBoughtAsinNumbers[i]);
                    writer.WriteElementString("title", PurchAlsoBoughtTitles[i]);
                    writer.WriteElementString("author", PpurchAlsoBoughtAuthorNames[i]);
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

            try
            {
                if (settings.sendtoKindle)
                {
                    if (Directory.Exists(settings.docDir))
                    {
                        File.Copy(ApPath, ApDest, true);
                        main.Log("Author Profile file successfully copied to your Kindle!");
                        File.Copy(EaPath, EaDest, true);
                        main.Log("End Action file successfully copied to your Kindle!");
                    }
                    main.Log("Specified Kindle cocuments folder not found. Is your Kindle connected?" +
                             " If so, please review the settings page.");
                }
            }
            catch (Exception ex)
            {
                main.Log("An error occured copying files to your Kindle!\r\nException: " + ex.Message);
            }
        }
    }
}