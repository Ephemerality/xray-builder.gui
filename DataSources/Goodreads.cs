using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI.DataSources
{
    public class Goodreads : DataSource
    {
        public override string Name { get { return "Goodreads"; } }
        private Properties.Settings settings = Properties.Settings.Default;

        public override string SearchBook(string author, string title, Action<string> Log)
        {
            string goodreadsSearchUrlBase = @"http://www.goodreads.com/search?q={0}%20{1}";
            string goodreadsBookUrl = "";
            // Goodreads expects %26 intead of &
            title = title.Replace("&", "%26");
            author = Functions.FixAuthor(author);

            HtmlDocument goodreadsHtmlDoc = new HtmlDocument();
            goodreadsHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(String.Format(goodreadsSearchUrlBase, author, title)));
            if (goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
            {
                author = Functions.TrimAuthor(author);
                goodreadsHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(String.Format(goodreadsSearchUrlBase, author, title)));
            }
            if (!goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
            {
                // Revert back for searching title
                goodreadsBookUrl = FindGoodreadsURL(goodreadsHtmlDoc, author, title.Replace("%26", "&amp;"));
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
                if (authorNode.InnerText.IndexOf(author, StringComparison.OrdinalIgnoreCase) < 0)
                    author = Functions.TrimAuthor(author);
                if (titleNode.InnerText.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (authorNode.InnerText.IndexOf(author, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    HtmlNode node = link.SelectSingleNode(".//a[@class='bookTitle']");
                    //string searchResultTitle = node.InnerText.Trim();
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
        public override BookInfo GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, string TLD, Action<string> Log)
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
                nextBook = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, "^" + title + @"(?: \(.*\))?$"));
                if (nextBook == null)
                {
                    // Attempt to search Amazon for the book instead
                    try
                    {
                        nextBook = Amazon.SearchBook(title, curBook.author, TLD);
                    }
                    catch
                    {
                        Log(String.Format("Failed to find {0} on Amazon." + TLD + ", trying again with Amazon.com.", title));
                        TLD = "com";
                        nextBook = Amazon.SearchBook(title, curBook.author, TLD);
                    }
                    if (nextBook != null)
                        nextBook.GetAmazonInfo(nextBook.amazonUrl); //fill in desc, imageurl, and ratings
                }
                if (nextBook == null && settings.promptASIN)
                {
                    //B002DW937Y
                    Log(String.Format("ASIN prompt for {0}...", title));
                    nextBook = new BookInfo(title, curBook.author, "");
                    frmASIN frmAS = new frmASIN();
                    frmAS.Text = "Next in Series";
                    frmAS.lblTitle.Text = title;
                    frmAS.ShowDialog();
                    string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, frmAS.tbAsin.Text.ToUpper());
                    nextBook.GetAmazonInfo(Url);
                    nextBook.amazonUrl = Url;
                    nextBook.asin = frmAS.tbAsin.Text.ToUpper();
                    frmAS.Dispose();
                }
                if (nextBook == null)
                {
                    Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n" +
                        "Please report this book and the Goodreads URL and output log to improve parsing.");
                }
            }
            else if (curBook.seriesPosition != curBook.totalInSeries)
                Log("An error occurred finding the next book in series, the book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out title))
            {
                if (curBook.previousInSeries == null)
                {
                    curBook.previousInSeries = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, "^" + title + @"(?: \(.*\))?$"));
                    if (curBook.previousInSeries == null)
                    {
                        // Attempt to search Amazon for the book instead
                        try
                        {
                            curBook.previousInSeries = Amazon.SearchBook(title, curBook.author, TLD);
                        }
                        catch
                        {
                            Log(String.Format("Failed to find {0} on Amazon." + TLD + ", trying again with Amazon.com.", title));
                            TLD = "com";
                            curBook.previousInSeries = Amazon.SearchBook(title, curBook.author, TLD);
                        }
                        //fill in desc, imageurl, and ratings
                        if (curBook.previousInSeries != null)
                            curBook.previousInSeries.GetAmazonInfo(curBook.previousInSeries.amazonUrl);
                    }
                    if (curBook.previousInSeries == null && settings.promptASIN)
                    {
                        //B000W94GH2
                        Log(String.Format("ASIN prompt for {0}...", title));
                        curBook.previousInSeries = new BookInfo(title, curBook.author, "");
                        frmASIN frmAS = new frmASIN();
                        frmAS.Text = "Previous in Series";
                        frmAS.lblTitle.Text = title;
                        frmAS.ShowDialog();
                        string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, frmAS.tbAsin.Text.ToUpper());
                        curBook.previousInSeries.GetAmazonInfo(Url);
                        curBook.previousInSeries.amazonUrl = Url;
                        curBook.previousInSeries.asin = frmAS.tbAsin.Text.ToUpper();
                        frmAS.Dispose();
                    }
                }
                if (curBook.previousInSeries == null)
                {
                    Log(
                        "Book was found to be part of a series, but an error occurred finding the previous book.\r\n" +
                        "Please report this book and the Goodreads URL and output log to improve parsing.");
                }
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

                Log(String.Format("{0} been highlighted {1}", textPassages, textHighlights));
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
            if (SeriesNode == null)
                return results;
            match = Regex.Match(SeriesNode.OuterHtml, @"/series/([0-9]*)");
            if (!match.Success)
                return results;
            goodreadsSeriesUrl = String.Format(goodreadsSeriesUrl, match.Groups[1].Value);
            match = Regex.Match(SeriesNode.InnerText, @"\((.*) #?([0-9]*([.,][0-9])?)\)");
            if (match.Success)
            {
                Log(String.Format("Series Goodreads Page URL: {0}", goodreadsSeriesUrl));
                curBook.seriesName = match.Groups[1].Value.Trim();
                curBook.seriesPosition = match.Groups[2].Value.Trim();
            }
            else
                return results;

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
                else
                {
                    match = Regex.Match(SeriesNode.InnerText, @"([0-9]*) works,");
                    curBook.totalInSeries = match.Groups[1].Value;
                }
                bool notWholeNumber = curBook.seriesPosition.Contains(".");
                int positionInt = (int)Convert.ToDouble(curBook.seriesPosition, CultureInfo.InvariantCulture.NumberFormat);
                int totalInt = (int)Convert.ToDouble(curBook.totalInSeries, CultureInfo.InvariantCulture.NumberFormat);
                
                if (positionInt == 1)
                {
                    Log(String.Format("This is the first book in the {0} series", curBook.seriesName));
                }
                if (positionInt == totalInt)
                {
                    Log(String.Format("This is the latest book in the {0} series", curBook.seriesName));
                }
                if (positionInt < totalInt)
                    Log(String.Format("This is book {0} of {1} in the {2} series",
                            curBook.seriesPosition, curBook.totalInSeries, curBook.seriesName));
                
                HtmlNodeCollection bookNodes = seriesHtmlDoc.DocumentNode.SelectNodes("//tr[@itemtype='http://schema.org/Book']");
                string stringPrevSearch = notWholeNumber ?
                    String.Format(@"book {0}", positionInt) :
                    String.Format(@"book {0}", positionInt - 1);
                string stringNextSearch = String.Format(@"book {0}", positionInt + 1);
                string nextTitle, previousTitle;
                if (bookNodes != null)
                {
                    foreach (HtmlNode book in bookNodes)
                    {
                        match = Regex.Match(book.InnerText, stringPrevSearch);
                        if (match.Success)
                        {
                            HtmlNode title = book.SelectSingleNode(".//a[@class='bookTitle']");
                            previousTitle = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", string.Empty);
                            results["Previous"] = previousTitle;
                            Log(String.Format("Preceded by: {0}", previousTitle));
                            continue;
                        }
                        match = Regex.Match(book.InnerText, stringNextSearch);
                        if (match.Success)
                        {
                            HtmlNode title = book.SelectSingleNode(".//a[@class='bookTitle']");
                            nextTitle = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", string.Empty);
                            results["Next"] = nextTitle;
                            Log(String.Format("Followed by: {0}", nextTitle));
                        }
                        if (results.Count == 2) break; // next and prev found
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

        public override List<XRay.Term> GetTerms(bool useSpoilers, string dataUrl, Action<string> Log)
        {
            List<XRay.Term> terms = new List<XRay.Term>();
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(dataUrl));
            }
			List<HtmlNode> allChars; 
            HtmlNodeCollection charNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/a");
            if (charNodes == null) return terms;
			allChars = charNodes.ToList();
			charNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/span[@class='toggleContent']/a");
			if (charNodes != null)
				allChars.AddRange(charNodes);
            foreach (HtmlNode charNode in allChars)
            {
                try
                {
                    XRay.Term tempTerm = GetTerm(dataUrl, charNode.GetAttributeValue("href", ""));
                    if (tempTerm != null)
                        terms.Add(tempTerm);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(404)"))
                        Log("Error getting page for character. URL: " + "https://www.goodreads.com" + charNode.GetAttributeValue("href", "")
                            + "\r\nMessage: " + ex.Message);
                }
            }
            return terms;
        }

        // Are there actually any goodreads pages that aren't at goodreads.com for other languages??
        private XRay.Term GetTerm(string baseUrl, string relativeUrl)
        {
            XRay.Term result = new XRay.Term("character");
            Uri tempUri = new Uri(baseUrl);
            tempUri = new Uri(new Uri(tempUri.GetLeftPart(UriPartial.Authority)), relativeUrl);
            result.DescSrc = "Goodreads";
            result.DescUrl = tempUri.ToString();
            HtmlDocument charDoc = new HtmlDocument();
            charDoc.LoadHtml(HttpDownloader.GetPageHtml(tempUri.ToString()));
            HtmlNode mainNode = charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat']");
            result.TermName = mainNode.SelectSingleNode("./h1").InnerText;
            mainNode = mainNode.SelectSingleNode("//div[@class='grey500BoxContent']");
            HtmlNodeCollection tempNodes = mainNode.SelectNodes("//div[@class='floatingBox']");
            if (tempNodes == null) return result;
            foreach (HtmlNode tempNode in tempNodes)
            {
                if (tempNode.Id.Contains("_aliases")) // If present, add any aliases found
                {
                    string aliasStr = tempNodes[0].InnerText.Replace("[close]", "").Trim();
                    string[] aliases = aliasStr.Split(new [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    if (aliases != null)
                        result.Aliases.AddRange(aliases);
                }
                else
                {
                    result.Desc = tempNode.InnerText.Replace("[close]", "").Trim();
                }
            }
            return result;
        }
    }
}
