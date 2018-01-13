using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    public class BookInfo
    {
        public string title;
        public string author;
        public string asin;
        public string guid;
        public string databasename;
        public string path;
        public string sidecarName;
        public string desc = "";
        public string bookImageUrl = "";
        private System.Drawing.Bitmap _bookImage;
        public float amazonRating;
        public int numReviews;
        public string dataUrl = "";
        public string amazonUrl = "";
        public string rawmlPath = "";

        public string authorAsin = "";
        public string authorImageUrl = "";
        public string goodreadsID = "";
        public string editions = "";

        // Added StartAction info
        public string seriesName = "";
        public string seriesPosition = "";
        public string totalInSeries = "";
        public string readingHours = "";
        public string readingMinutes = "";
        public string pagesInBook = "";
        public BookInfo nextInSeries;
        public BookInfo previousInSeries;

        // List of clips and their highlight/like count
        public List<Tuple<string, int>> notableClips;

        public BookInfo(string title, string author, string asin, string guid, string databasename, string path, string sidecarName, string dataUrl, string rawmlPath)
        {
            this.title = title;
            this.author = author;
            this.asin = asin;
            this.guid = guid;
            this.databasename = databasename;
            this.path = path;
            this.sidecarName = sidecarName;
            this.dataUrl = dataUrl;
            this.rawmlPath = rawmlPath;
        }

        public BookInfo(string title, string author, string asin)
        {
            this.title = title;
            this.author = author;
            this.asin = asin;
        }

        public string ToJSON(string nClass, bool includeDescRatings)
        {
            string template = String.Format(@"{{""class"":""{0}"",""asin"":""{1}"",""title"":""{2}"",""authors"":[""{3}""],""imageUrl"":""{4}"",""hasSample"":false",
                                            nClass, asin, title, author, bookImageUrl);
            if (includeDescRatings)
                template += String.Format(@",""description"":""{0}"",""amazonRating"":{1},""numberOfReviews"":{2}", desc, amazonRating, numReviews);
            template += "}";
            return Functions.ExpandUnicode(template);
        }

        public string ToExtraJSON(string nClass)
        {
            string template = String.Format(@"{{""class"":""{0}"",""asin"":""{1}"",""title"":""{2}"",""description"":""{3}"",""authors"":[""{4}""],""imageUrl"":""{5}"",""hasSample"":false,""amazonRating"":{6},""numberOfReviews"":{7}}}",
                nClass, asin, title, desc, author, bookImageUrl, amazonRating, numReviews);
            return Functions.ExpandUnicode(template);
        }


        public override string ToString()
        {
            return title + " - " + author;
        }

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon URL.
        /// </summary>
        /// <param name="amazonUrl">Book's Amazon URL</param>
        public async Task GetAmazonInfo(string amazonUrl)
        {
            if (amazonUrl == "") return;
            HtmlDocument bookDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
            bookDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(amazonUrl));
            GetAmazonInfo(bookDoc);
        }

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon page.
        /// </summary>
        /// <param name="bookDoc">Book's Amazon page, pre-downloaded</param>
        public void GetAmazonInfo(HtmlDocument bookDoc)
        {
            if (bookImageUrl == "")
            {
                // Parse Book image URL
                HtmlNode bookImageLoc = bookDoc.DocumentNode.SelectSingleNode("//*[@id='imgBlkFront']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='imageBlock']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='series-detail-product-image']")
                    ?? bookDoc.DocumentNode.SelectSingleNode("//*[@id='ebooksImgBlkFront']"); //co.uk seems to use this id sometimes
                if (bookImageLoc == null)
                    throw new HtmlWebException(String.Format(@"Error finding book image. If you want, you can report the book's Amazon URL to help with parsing.\r\n{0}", amazonUrl));
                else
                    bookImageUrl = Regex.Replace(bookImageLoc.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);
                if (bookImageUrl.Contains("base64"))
                {
                    bookImageUrl = bookImageLoc.GetAttributeValue("data-a-dynamic-image", "");
                    Match match = Regex.Match(bookImageUrl, @"(https://.*?_\.(jpg|jpeg|gif|png))");
                    if (match.Success)
                    {
                        bookImageUrl = match.Groups[1].Value;
                        if (!bookImageUrl.EndsWith(".png"))
                            bookImageUrl = Regex.Replace(bookImageUrl, @"_.*?_\.", string.Empty);
                    }
                }

                // cleanup to match retail file image links
                if (bookImageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                    bookImageUrl = bookImageUrl.Replace(@"https://images-na.ssl-images-amazon", @"http://ecx.images-amazon");
                
                // Use no image URL
                if (bookImageUrl == "")
                    bookImageUrl = "http://ecx.images-amazon.com/images/G/01/x-site/icons/no-img-sm.gif";
            }
            if (desc == "")
            {
                HtmlNode descNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='bookDescription_feature_div']/noscript");
                if (descNode == null)
                    descNode = bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-size-medium series-detail-description-text']");
                if (descNode != null && descNode.InnerText != "")
                {
                    desc = descNode.InnerText.Trim();
                    // Following the example of Amazon, cut off desc around 1000 characters.
                    // If conveniently trimmed at the end of the sentence, let it end with the punctuation.
                    // If the sentence continues, cut it off and replace the space with an ellipsis
                    if (desc.Length > 1000)
                    {
                        desc = desc.Substring(0, 1000);
                        int lastPunc = desc.LastIndexOfAny(new [] {'.', '!', '?'});
                        int lastSpace = desc.LastIndexOf(' ');
                        if (lastPunc > lastSpace)
                            desc = desc.Substring(0, lastPunc + 1);
                        else
                            desc = desc.Substring(0, lastSpace) + '\u2026';
                    }
                    desc = System.Net.WebUtility.HtmlDecode(desc);
                    desc = desc.CleanString();
                }
            }
            if (numReviews == 0)
            {
                try
                {
                    HtmlNode ratingNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrPopover']");
                    if (ratingNode == null)
                        ratingNode = bookDoc.DocumentNode.SelectSingleNode("//*[@class='fl acrStars']/span");
                    if (ratingNode != null)
                    {
                        string aRating = ratingNode.GetAttributeValue("title", "0");
                        amazonRating = float.Parse(ratingNode.GetAttributeValue("title", "0").Substring(0, aRating.IndexOf(' ')));
                        HtmlNode reviewsNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrCustomerReviewText']")
                            ?? bookDoc.DocumentNode.SelectSingleNode("//*[@class='a-link-normal']");
                        if (reviewsNode != null)
                        {
                            Match match = Regex.Match(reviewsNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                            if (match.Success)
                                numReviews = int.Parse(match.Value.Replace(".", "").Replace(",", ""));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HtmlWebException("Error finding book ratings. If you want, you can report the book's Amazon URL to help with parsing.\r\n" +
                        "Error: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        public System.Drawing.Bitmap CoverImage()
        {
            if (bookImageUrl == "") return null;
            try
            {
                _bookImage = Task.Run(() => HttpDownloader.GetImage(bookImageUrl)).Result;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to download cover image: " + ex.Message);
            }
            return _bookImage;
        }
    }
}
