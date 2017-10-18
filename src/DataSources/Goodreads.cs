using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        private List<BookInfo> goodreadsBookList = new List<BookInfo>();

        // Goodreads expects %26 and %27 instead of & and ’ or ' and %20 instead of spaces
        Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                {"&", "%26"},
                {"’", "%27"},
                {"'", "%27"},
                {" ", "%20"}
            };

        public override async Task<string> SearchBook(string author, string title)
        {
            string goodreadsSearchUrlBase = @"http://www.goodreads.com/search?q={0}%20{1}";
            string goodreadsBookUrl = "";
           
            Regex regex = new Regex(String.Join("|", replacements.Keys.Select(k => Regex.Escape(k))));
            title = regex.Replace(title, m => replacements[m.Value]);
            author = Functions.FixAuthor(author);
            author = regex.Replace(author, m => replacements[m.Value]);

            HtmlDocument goodreadsHtmlDoc = new HtmlDocument();
            goodreadsHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(String.Format(goodreadsSearchUrlBase, author, title)));
            if (goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
            {
                author = Functions.TrimAuthor(author);
                goodreadsHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(String.Format(goodreadsSearchUrlBase, author, title)));
            }
            if (!goodreadsHtmlDoc.DocumentNode.InnerText.Contains("No results"))
            {
                // Revert back for searching title
                string revertTitle = title.Replace("%26", "&amp;").Replace("%27", "'").Replace("%20", " ");
                goodreadsBookUrl = FindGoodreadsURL(goodreadsHtmlDoc, author, revertTitle);
                if (goodreadsBookUrl != "")
                {
                    return goodreadsBookUrl;
                }
            }
            return "";
        }

        private string FindGoodreadsURL(HtmlDocument goodreadsHtmlDoc, string author, string title)
        {
            goodreadsBookList.Clear();
            string goodreadsBookUrl = @"http://www.goodreads.com/book/show/{0}", ratingText = "";
            HtmlNodeCollection resultNodes =
                goodreadsHtmlDoc.DocumentNode.SelectNodes("//tr[@itemtype='http://schema.org/Book']");
            //Allow user to choose from a list of search results
            foreach (HtmlNode link in resultNodes)
            {
                HtmlNode coverNode = link.SelectSingleNode(".//img");
                //If more than one result found, skip book if it does not have a cover
                //Books with a cover are more likely to be a correct match?
                //if (coverNode.GetAttributeValue("src", "").Contains("nophoto") && (resultNodes.Count != 1)) continue;
                //Skip audiobook results
                HtmlNode audiobookNode = link.SelectSingleNode(".//span[@class='authorName greyText smallText role']");
                if (audiobookNode != null && audiobookNode.InnerText.Contains("Audiobook")) continue;
                HtmlNode titleNode = link.SelectSingleNode(".//a[@class='bookTitle']");
                HtmlNode authorNode = link.SelectSingleNode(".//a[@class='authorName']");

                string cleanTitle = titleNode.InnerText.Trim().Replace("&amp;", "&").Replace("%27", "'").Replace("%20", " ");

                BookInfo newBook = new BookInfo(cleanTitle, authorNode.InnerText.Trim(), null);

                Match matchID = Regex.Match(link.OuterHtml, @"./book/show/([0-9]*)");
                if (matchID.Success)
                    newBook.goodreadsID = matchID.Groups[1].Value;
                newBook.bookImageUrl = coverNode.GetAttributeValue("src", "");
                newBook.dataUrl = String.Format(goodreadsBookUrl, matchID.Groups[1].Value);

                HtmlNode ratingNode = link.SelectSingleNode(".//span[@class='greyText smallText uitext']");
                ratingText = Functions.CleanString(ratingNode.InnerText.Trim());
                    matchID = Regex.Match(ratingText, @"(\d+[\.,]?\d*) avg rating");
                    if (matchID.Success)
                        newBook.amazonRating = float.Parse(matchID.Groups[1].Value);
                    matchID = Regex.Match(ratingText, @"(\d+) ratings");
                    if (matchID.Success)
                        newBook.numReviews = int.Parse(matchID.Groups[1].Value);
                    matchID = Regex.Match(ratingText, @"(\d+) edition|editions");
                    if (matchID.Success)
                        newBook.editions = matchID.Groups[1].Value;
                    goodreadsBookList.Add(newBook);
                }

            int i = 0;
            if (goodreadsBookList.Count > 1)
            {
                Logger.Log("Warning: Multiple results returned from Goodreads...");
                frmG.BookList = goodreadsBookList;
                frmG.ShowDialog();
                i = frmG.cbResults.SelectedIndex;
            }
            return goodreadsBookList[i].dataUrl;
           }

        /// <summary>
        /// Searches for the next and previous books in a series, if it is part of one.
        /// Modifies curBook.previousInSeries to contain the found book info.
        /// </summary>
        /// <returns>Next book in series</returns>
        public override async Task<BookInfo> GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, string TLD)
        {
            BookInfo nextBook = null;
            BookInfo prevBook = null;

            if (curBook.dataUrl == "") return null;
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }

            // Get title of next book
            Dictionary<string, BookInfo> seriesInfo = await GetNextInSeriesTitle(curBook);
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
                            await nextBook.GetAmazonInfo(Url);
                        }
                        else
                            nextBook = await Amazon.SearchBook(book.title, book.author, TLD);
                        if (nextBook == null && settings.promptASIN)
                        {
                            Logger.Log(String.Format("ASIN prompt for {0}...", book.title));
                            nextBook = new BookInfo(book.title, book.author, "");
                            frmAS.Text = "Next in Series";
                            frmAS.lblTitle.Text = book.title;
                            frmAS.tbAsin.Text = "";
                            frmAS.ShowDialog();
                            Logger.Log(String.Format("ASIN supplied: {0}", frmAS.tbAsin.Text));
                            string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, frmAS.tbAsin.Text);
                            await nextBook.GetAmazonInfo(Url);
                            nextBook.amazonUrl = Url;
                            nextBook.asin = frmAS.tbAsin.Text;
                        }
                    }
                    catch
                    {
                        Logger.Log(String.Format("Failed to find {0} on Amazon." + TLD + ", trying again with Amazon.com.", book.title));
                        TLD = "com";
                        nextBook = await Amazon.SearchBook(book.title, book.author, TLD);
                    }
                    if (nextBook != null)
                        await nextBook.GetAmazonInfo(nextBook.amazonUrl); //fill in desc, imageurl, and ratings
                }
                
                if (nextBook == null)
                {
                    Logger.Log("Book was found to be part of a series, but an error occurred finding the next book.\r\n" +
                        "Please report this book and the Goodreads URL and output log to improve parsing.");
                }
            }
            else if (curBook.seriesPosition != curBook.totalInSeries && !curBook.seriesPosition.Contains("."))
                Logger.Log("An error occurred finding the next book in series, the book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out book))
            {
                if (prevBook == null)
                {
                    prevBook = authorProfile.otherBooks.FirstOrDefault(bk => Regex.IsMatch(bk.title, "^" + book.title + @"(?: \(.*\))?$"));
                    if (book.asin != null)
                    {
                        prevBook = book;
                        string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, book.asin);
                        await prevBook.GetAmazonInfo(Url);
                    }
                    else if(prevBook != null)
                        await prevBook.GetAmazonInfo(prevBook.amazonUrl);
                    if (prevBook == null && settings.promptASIN)
                    {
                        Logger.Log(String.Format("ASIN prompt for {0}...", book.title));
                        prevBook = new BookInfo(book.title, book.author, "");
                        frmAS.Text = "Previous in Series";
                        frmAS.lblTitle.Text = book.title;
                        frmAS.tbAsin.Text = "";
                        frmAS.ShowDialog();
                        Logger.Log(String.Format("ASIN supplied: {0}", frmAS.tbAsin.Text));
                        string Url = String.Format("http://www.amazon.{0}/dp/{1}", TLD, frmAS.tbAsin.Text);
                        await prevBook.GetAmazonInfo(Url);
                        prevBook.amazonUrl = Url;
                        prevBook.asin = frmAS.tbAsin.Text;
                    }
                }
                if (prevBook == null)
                {
                    Logger.Log(
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
        private async Task<Dictionary<string, BookInfo>> GetNextInSeriesTitle(BookInfo curBook)
        {
            Match match;
            Dictionary<string, BookInfo> results = new Dictionary<string, BookInfo>(2); 
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }

            //Search Goodreads for series info
            string goodreadsSeriesUrl = @"http://www.goodreads.com/series/{0}";
            HtmlNode metaNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
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
                Logger.Log(String.Format("Series Goodreads Page URL: {0}", goodreadsSeriesUrl));
                curBook.seriesName = match.Groups[1].Value.Trim();
                curBook.seriesPosition = match.Groups[2].Value.Trim();
            }
            else
                return results;

            HtmlDocument seriesHtmlDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
            seriesHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(goodreadsSeriesUrl));
            
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
                    Logger.Log(String.Format("This is the first book in the {0} series", curBook.seriesName));
                }
                if (positionInt == totalInt)
                {
                    Logger.Log(String.Format("This is the latest book in the {0} series", curBook.seriesName));
                }
                if (positionInt < totalInt)
                    Logger.Log(String.Format("This is book {0} of {1} in the {2} series",
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
                                prevBook.asin = await SearchBookASIN(match.Groups[1].Value, prevBook.title);
                            prevBook.author = book.SelectSingleNode(".//a[@class='authorName']").InnerText.Trim();                            
                            results["Previous"] = prevBook;
                            curBook.previousInSeries = prevBook;
                            Logger.Log(String.Format("Preceded by: {0}", prevBook.title));
                            continue;
                        }
                        if (bookIndex.InnerText == stringNextSearch)
                        {
                            BookInfo nextBook = new BookInfo("", "", "");
                            HtmlNode title = book.SelectSingleNode(".//a[@class='bookTitle']");
                            nextBook.title = Regex.Replace(title.InnerText.Trim(), @" \(.*\)", string.Empty);
                            match = Regex.Match(title.GetAttributeValue("href", ""), @"show/([0-9]*)");
                            if (match.Success)
                                nextBook.asin = await SearchBookASIN(match.Groups[1].Value, nextBook.title);                            
                            nextBook.author = book.SelectSingleNode(".//a[@class='authorName']").InnerText.Trim();
                            results["Next"] = nextBook;
                            curBook.nextInSeries = nextBook;
                            Logger.Log(String.Format("Followed by: {0}", nextBook.title));
                        }
                        if (results.Count == 2 || (results.Count == 1 & positionInt == totalInt)) break; // next and prev found or prev found and latest in series
                    }
                }
            }
            return results;
        }

        // Search Goodread for possible kindle edition of book and return ASIN.
        private async Task<string> SearchBookASIN(string id, string title)
        {
            string goodreadsBookUrl = String.Format("http://www.goodreads.com/book/show/{0}", id);
            try
            {
                HtmlDocument bookHtmlDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
                bookHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(goodreadsBookUrl));
                if (bookHtmlDoc != null)
                {
                    HtmlNode link = bookHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='otherEditionsActions']/a");
                    Match match = Regex.Match(link.GetAttributeValue("href", ""), @"editions/([0-9]*)-");
                    if (match.Success)
                    {
                        string kindleEditionsUrl = String.Format("http://www.goodreads.com/work/editions/{0}?utf8=%E2%9C%93&sort=num_ratings&filter_by_format=Kindle+Edition", match.Groups[1].Value);
                        bookHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(kindleEditionsUrl));
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
                Logger.Log(String.Format("An error occurred while searching for {0}s ASIN.\r\n", title) + ex.Message + "\r\n" + ex.StackTrace);
                return "";
            }
        }

        public override async Task<bool> GetPageCount(BookInfo curBook)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }
            HtmlNode pagesNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='details']");
            if (pagesNode == null)
                return false;
            Match match = Regex.Match(pagesNode.InnerText, @"((\d+)|(\d+,\d+)) pages");
            if (match.Success)
            {
                double minutes = int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                TimeSpan span = TimeSpan.FromMinutes(minutes);
                Logger.Log(String.Format("Typical time to read: {0} hours and {1} minutes ({2} pages)", span.Hours, span.Minutes, match.Groups[1].Value));
                curBook.pagesInBook = match.Groups[1].Value;
                curBook.readingHours = span.Hours.ToString();
                curBook.readingMinutes = span.Minutes.ToString();
                return true;
            }
            return false;
        }

        public override async Task<List<XRay.Term>> GetTerms(string dataUrl, IProgress<Tuple<int, int>> progress, CancellationToken token)
        {
            List<XRay.Term> terms = new List<XRay.Term>();
            if (sourceHtmlDoc == null)
            {
                Logger.Log("Downloading Goodreads page...");
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(dataUrl));
            }
			List<HtmlNode> allChars;
            HtmlNodeCollection charNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/a");
            if (charNodes == null) return terms;
			allChars = charNodes.ToList();
            // Check if ...more link exists on Goodreads page
            HtmlNodeCollection moreCharNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@class='infoBoxRowTitle' and text()='Characters']/../div[@class='infoBoxRowItem']/span[@class='toggleContent']/a");
            if (moreCharNodes != null)
            {
                List<HtmlNode> moreChars = moreCharNodes.ToList();
                if (moreChars != null)
                    allChars.AddRange(moreChars);
            }
            Logger.Log("Gathering term information from Goodreads... (" + allChars.Count + ")");
            if (allChars.Count > 20)
                Logger.Log("More than 20 characters found. Consider using the 'download to XML' option if you need to build repeatedly.");
            int count = 1;
            if (progress != null) progress.Report(new Tuple<int, int>(1, allChars.Count));
            foreach (HtmlNode charNode in allChars)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    XRay.Term tempTerm = await GetTerm(dataUrl, charNode.GetAttributeValue("href", ""));
                    if (tempTerm != null)
                        terms.Add(tempTerm);
                    if (progress != null) progress.Report(new Tuple<int, int>(count++, allChars.Count));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(404)"))
                        Logger.Log("Error getting page for character. URL: " + "https://www.goodreads.com" + charNode.GetAttributeValue("href", "")
                            + "\r\nMessage: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            return terms;
        }

        // Are there actually any goodreads pages that aren't at goodreads.com for other languages??
        private async Task<XRay.Term> GetTerm(string baseUrl, string relativeUrl)
        {
            XRay.Term result = new XRay.Term("character");
            Uri tempUri = new Uri(baseUrl);
            tempUri = new Uri(new Uri(tempUri.GetLeftPart(UriPartial.Authority)), relativeUrl);
            result.DescSrc = "Goodreads";
            result.DescUrl = tempUri.ToString();
            HtmlDocument charDoc = new HtmlDocument();
            charDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(tempUri.ToString()));
            HtmlNode mainNode = charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat']")
                ?? charDoc.DocumentNode.SelectSingleNode("//div[@class='mainContentFloat ']");
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

        /// <summary>
        /// Gather the list of quotes & number of times they've been liked -- close enough to "x paragraphs have been highlighted y times" from Amazon
        /// </summary>
        public override async Task<List<Tuple<string, int>>> GetNotableClips(string url, CancellationToken token, HtmlDocument srcDoc = null, IProgress<Tuple<int, int>> progress = null)
        {
            if (srcDoc == null)
            {
                srcDoc = new HtmlDocument();
                srcDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(url));
            }
            List<Tuple<string, int>> result = null;
            HtmlNode quoteNode = srcDoc.DocumentNode.SelectSingleNode("//div[@class='h2Container gradientHeaderContainer']/h2/a[starts-with(.,'Quotes from')]");
            if (quoteNode == null) return null;
            string quoteURL = String.Format("http://www.goodreads.com{0}?page={{0}}", quoteNode.GetAttributeValue("href", ""));
            int maxPages = 1;
            if (progress != null) progress.Report(new Tuple<int, int>(0, 1));
            for (int i = 1; i <= maxPages; i++)
            {
                token.ThrowIfCancellationRequested();
                HtmlDocument quoteDoc = new HtmlDocument();
                quoteDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(String.Format(quoteURL, i)));
                // first time through, check how many pages there are (find previous page button, get parent div, take all children of that, 2nd last one should be the max page count
                if (maxPages == 1)
                {
                    HtmlNode tempNode = quoteDoc.DocumentNode.SelectSingleNode("//span[contains(@class,'previous_page')]/parent::div/*[last()-1]");
                    if (!int.TryParse(tempNode.InnerHtml, out maxPages)) maxPages = 1;
                    result = new List<Tuple<string, int>>(maxPages * 30);
                }
                HtmlNodeCollection tempNodes = quoteDoc.DocumentNode.SelectNodes("//div[@class='quotes']/div[@class='quote']");
                foreach (HtmlNode quote in tempNodes)
                {
                    int start = quote.InnerText.IndexOf("&ldquo;") + 7;
                    int end = quote.InnerText.IndexOf("&rdquo;");
                    int likes;
                    int.TryParse(quote.SelectSingleNode(".//div[@class='right']/a").InnerText.Replace(" likes", ""), out likes);
                    result.Add(new Tuple<string, int>(quote.InnerText.Substring(start, end - start), likes));
                }
                if (progress != null) progress.Report(new Tuple<int, int>(i, maxPages));
            }
            return result;
        }

        /// <summary>
        /// Scrape any notable quotes from Goodreads and grab ratings if missing from book info
        /// Modifies curBook.
        /// </summary>
        /// <param name="curBook"></param>
        public override async Task GetExtras(BookInfo curBook, CancellationToken token, IProgress<Tuple<int, int>> progress = null)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }
            
            if (curBook.notableClips == null)
            {
                curBook.notableClips = await GetNotableClips("", token, sourceHtmlDoc, progress);
            }
            
            //Add rating and reviews count if missing from Amazon book info
            HtmlNode metaNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='bookMeta']");
            if (curBook.amazonRating == 0)
            {
                HtmlNode goodreadsRating = metaNode.SelectSingleNode("//span[@class='value rating']");
                if (goodreadsRating != null)
                    curBook.amazonRating = float.Parse(goodreadsRating.InnerText);
                HtmlNode passagesNode = metaNode.SelectSingleNode(".//a[@class='actionLinkLite votes' and @href='#other_reviews']")
                    ?? metaNode.SelectSingleNode(".//span[@class='count value-title']");
                if (passagesNode != null)
                {
                    Match match = Regex.Match(passagesNode.InnerText, @"(\d+|\d{1,3}([,\.]\d{3})*)(?=\s)");
                    if (match.Success)
                        curBook.numReviews = int.Parse(match.Value.Replace(",", "").Replace(".", ""));
                }
            }
        }
    }
}
