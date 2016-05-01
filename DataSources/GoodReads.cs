using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;

namespace XRayBuilderGUI.DataSources
{
    public class GoodReads : DataSource
    {
        public override string Name { get { return "GoodReads"; } }

        public override string SearchBook(string author, string title, Action<string> Log)
        {
            string goodreadsSearchUrlBase = @"http://www.goodreads.com/search?q={0}%20{1}";
            string goodreadsBookUrl = "";
            author = Functions.FixAuthor(author);

            HtmlAgilityPack.HtmlDocument goodreadsHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            goodreadsHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(String.Format(goodreadsSearchUrlBase, author, title)));
            if (goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
            {
                author = Functions.TrimAuthor(author);
                goodreadsHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(String.Format(goodreadsSearchUrlBase, author, title)));
            }
            if (!goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
            {
                goodreadsBookUrl = FindGoodreadsURL(goodreadsHtmlDoc, author, title);
                if (goodreadsBookUrl != "")
                {
                    return goodreadsBookUrl;
                }
            }
            return "";
        }

        private string FindGoodreadsURL(HtmlDocument goodreadsHtmlDoc, string author, string title)
        {
            string goodreadsBookUrl = @"http://www.goodreads.com/book/show/{0}";
            //Check if results contain title and author
            foreach (HtmlNode link in goodreadsHtmlDoc.DocumentNode.SelectNodes("//tr[@itemtype='http://schema.org/Book']"))
            {
                HtmlNode titleNode = link.SelectSingleNode(".//a[@class='bookTitle']");
                HtmlNode authorNode = link.SelectSingleNode(".//a[@class='authorName']");
                if (titleNode.InnerText.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (authorNode.InnerText.IndexOf(author, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    HtmlNode node = link.SelectSingleNode(".//a[@class='bookTitle']");
                    //Parse goodreads ID
                    Match match = Regex.Match(node.OuterHtml, @"./book/show/([0-9]*)");
                    if (match.Success)
                        return String.Format(goodreadsBookUrl, match.Groups[1].Value);
                }
            }
            return "";
        }

        /// <summary>
        /// Searches for the next and previous books in a series, if it is part of one.
        /// Modifies curBook.previousInSeries to contain the found book info.
        /// </summary>
        /// <returns>Next book in series</returns>
        public override BookInfo GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, Action<string> Log)
        {
            BookInfo nextBook = null;

            if (curBook.dataUrl == "") return null;
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(curBook.dataUrl));
            }

