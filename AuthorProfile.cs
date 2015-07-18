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

using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    public class AuthorProfile
    {
        private Properties.Settings settings = XRayBuilderGUI.Properties.Settings.Default;
        private frmMain main;

        private string ApPath = "";
        private string ApDest = "";

        public string ApTitle = null;
        public Image ApAuthorImage = null;
        public string ApSubTitle = null;
        public string BioTrimmed = "";
        public List<string> AuthorsOtherBookAsins = new List<string>();
        public List<string> AuthorsOtherBookList = new List<string>();
        public List<string> AuthorsOtherBookNames = new List<string>();
        public HtmlDocument authorHtmlDoc = null;

        public string EaSubTitle = null;

        public AuthorProfile(BookInfo book, frmMain frm)
        {
            this.main = frm;
            string outputDir;
            try
            {
                outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectory(book.author, book.sidecarName) : settings.outDir;
            }
            catch (Exception ex)
            {
                main.Log("Failed to create output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            ApPath = outputDir + @"\AuthorProfile.profile." + book.asin + ".asc";

            if (!XRayBuilderGUI.Properties.Settings.Default.overwrite && File.Exists(ApPath))
            {
                main.Log("AuthorProfile file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return;
            }
            ApDest = settings.docDir + @"\" + book.author + @"\" + book.title + @".sdr" + @"\AuthorProfile.profile." + book.asin + ".asc";

            //Process GUID. If in decimal form, convert to hex.
            if (Regex.IsMatch(book.guid, "/[a-zA-Z]/"))
                book.guid = book.guid.ToUpper();
            else
            {
                long guidDec;
                long.TryParse(book.guid, out guidDec);
                book.guid = guidDec.ToString("X");
            }
            if (book.guid == "0")
            {
                main.Log("Something bad happened while converting the GUID.");
                return;
            }

            //Generate Author search URL from author's name
            if (book.author.IndexOf(';') > 0)
                book.author = book.author.Split(';')[0];
            if (book.author.IndexOf(',') > 0)
            {
                string[] parts = book.author.Split(',');
                book.author = parts[1].Trim() + " " + parts[0].Trim();
            }
            string percAuthorName = book.author.Replace(" ", "%20");
            string dashAuthorName = book.author.Replace(" ", "-");
            string plusAuthorName = book.author.Replace(" ", "+");
            string amazonAuthorSearchUrl = @"http://www.amazon.com/s/?url=search-alias%3Dstripbooks&field-keywords=" +
                                        plusAuthorName;
            main.Log("Searching for Author's page on Amazon...");

            // Search Amazon for Author
            HtmlDocument authorHtmlDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
            HttpDownloader client = new HttpDownloader(amazonAuthorSearchUrl);
            string authorsearchHtml = client.GetPage();
            authorHtmlDoc.LoadHtml(authorsearchHtml);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.authorsearchHtml.txt", book.asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving authorsearchHtml.txt: {0}", ex.Message));
                    return;
                }
            }

            // Try to find Author's page from Amazon search
            HtmlNode node = authorHtmlDoc.DocumentNode.SelectSingleNode("//*[@id='result_1']");
            if (node == null || !node.OuterHtml.Contains("/e/B"))
            {
                main.Log("Could not find Author's page on Amazon.\r\nUnable to create Author Profile.\r\nEnsure the author metadata field matches the author's name exactly.\r\nSearch results can be viewed at " + amazonAuthorSearchUrl);
                return;
            }
            string authorAsin = node.OuterHtml;
            int index1 = authorAsin.IndexOf("data-asin");
            if (index1 > 0)
                authorAsin = authorAsin.Substring(index1 + 11, 10);
            
            //var authorAmazonWebsiteLocation = @"http://www.amazon.com/" + dashAuthorName + "/e/" + authorAsin;
            node = node.SelectSingleNode("//*[@id='result_1']/div/div/div/div/a");
            string properAuthor = node.GetAttributeValue("href", "not found");
            if (properAuthor == "not found" || properAuthor.IndexOf('/', 1) < 3)
            {
                main.Log("Found author's page, but could not parse URL properly. Report this URL on the MobileRead thread: " + amazonAuthorSearchUrl);
                return;
            }
            properAuthor = properAuthor.Substring(1, properAuthor.IndexOf('/', 1) - 1);
            string authorAmazonWebsiteLocationLog = @"http://www.amazon.com/" + properAuthor + "/e/" + authorAsin;
            string authorAmazonWebsiteLocation = @"http://www.amazon.com/" + properAuthor + "/e/" + authorAsin +
                                              "/ref=la_" + authorAsin +
                                              "_rf_p_n_feature_browse-b_2?fst=as%3Aoff&rh=n%3A283155%2Cp_82%3A" +
                                              authorAsin +
                                              "%2Cp_n_feature_browse-bin%3A618073011&bbn=283155&ie=UTF8&qid=1432378570&rnid=618072011";

            //http://www.amazon.com/{0}/e/{1}/ref=la_{1}_rf_p_n_feature_browse-b_2?fst=as%3Aoff&rh=n%3A283155%2Cp_82%3A{1}%2Cp_n_feature_browse-bin%3A618073011&bbn=283155&ie=UTF8&qid=1432378570&rnid=618072011
            main.Log("Author page found on Amazon!");
            main.Log(String.Format("Author's Amazon Page URL: {0}", authorAmazonWebsiteLocationLog));

            // Load Author's Amazon page
            string authorpageHtml = HttpDownloader.GetPageHtml(authorAmazonWebsiteLocation);
            authorHtmlDoc.LoadHtml(authorpageHtml);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.authorpageHtml.txt", book.asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving authorpageHtml.txt: {0}", ex.Message));
                    return;
                }
            }

            // Try to find Author's Biography
            HtmlNode bio = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-bio' and @class='a-row']/div/div/span");
            //Trim authour biography to less than 1000 characters and/or replace more problematic characters.
            if (bio.InnerText.Trim().Length != 0)
            {
                    if (bio.InnerHtml.Length > 1000)
                    {
                        string bioTrim = bio.InnerHtml.Substring(0, 1000);
                        BioTrimmed = bioTrim.Substring(0, bioTrim.LastIndexOf(".") + 1);
                    }
                BioTrimmed = BioTrimmed.Replace("\"", "'");
                BioTrimmed = BioTrimmed.Replace("<br><br>", " ");
                BioTrimmed = BioTrimmed.Replace("&amp;#133;", "...");
                BioTrimmed = BioTrimmed.Replace("&#169;", "©");
                BioTrimmed = BioTrimmed.Replace("&quot;", "'");
                BioTrimmed = Regex.Replace(BioTrimmed, @"\s+", " ", RegexOptions.IgnoreCase);
                main.Log("Author biography found on Amazon!");
                main.Log("Attempting to create Author Profile...");
            }
            else
            {
                main.Log("No Author biography found on Amazon!");
            }

            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"\t", " ", RegexOptions.IgnoreCase);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @" <br>", " ", RegexOptions.IgnoreCase);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"<br>", " ", RegexOptions.IgnoreCase);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"&#169;", "©", RegexOptions.None);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"\s+", " ", RegexOptions.Multiline);
            //bio.InnerHtml = Regex.Replace(bio.InnerHtml, @"&amp;#133;", "...", RegexOptions.Multiline);

            // Try to download Author image
            HtmlNode imageXpath = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-image']/img");
            //Full size image (overkill?)
            //var imageUrl = Regex.Replace(imageXpath.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);
            string imageUrl = imageXpath.GetAttributeValue("src", "");
            string downloadedAuthorImage = book.path + @"\DownloadedAuthorImage.jpg";
            try
            {
                using (WebClient webClient = new WebClient())
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
            Bitmap o = (Bitmap) Image.FromFile(downloadedAuthorImage);
            Bitmap nb = new Bitmap(o, o.Width, o.Height);

            int sourceWidth = o.Width;
            int sourceHeight = o.Height;
            float nPercent;
            float nPercentW = (185/(float) sourceWidth);
            float nPercentH = (278/(float) sourceHeight);

            //nPercent = nPercentH > nPercentW ? nPercentH : nPercentW;
            if (nPercentH > nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int) (sourceWidth*nPercent);
            int destHeight = (int) (sourceHeight*nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;

            ImageAttributes ia = new ImageAttributes();
            ia.SetWrapMode(WrapMode.TileFlipXY);

            g.DrawImage(nb, 0, 0, destWidth, destHeight);
            b.Save(book.path + @"\ResizedAuthorImage.jpg");
            b.Dispose();
            g.Dispose();
            o.Dispose();
            nb.Dispose();

            Bitmap target = new Bitmap(185, destHeight);
            Rectangle cropRect = new Rectangle(((destWidth - 185)/2), 0, 185, destHeight);
            using (g = Graphics.FromImage(target))
            {
                g.DrawImage(Image.FromFile(book.path + @"\ResizedAuthorImage.jpg"),
                    new Rectangle(0, 0, target.Width, target.Height),
                    cropRect, GraphicsUnit.Pixel);
            }
            target.Save(book.path + @"\CroppedAuthorImage.jpg");
            target.Dispose();
            g.Dispose();
            Bitmap bc = new Bitmap(book.path + @"\CroppedAuthorImage.jpg");

            //Convert Author image to Grayscale and save as jpeg
            Bitmap bgs = Functions.MakeGrayscale3(bc);

            ImageCodecInfo[] availableCodecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpgCodec = availableCodecs.FirstOrDefault(codec => codec.MimeType == "image/jpeg");
            if (jpgCodec == null)
                throw new NotSupportedException("Encoder for JPEG not found.");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.ColorDepth, 8L);
            bgs.Save(book.path + @"\FinalImage.jpg", jpgCodec, encoderParams);
            //190
            int authorImageHeight = bgs.Height;
            bc.Dispose();

            //Convert final grayscale Author image to Base64 Format String
            string base64ImageString = Functions.ImageToBase64(bgs, ImageFormat.Jpeg);
            main.Log("Grayscale Base-64 encoded Author image created!");
            bgs.Dispose();

            main.Log("Writing Author Profile to file...");
            //Parse Authors other Kindle titles names.
            HtmlNodeCollection authorsOtherBooksTitle =
                authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='mainResults']/div/div/h3/a/@href");
            foreach (HtmlNode otherBook in authorsOtherBooksTitle)
                AuthorsOtherBookNames.Add(otherBook.InnerText);
            //Parse Authors other Kindle titles ASINs.
            HtmlNodeCollection authorsOtherBooksAsin = authorHtmlDoc.DocumentNode.SelectNodes("//*[@class='tpType']");
            foreach (HtmlNode otherBook in authorsOtherBooksAsin)
            {
                int index = otherBook.OuterHtml.IndexOf("/dp/B");
                if (index != -1)
                {
                    AuthorsOtherBookAsins.Add(otherBook.OuterHtml.Substring(index + 4, 10));
                }
            }

            //Create list of Asin numbers and titles
            //var authorsOtherBookList = new List<string>();
            //List<String> controlsToChange = new List<String> { "lblPreviewBook1", "lblPreviewBook2","lblPreviewBook3", "lblPreviewBook4" };

            for (int i = 0; i < AuthorsOtherBookNames.Count; i++)
            {
                AuthorsOtherBookList.Add(string.Format(@"{{""e"":1,""a"":""{0}"",""t"":""{1}""}}",
                    AuthorsOtherBookAsins[i], AuthorsOtherBookNames[i]));
            }

            //Create finalAuthorProfile.profile.ASIN.asc
            int unixTimestamp = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            try
            {
                string authorProfileOutput = @"{""u"":[{""y"":" + authorImageHeight + @",""l"":[""" +
                                          string.Join(@""",""", AuthorsOtherBookAsins.ToArray()) + @"""],""n"":""" +
                                          book.author + @""",""a"":""" + authorAsin + @""",""b"":""" + BioTrimmed +
                                          @""",""i"":""" + base64ImageString + @"""}],""a"":""" +
                                          String.Format(@"{0}"",""d"":{1},""o"":[", book.asin, unixTimestamp) +
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

            ApTitle = "About " + book.author;
            ApSubTitle = "Kindle books by " + book.author;
            ApAuthorImage = Image.FromFile(book.path + @"\FinalImage.jpg");
            EaSubTitle = "More books by " + book.author;

            
            /*try
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
            }*/
        }
    }
}