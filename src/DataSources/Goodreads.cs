﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI.DataSources
{
    public class Goodreads : DataSource
    {
        public override string Name { get { return "Goodreads"; } }
        private Properties.Settings settings = Properties.Settings.Default;

        private frmASIN frmAS = new frmASIN();
        private frmGR frmG = new frmGR();

        public override string SearchBook(string author, string title, Action<string> Log)
        {
            string goodreadsSearchUrlBase = @"http://www.goodreads.com/search?q={0}%20{1}";
            string goodreadsBookUrl = "";
            // Goodreads expects %26 and %27 instead of & and ’ or ' and %20 instead of spaces
            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                {"&", "%26"},
                {"’", "%27"},
                {"'", "%27"},
                {" ", "%20"}
            };

            Regex regex = new Regex(String.Join("|", replacements.Keys.Select(k => Regex.Escape(k))));
            title = regex.Replace(title, m => replacements[m.Value]);
            author = Functions.FixAuthor(author);
            author = regex.Replace(author, m => replacements[m.Value]);

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
                string revertTitle = title.Replace("%26", "&amp;").Replace("%27", "'").Replace("%20", " ");
                goodreadsBookUrl = FindGoodreadsURL(goodreadsHtmlDoc, author, revertTitle, Log);
                if (goodreadsBookUrl != "")
                {
                    return goodreadsBookUrl;
                }
            }
            return "";
        }

        private string FindGoodreadsURL(HtmlDocument goodreadsHtmlDoc, string author, string title, Action<string> Log)
        {
            string goodreadsBookUrl = @"http://www.goodreads.com/book/show/{0}";
            //Check if results contain title and author
            HtmlNodeCollection resultNodes =
                goodreadsHtmlDoc.DocumentNode.SelectNodes("//tr[@itemtype='http://schema.org/Book']");
            //Allow user to choose from a list of search results
            if (resultNodes.Count > 0)
            {
                frmG.cbResults.Items.Clear();
                foreach (HtmlNode link in resultNodes)
                {
                    HtmlNode noPhoto = link.SelectSingleNode(".//img");
                    //If more than one result found, skip book if it does not have a cover
                    //Books with a cover are more likely to be a correct match?
                    if (noPhoto.GetAttributeValue("src", "").Contains("nophoto") && (resultNodes.Count != 1)) continue;
                    HtmlNode titleText = link.SelectSingleNode(".//a[@class='bookTitle']");
                    if (Properties.Settings.Default.showGoodreadsID)
                    {
                        Match matchID = Regex.Match(link.OuterHtml, @"./book/show/([0-9]*)");
                        if (matchID.Success)
                        {
                            frmG.cbResults.Items.Add(String.Format(@"({0}) {1}", matchID.Groups[1].Value, titleText.InnerText.Trim()));
                        }
                        else
                            frmG.cbResults.Items.Add(titleText.InnerText.Trim());
                    }
                    else
                        frmG.cbResults.Items.Add(titleText.InnerText.Trim());
                }
                frmG.cbResults.SelectedIndex = 0;
                if (frmG.cbResults.Items.Count > 1)
                {
                    Log("Warning: Multiple results returned from Goodreads...");
                    frmG.ShowDialog();
                }

                int i = frmG.cbResults.SelectedIndex;
                HtmlNode chosenResult = resultNodes[i];
                HtmlNode titleNode = chosenResult.SelectSingleNode(".//a[@class='bookTitle']");
                HtmlNode authorNode = chosenResult.SelectSingleNode(".//a[@class='authorName']");
                HtmlNode audiobookNode = chosenResult.SelectSingleNode(".//span[@class='authorName greyText smallText role']");
                Match match = Regex.Match(chosenResult.OuterHtml, @"./book/show/([0-9]*)");
                if (match.Success)
                {
                    //Return actual selected book title
                    Log(String.Format("Book found on Goodreads!\r\n{0} by {1}\r\nGoodreads URL: {2}\r\n" +
                                      "You may want to visit the URL to ensure it is correct.",
                        titleNode.InnerText.Trim(), authorNode.InnerText.Trim(),
                        String.Format(goodreadsBookUrl, match.Groups[1].Value)));
                    if (audiobookNode != null)
                    {
                        if (audiobookNode.InnerText.Contains("Narrator"))
                            Log("Warning: This book is an audiobook. You may want to visit the URL to select a different edition.");
                    }
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
            BookInfo prevBook = null;

            if (curBook.dataUrl == "") return null;
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(curBook.dataUrl));
            }

            // Get title of next book
            Dictionary<string, BookInfo> seriesInfo = GetNextInSeriesTitle(curBook, Log);
            BookInfo book;
            if (seriesInfo.TryGetValue("Next", out book))
            {
                // Search author's other books for the book (assumes next in series was written by the same author...)
                // Returns the first one found, though there should probably not be more than 1 of the same name anyway
                nextBook = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, "^" + book.title + @"(?: \(.*\))?$"));
                if (nextBook == null)
                {
                    // Attempt to search Amazon for the book instead
                    try
                    {
                        if (book.asin != null)
                        {
                            nextBook = book;
                            string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, book.asin);
                            nextBook.GetAmazonInfo(Url);
                        }
                        else
                            nextBook = Amazon.SearchBook(book.title, book.author, TLD);
                        if (nextBook == null && settings.promptASIN)
                        {
                            Log(String.Format("ASIN prompt for {0}...", book.title));
                            nextBook = new BookInfo(book.title, book.author, "");
                            frmAS.Text = "Next in Series";
                            frmAS.lblTitle.Text = book.title;
                            frmAS.tbAsin.Text = "";
                            frmAS.ShowDialog();
                            Log(String.Format("ASIN supplied: {0}", frmAS.tbAsin.Text));
                            string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, frmAS.tbAsin.Text);
                            nextBook.GetAmazonInfo(Url);
                            nextBook.amazonUrl = Url;
                            nextBook.asin = frmAS.tbAsin.Text;
                        }
                    }
                    catch
                    {
                        Log(String.Format("Failed to find {0} on Amazon." + TLD + ", trying again with Amazon.com.", book.title));
                        TLD = "com";
                        nextBook = Amazon.SearchBook(book.title, book.author, TLD);
                    }
                    if (nextBook != null)
                        nextBook.GetAmazonInfo(nextBook.amazonUrl); //fill in desc, imageurl, and ratings
                }
                
                if (nextBook == null)
                {
                    Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n" +
                        "Please report this book and the Goodreads URL and output log to improve parsing.");
                }
            }
            else if (curBook.seriesPosition != curBook.totalInSeries && !curBook.seriesPosition.Contains("."))
                Log("An error occurred finding the next book in series, the book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out book))
            {
                if (prevBook == null)
                {
                    prevBook = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, "^" + book.title + @"(?: \(.*\))?$"));
                    if (book.asin != null)
                    {
                        prevBook = book;
                        string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, book.asin);
                        prevBook.GetAmazonInfo(Url);
                    }
                    else if(prevBook != null)
                        prevBook.GetAmazonInfo(prevBook.amazonUrl);
                    if (prevBook == null && settings.promptASIN)
                    {
                        Log(String.Format("ASIN prompt for {0}...", book.title));
                        prevBook = new BookInfo(book.title, book.author, "");
                        frmAS.Text = "Previous in Series";
                        frmAS.lblTitle.Text = book.title;
                        frmAS.tbAsin.Text = "";
                        frmAS.ShowDialog();
                        Log(String.Format("ASIN supplied: {0}", frmAS.tbAsin.Text));
                        string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, frmAS.tbAsin.Text);
                        prevBook.GetAmazonInfo(Url);
                        prevBook.amazonUrl = Url;
                        prevBook.asin = frmAS.tbAsin.Text;
                    }
                }
                if (prevBook == null)
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
        private Dictionary<string, BookInfo> GetNextInSeriesTitle(BookInfo curBook, Action<string> Log)
        {
            Match match;
            Dictionary<string, BookInfo> results = new Dictionary<string, BookInfo>(2); 
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(curBook.dataUrl));
            }
            //Use Goodreads reviews and ratings to generate popular passages dummy
            int highlights = 0;
            int passages = 0;
            Random randomNumber = new Random();

            HtmlNode metaNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
            if (metaNode != null)
            {
                HtmlNode passagesNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']");
                match = Regex.Match(passagesNode.InnerText, @"(\d+,\d+,\d+)|(\d+,\d+)|(\d+)");
                if (match.Success)
                {
                    string tempStrg = match.Value.Replace(",", ""); //Remove all commas from value
                    passages = int.Parse(tempStrg, NumberStyles.AllowThousands);                    
                }
                if (passages == 0 || passages < 10)
                {
                    passages = randomNumber.Next(1, 51);
                }
                if (passages > 10000)
                    passages = passages / 100;
                if (passages > 200)
                    passages = passages / 10;
                curBook.popularPassages = passages.ToString();

                HtmlNode highlightsNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite' and @href='#other_reviews']");
                match = Regex.Match(highlightsNode.InnerText, @"(\d+,\d+,\d+)|(\d+,\d+)|(\d+)");
                if (match.Success)
                {
                    string tempStrg = match.Value.Replace(",", ""); //Remove all commas from value
                    highlights = int.Parse(tempStrg, NumberStyles.AllowThousands);                    
                }
                if (highlights == 0 || highlights < 10)
                {
                    highlights = randomNumber.Next(50, 101);
                }
                if (highlights > 10000)
                    highlights = highlights / 100;
                if (highlights > 200)
                    highlights = highlights / 10;
                curBook.popularHighlights = highlights.ToString();

                string textPassages = curBook.popularPassages == "1"
                    ? String.Format("{0} passage has", curBook.popularPassages)
                    : String.Format("{0} passages have", curBook.popularPassages);
                string textHighlights = curBook.popularHighlights == "1"
                    ? String.Format("{0} time", curBook.popularHighlights)
                    : String.Format("{0} times", curBook.popularHighlights);

                Log(String.Format("{0} been highlighted {1}", textPassages, textHighlights));
            }
            if (highlights == 0 && passages == 0)
            {
                Log("No highlighted passages have been found for this book");
                curBook.popularPassages = "";
                curBook.popularHighlights = "";
            }

            //Add rating and reviews count if missing from Amazon book ifo
            if (curBook.amazonRating == 0)
            {
                HtmlNode goodreadsRating = metaNode.SelectSingleNode("//span[@class='value rating']");
                if (goodreadsRating != null)
                {
                    curBook.amazonRating = float.Parse(goodreadsRating.InnerText);
                }
                HtmlNode passagesNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']");
                match = Regex.Match(passagesNode.InnerText, @"(\d+,\d+,\d+)|(\d+,\d+)|(\d+)");
                if (match.Success)
                {
                    string tempStrg = match.Value.Replace(",", ""); //Remove all commas from value
                    curBook.numReviews = int.Parse(tempStrg, NumberStyles.AllowThousands);
                }
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
                if (bookNodes != null)
                {
                    foreach (HtmlNode book in bookNodes)
                    {
                        HtmlNode bookIndex = book.SelectSingleNode(".//em");
                        if (bookIndex == null) continue;
                        if (bookIndex.InnerText == stringPrevSearch && results.Count == 0)
                        {
                            BookInfo prevBook = new BookInfo("", "", "");
                            HtmlNode title = book.SelectSingleNode(".//a[@class='bookTitle']");
                            prevBook.title = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", string.Empty);
                            match = Regex.Match(title.GetAttributeValue("href", ""), @"show/([0-9]*)");
                            if (match.Success)
                                prevBook.asin = SearchBookASIN(match.Groups[1].Value, prevBook.title, Log);
                            prevBook.author = book.SelectSingleNode(".//a[@class='authorName']").InnerText.Trim();                            
                            results["Previous"] = prevBook;
                            curBook.previousInSeries = prevBook;
                            Log(String.Format("Preceded by: {0}", prevBook.title));
                            continue;
                        }
                        if (bookIndex.InnerText == stringNextSearch)
                        {
                            BookInfo nextBook = new BookInfo("", "", "");
                            HtmlNode title = book.SelectSingleNode(".//a[@class='bookTitle']");
                            nextBook.title = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", string.Empty);
                            match = Regex.Match(title.GetAttributeValue("href", ""), @"show/([0-9]*)");
                            if (match.Success)
                                nextBook.asin = SearchBookASIN(match.Groups[1].Value, nextBook.title, Log);                            
                            nextBook.author = book.SelectSingleNode(".//a[@class='authorName']").InnerText.Trim();
                            results["Next"] = nextBook;
                            curBook.nextInSeries = nextBook;
                            Log(String.Format("Followed by: {0}", nextBook.title));
                        }
                        if (results.Count == 2 || (results.Count == 1 & positionInt == totalInt)) break; // next and prev found or prev found and latest in series
                    }
                }
            }
            return results;
        }

        // Search Goodread for possible kindle edition of book and return ASIN.
        private string SearchBookASIN(string id, string title, Action<string> Log)
        {
            string goodreadsBookUrl = String.Format("http://www.goodreads.com/book/show/{0}", id);
            try
            {
                HtmlDocument bookHtmlDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
                bookHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(goodreadsBookUrl));
                if (bookHtmlDoc != null)
                {
                    HtmlNode link = bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='otherEditionsActions']/a");
                    Match match = Regex.Match(link.GetAttributeValue("href", ""), @"editions/([0-9]*)-");
                    if (match.Success)
                    {
                        string kindleEditionsUrl = String.Format("http://www.goodreads.com/work/editions/{0}?utf8=%E2%9C%93&sort=num_ratings&filter_by_format=Kindle+Edition", match.Groups[1].Value);
                        bookHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(kindleEditionsUrl));
                        HtmlNodeCollection bookNodes = bookHtmlDoc.DocumentNode.SelectNodes("//div[@class='elementList clearFix']");
                        if (bookNodes != null)
                        {
                            foreach (HtmlNode book in bookNodes)
                            {
                                match = Regex.Match(book.InnerHtml, "(B[A-Z0-9]{9})");
                                if (match.Success)
                                    return match.Value;
                            }
                        }
                    }
                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                Log(String.Format("An error occurred while searching for {0}s ASIN.\r\n", title) + ex.Message + "\r\n" + ex.StackTrace);
                return "";
            }
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
            Log("Gathering term information from Goodreads...");
			List<HtmlNode> allChars;
            List<HtmlNode> moreChars = null;
            HtmlNodeCollection charNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/a");
            if (charNodes == null) return terms;
			allChars = charNodes.ToList();
            // Check if ...more link exists on Goodreads page
            HtmlNodeCollection moreCharNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/span[@class='toggleContent']/a");
            if (moreCharNodes != null)
                moreChars = moreCharNodes.ToList();
            if (moreChars != null)
                allChars.AddRange(moreChars);
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
                            + "\r\nMessage: " + ex.Message + "\r\n" + ex.StackTrace);
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
