﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    public class AuthorProfile
    {
        private Properties.Settings settings = Properties.Settings.Default;
        private frmMain main;

        private string ApPath = "";
        private BookInfo curBook = null;

        public string ApTitle = null;
        public Image ApAuthorImage = null;
        public string ApSubTitle = null;
        public string BioTrimmed = "";
        public List<BookInfo> otherBooks = new List<BookInfo>();
        public string authorImageUrl = "";
        public HtmlDocument authorHtmlDoc = null;
        public string authorAsin = "";

        public string EaSubTitle = null;

        public bool complete = false; //Set if constructor succeeded in generating profile

        public AuthorProfile(BookInfo nBook, frmMain frm)
        {
            this.curBook = nBook;
            this.main = frm;
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
                main.Log("Failed to create output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            ApPath = outputDir + @"\AuthorProfile.profile." + curBook.asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(ApPath))
            {
                main.Log("AuthorProfile file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return;
            }
            
            //Process GUID. If in decimal form, convert to hex.
            if (Regex.IsMatch(curBook.guid, "/[a-zA-Z]/"))
                curBook.guid = curBook.guid.ToUpper();
            else
            {
                long guidDec;
                long.TryParse(curBook.guid, out guidDec);
                curBook.guid = guidDec.ToString("X");
            }
            if (curBook.guid == "0")
            {
                main.Log("Something bad happened while converting the GUID.");
                return;
            }

            //Generate Author search URL from author's name
            string newAuthor = Functions.FixAuthor(curBook.author);
            string plusAuthorName = newAuthor.Replace(" ", "+");
            string amazonAuthorSearchUrl = @"http://www.amazon.com/s/?url=search-alias%3Dstripbooks&field-keywords=" +
                                        plusAuthorName;
            main.Log("Searching for author's page on Amazon...");

            // Search Amazon for Author
            HtmlDocument authorHtmlDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
            string authorsearchHtml = HttpDownloader.GetPageHtml(amazonAuthorSearchUrl);
            authorHtmlDoc.LoadHtml(authorsearchHtml);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    main.Log("Saving Amazon's author search webpage...");
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.authorsearchHtml.txt", curBook.asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving authorsearchHtml.txt: {0}", ex.Message));
                }
            }

            // Try to find Author's page from Amazon search
            HtmlNode node = authorHtmlDoc.DocumentNode.SelectSingleNode("//*[@id='result_1']");
            if (node == null || !node.OuterHtml.Contains("/e/B"))
            {
                main.Log("Could not find author's page on Amazon.\r\nUnable to create Author Profile.\r\nEnsure the author metadata field matches the author's name exactly.\r\nSearch results can be viewed at " + amazonAuthorSearchUrl);
                return;
            }
            authorAsin = node.OuterHtml;
            int index1 = authorAsin.IndexOf("data-asin");
            if (index1 > 0)
                authorAsin = authorAsin.Substring(index1 + 11, 10);
            
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

            main.Log("Author page found on Amazon!");
            main.Log(String.Format("Author's Amazon Page URL: {0}", authorAmazonWebsiteLocationLog));

            // Load Author's Amazon page
            string authorpageHtml = HttpDownloader.GetPageHtml(authorAmazonWebsiteLocation);
            authorHtmlDoc.LoadHtml(authorpageHtml);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    main.Log("Saving author's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory +
                                      String.Format(@"\dmp\{0}.authorpageHtml.txt", curBook.asin),
                        authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An error ocurred saving authorpageHtml.txt: {0}", ex.Message));
                }
            }

            // Try to find Author's Biography
            HtmlNode bio = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-bio' and @class='a-row']/div/div/span");
            //Trim authour biography to less than 1000 characters and/or replace more problematic characters.
            if (bio.InnerText.Trim().Length != 0)
            {
                if (bio.InnerText.Length > 1000)
                {
                    int lastPunc = bio.InnerText.LastIndexOfAny(new char[] { '.', '!', '?' });
                    int lastSpace = bio.InnerText.LastIndexOf(' ');
                    if (lastPunc > lastSpace)
                        BioTrimmed = bio.InnerText.Substring(0, lastPunc + 1);
                    else
                        BioTrimmed = bio.InnerText.Substring(0, lastSpace) + '\u2026';
                }
                else
                {
                    BioTrimmed = bio.InnerText;
                }
                BioTrimmed = Functions.CleanString(BioTrimmed);
                main.Log("Author biography found on Amazon!");
            }
            else
            {
                BioTrimmed = "No author biography found on Amazon!";
                main.Log("No author biography found on Amazon!");
            }
            // Try to download Author image
            HtmlNode imageXpath = authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-image']/img");
            authorImageUrl = imageXpath.GetAttributeValue("src", "");
            string downloadedAuthorImage = curBook.path + @"\DownloadedAuthorImage.jpg";
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(authorImageUrl), downloadedAuthorImage);
                    main.Log("Downloading author image...");
                }
            }
            catch (Exception ex)
            {
                main.Log(String.Format("Failed to download author image: {0}", ex.Message));
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

            nPercent = nPercentH > nPercentW ? nPercentH : nPercentW;

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
            b.Save(curBook.path + @"\ResizedAuthorImage.jpg");
            b.Dispose();
            g.Dispose();
            o.Dispose();
            nb.Dispose();

            Bitmap target = new Bitmap(185, destHeight);
            Rectangle cropRect = new Rectangle(((destWidth - 185)/2), 0, 185, destHeight);
            using (g = Graphics.FromImage(target))
            {
                g.DrawImage(Image.FromFile(curBook.path + @"\ResizedAuthorImage.jpg"),
                    new Rectangle(0, 0, target.Width, target.Height),
                    cropRect, GraphicsUnit.Pixel);
            }
            target.Save(curBook.path + @"\CroppedAuthorImage.jpg");
            target.Dispose();
            Bitmap bc = new Bitmap(curBook.path + @"\CroppedAuthorImage.jpg");

            //Convert Author image to Grayscale and save as jpeg
            Bitmap bgs = Functions.MakeGrayscale3(bc);

            ImageCodecInfo[] availableCodecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpgCodec = availableCodecs.FirstOrDefault(codec => codec.MimeType == "image/jpeg");
            if (jpgCodec == null)
                throw new NotSupportedException("Encoder for JPEG not found.");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.ColorDepth, 8L);
            bgs.Save(curBook.path + @"\FinalImage.jpg", jpgCodec, encoderParams);
            int authorImageHeight = bgs.Height;
            bc.Dispose();

            //Convert final grayscale Author image to Base64 Format String
            string base64ImageString = Functions.ImageToBase64(bgs, ImageFormat.Jpeg);
            main.Log("Grayscale Base-64 encoded author image created!");
            bgs.Dispose();

            main.Log("Gathering author's other books...");
            List<BookInfo> bookList = new List<BookInfo>();
            HtmlNodeCollection resultsNodes =
                authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='mainResults']/ul/li");
            foreach (HtmlNode result in resultsNodes)
            {
                if (!result.Id.StartsWith("result_")) continue;
                string name, url, asin = "";
                HtmlNode otherBook = result.SelectSingleNode(".//div[@class='a-row a-spacing-small']/a/h2");
                Match match = Regex.Match(otherBook.InnerText, @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    continue;
                }
                name = otherBook.InnerText;
                otherBook = result.SelectSingleNode(".//*[@title='Kindle Edition']");
                match = Regex.Match(otherBook.OuterHtml, "dp/(B[A-Z0-9]{9})/");
                if (match.Success)
                {
                    asin = match.Groups[1].Value;
                }
                //url = otherBook.GetAttributeValue("href", "");
                //url = otherBook.GetAttributeValue("href", "").
                //    Substring(0, otherBook.GetAttributeValue("href", "").
                //    IndexOf(match.Groups[1].Value) +
                //    match.Groups[1].Length);
                url = String.Format("http://www.amazon.com/dp/{0}", asin);
                if (name != "" && url != "" && asin != "")
                {
                    BookInfo newBook = new BookInfo(name, curBook.author, asin);
                    newBook.amazonUrl = url;
                    bookList.Add(newBook);
                }
            }

            main.Log("Gathering metadata for other books...");
            foreach (BookInfo book in bookList)
            {
                try
                {
                    //Gather book desc, image url, etc, if using new format
                    if (settings.useNewVersion)
                        book.GetAmazonInfo(book.amazonUrl);
                    otherBooks.Add(book);
                }
                catch (Exception ex)
                {
                    main.Log(String.Format("An problem occured gathering metadata for other books: {0}\r\nURL: {1}\r\nBook: {2}\r\nContinuing anyway...", ex.Message, book.amazonUrl, book.title));
                }
            }

            main.Log("Writing Author Profile to file...");

            //Create list of Asin numbers and titles
            List<string> authorsOtherBookList = new List<string>();
            foreach (BookInfo bk in otherBooks)
            {
                authorsOtherBookList.Add(String.Format(@"{{""e"":1,""a"":""{0}"",""t"":""{1}""}}",
                    bk.asin, bk.title));
            }

            //Create finalAuthorProfile.profile.ASIN.asc
            int unixTimestamp = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            try
            {
                string authorProfileOutput = @"{""u"":[{""y"":" + authorImageHeight + @",""l"":[""" +
                                          string.Join(@""",""", otherBooks.Select(book => book.asin).ToArray()) + @"""],""n"":""" +
                                          curBook.author + @""",""a"":""" + authorAsin + @""",""b"":""" + BioTrimmed +
                                          @""",""i"":""" + base64ImageString + @"""}],""a"":""" +
                                          String.Format(@"{0}"",""d"":{1},""o"":[", curBook.asin, unixTimestamp) +
                                          string.Join(",", authorsOtherBookList.ToArray()) + "]}";
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

            ApTitle = "About " + curBook.author;
            ApSubTitle = "Kindle Books By " + curBook.author;
            ApAuthorImage = Image.FromFile(curBook.path + @"\FinalImage.jpg");
            EaSubTitle = "More Books By " + curBook.author;

            complete = true;
        }

        public string ToJSON()
        {
            string template = @"{{""class"":""authorBio"",""asin"":""{0}"",""name"":""{1}"",""bio"":""{2}"",""imageUrl"":""{3}""}}";
            return Functions.ExpandUnicode(String.Format(template, authorAsin, curBook.author, BioTrimmed, authorImageUrl));
        }
    }
}