            // Get title of next book
            Dictionary<string, string> seriesInfo = GetNextInSeriesTitle(curBook, Log);
            string title;
            if (seriesInfo.TryGetValue("Next", out title))
            {
                // Search author's other books for the book (assumes next in series was written by the same author...)
                // Returns the first one found, though there should probably not be more than 1 of the same name anyway
                nextBook = authorProfile.otherBooks.FirstOrDefault(bk => bk.title == title);
                if (nextBook == null)
                {
                    // Attempt to search Amazon for the book instead
                    nextBook = Functions.AmazonSearchBook(title, curBook.author);
                    if (nextBook != null)
                        nextBook.GetAmazonInfo(nextBook.amazonUrl); //fill in desc, imageurl, and ratings
                }
                if (nextBook == null)
                    Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n" +
                        "Please report this book and the GoodReads URL and output log to improve parsing.");

            }
            else if (curBook.seriesPosition != curBook.totalInSeries)
                Log("An error occurred finding the next book in series, the book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out title))
            {
                if (curBook.previousInSeries == null)
                {
                    // Attempt to search Amazon for the book
                    curBook.previousInSeries = Functions.AmazonSearchBook(title, curBook.author);
                    if (curBook.previousInSeries != null)
                        curBook.previousInSeries.GetAmazonInfo(curBook.previousInSeries.amazonUrl); //fill in desc, imageurl, and ratings
                }
                else
                    Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n" +
                        "Please report this book and the Goodreads URL and output log to improve parsing.");
            }
            return nextBook;
        }

        /// <summary>
        /// Search Goodread for possible series info, returning the next title in the series.
        /// Modifies curBook.
        /// </summary>
        private Dictionary<string, string> GetNextInSeriesTitle(BookInfo curBook, Action<string> Log)
        {
            Match match;
            Dictionary<string, string> results = new Dictionary<string, string>(2);
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(curBook.dataUrl));
            }
            //Use Goodreads reviews and ratings to generate popular passages dummy
            int highlights = 0;
            HtmlNode metaNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
            if (metaNode != null)
            {
                HtmlNode passagesNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']");
                match = Regex.Match(passagesNode.InnerText, @"(\d+,\d+)|(\d+)");
                if (match.Success)
                {
                    int passages = int.Parse(match.Value, NumberStyles.AllowThousands);
                    if (passages > 10000)
                        passages = passages / 100;
                    if (passages > 200)
                        passages = passages / 10;
                    curBook.popularPassages = passages.ToString();
                }
                HtmlNode highlightsNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite' and @href='#other_reviews']");
                match = Regex.Match(highlightsNode.InnerText, @"(\d+,\d+)|(\d+)");
                if (match.Success)
                {
                    highlights = int.Parse(match.Value, NumberStyles.AllowThousands);
                    if (highlights > 10000)
                        highlights = highlights / 100;
                    if (highlights > 200)
                        highlights = highlights / 10;
                    curBook.popularHighlights = highlights.ToString();
                }
                string textPassages = curBook.popularPassages == "1"
                    ? String.Format("{0} passage has", curBook.popularPassages)
                    : String.Format("{0} passages have", curBook.popularPassages);
                string textHighlights = curBook.popularHighlights == "1"
                    ? String.Format("{0} time", curBook.popularHighlights)
                    : String.Format("{0} times", curBook.popularHighlights);

                Log(String.Format("{0} been highlighted {1}" , textPassages, textHighlights));
            }
            if (highlights == 0)
            {
                Log("No highlighted passages have been found for this book");
                curBook.popularPassages = "";
                curBook.popularHighlights = "";
            }

            //Search Goodreads for series info
            string goodreadsSeriesUrl = @"http://www.goodreads.com/series/{0}";
            HtmlNode SeriesNode = metaNode.SelectSingleNode("//h1[@id='bookTitle']");
            match = Regex.Match(SeriesNode.OuterHtml, @"/series/([0-9]*)");
            if (!match.Success)
                return results;
            goodreadsSeriesUrl = String.Format(goodreadsSeriesUrl, match.Groups[1].Value);
            match = Regex.Match(SeriesNode.InnerText, @"\((.*) #([0-9]*)\)");
            if (match.Success)
            {
                curBook.seriesName = match.Groups[1].Value.Trim();
                curBook.seriesPosition = match.Groups[2].Value.Trim();
            }

            HtmlDocument seriesHtmlDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
            seriesHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(goodreadsSeriesUrl));
            if (seriesHtmlDoc != null)
            {
                SeriesNode = seriesHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='greyText']");
                match = Regex.Match(SeriesNode.InnerText, @"([0-9]*) primary works");
                if (match.Success)
                {
                    curBook.totalInSeries = match.Groups[1].Value;
                }
                if (int.Parse(curBook.seriesPosition) == 1)
                {
                    Log(String.Format("This is the first book in the {0} series", curBook.seriesName));
                }
                if (int.Parse(curBook.seriesPosition) == int.Parse(curBook.totalInSeries))
                {
                    Log(String.Format("This is the latest book in the {0} series", curBook.seriesName));
                }
                if (int.Parse(curBook.seriesPosition) < int.Parse(curBook.totalInSeries))
                    Log(String.Format("This is book {0} of {1} in the {2} series",
                            curBook.seriesPosition, curBook.totalInSeries, curBook.seriesName));
                if (int.Parse(curBook.seriesPosition) > 1)
                {
                    string stringSearch = String.Format(@"'#{0}'", int.Parse(curBook.seriesPosition) - 1);
                    HtmlNode previousBookNode = seriesHtmlDoc.DocumentNode.SelectSingleNode("//a[@class='bookTitle']/span[contains(., "
                        + stringSearch + ")]/text()");
                    match = Regex.Match(previousBookNode.InnerText, @"(.*) \(.*#[0-9]*\)");
                    if (match.Success)
                    {
                        results["Previous"] = match.Groups[1].Value.Trim();
                        Log(String.Format("Preceded by: {0}", match.Groups[1].Value.Trim()));
                    }
                }
                if (int.Parse(curBook.seriesPosition) < int.Parse(curBook.totalInSeries))
                {
                    string stringSearch = String.Format(@"'#{0}'", int.Parse(curBook.seriesPosition) + 1);
                    HtmlNode nextBookNode = seriesHtmlDoc.DocumentNode.SelectSingleNode("//a[@class='bookTitle']/span[contains(., "
                        + stringSearch + ")]/text()");
                    match = Regex.Match(nextBookNode.InnerText, @"(.*) \(.*#[0-9]*\)");
                    if (match.Success)
                    {
                        Log(String.Format("Followed by: {0}", match.Groups[1].Value.Trim()));
                        results["Next"] = match.Groups[1].Value.Trim();
                    }
                }
            }
            return results;
        }

        public override bool GetPageCount(BookInfo curBook, Action<string> Log)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(curBook.dataUrl));
            }
            HtmlNode pagesNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='details']");
            if (pagesNode == null)
                return false;
            Match match = Regex.Match(pagesNode.InnerText, @"((\d+)|(\d+,\d+)) pages");
            if (match.Success)
            {
                double minutes = int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                TimeSpan span = TimeSpan.FromMinutes(minutes);
                Log(String.Format("Typical time to read: {0} hours and {1} minutes ({2} pages)", span.Hours, span.Minutes, match.Groups[1].Value));
                curBook.pagesInBook = match.Groups[1].Value;
                curBook.readingHours = span.Hours.ToString();
                curBook.readingMinutes = span.Minutes.ToString();
                return true;
            }
            return false;
        }
    }
}
