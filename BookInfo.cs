using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        public float amazonRating = 0.0F;
        public int numReviews = 0;

        public BookInfo(string title, string author, string asin, string guid, string databasename, string path, string sidecarName)
        {
            this.title = title;
            this.author = author;
            this.asin = asin;
            this.guid = guid;
            this.databasename = databasename;
            this.path = path;
            this.sidecarName = sidecarName;
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
            return template;
        }

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon URL.
        /// </summary>
        public void GetAmazonInfo(string amazonUrl)
        {
            if (amazonUrl == "") return;
            HtmlDocument bookDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
            bookDoc.LoadHtml(HttpDownloader.GetPageHtml(amazonUrl));
            GetAmazonInfo(bookDoc);
        }

        /// <summary>
        /// Retrieves the book's description, image URL, and rating from the book's Amazon page.
        /// </summary>
        public void GetAmazonInfo(HtmlDocument bookDoc)
        {
            if (bookImageUrl == "")
            {
                // Parse Book image URL
                HtmlNode bookImageLoc = bookDoc.DocumentNode.SelectSingleNode("//*[@id='imgBlkFront']");
                if (bookImageLoc == null)
                    throw new HtmlWebException("Error finding book image. If you want, you can report the book's Amazon URL to help with parsing.");
                else
                    bookImageUrl = Regex.Replace(bookImageLoc.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

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
            }
            if (desc == "")
            {
                HtmlNode descNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='bookDescription_feature_div']/noscript");
                if (descNode != null && descNode.InnerText != "")
                {
                    desc = descNode.InnerText.Trim();
                    // Following the example of Amazon, cut off desc around 400-410 characters.
                    // If conveniently trimmed at the end of the sentence, let it end with the punctuation.
                    // If the sentence continues, cut it off and replace the space with an ellipsis
                    if (desc.Length > 410)
                    {
                        desc = desc.Substring(0, 410);
                        int lastPunc = desc.LastIndexOfAny(new char[] {'.', '!', '?'});
                        int lastSpace = desc.LastIndexOf(' ');
                        if (lastPunc > lastSpace)
                            desc = desc.Substring(0, lastPunc + 1);
                        else
                            desc = desc.Substring(0, lastSpace) + '\u2026';
                        //Clean up desc the same as biography was... not all of these may be needed?
                        desc = Regex.Replace(desc, @"\t|\n|\r", " ");
                        desc = desc.Replace("\"", "'");
                        desc = desc.Replace("&quot;", "'");
                        desc = desc.Replace("<br><br>", " ");
                        desc = desc.Replace("&amp;#133;", "...");
                        desc = desc.Replace("&#169;", "©");
                        desc = Regex.Replace(desc, @"\s+", " ");
                    }
                }
            }
            if (numReviews == 0)
            {
                try
                {
                    HtmlNode ratingNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrPopover']");
                    string aRating = ratingNode.GetAttributeValue("title", "0");
                    amazonRating = float.Parse(ratingNode.GetAttributeValue("title", "0").Substring(0, aRating.IndexOf(' ')));
                    HtmlNode reviewsNode = bookDoc.DocumentNode.SelectSingleNode("//*[@id='acrCustomerReviewText']");
                    numReviews = int.Parse(reviewsNode.InnerText.Substring(0, reviewsNode.InnerText.IndexOf(' ')).Replace(",", ""));
                }
                catch (Exception ex)
                {
                    throw new HtmlWebException("Error finding book ratings. If you want, you can report the book's Amazon URL to help with parsing.\r\n" +
                        "Error: " + ex.Message);
                }
            }
        }
    }
}
