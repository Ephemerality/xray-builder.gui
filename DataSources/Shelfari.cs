using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;

namespace XRayBuilderGUI.DataSources
{
    public class Shelfari : DataSource
    {
        public override string Name { get { return "Shelfari"; } }

        public override string SearchBook(string author, string title, Action<string> Log)
        {
            //Get Shelfari Search URL
            Log("Searching for book on Shelfari...");
            string shelfariSearchUrlBase = @"http://www.shelfari.com/search/books?Author={0}&Title={1}&Binding={2}";
            string[] bindingTypes = { "Hardcover", "Kindle", "Paperback" };

            // Search book on Shelfari
            bool bookFound = false;
            string shelfariBookUrl = "";
            author = Functions.FixAuthor(author);

            HtmlDocument shelfariHtmlDoc = new HtmlDocument();
            for (int j = 0; j <= 1; j++)
            {
                for (int i = 0; i < bindingTypes.Length; i++)
                {
                    Log("Searching for " + bindingTypes[i] + " edition...");
                    // Insert parameters (mainly for searching with removed diacritics). Seems to work fine without replacing spaces?
                    shelfariHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(String.Format(shelfariSearchUrlBase, author, title, bindingTypes[i])));
                    if (!shelfariHtmlDoc.DocumentNode.InnerText.Contains("Your search did not return any results"))
                    {
                        shelfariBookUrl = FindShelfariURL(shelfariHtmlDoc, author, title);
                        if (shelfariBookUrl != "")
                        {
                            return shelfariBookUrl;
                        }
                    }
                    Log("Unable to find a " + bindingTypes[i] + " edition of this book on Shelfari!");
                }
                if (bookFound) break;
                // Attempt to remove diacritics (accented characters) from author & title for searching
                string newAuthor = author.RemoveDiacritics();
                string newTitle = title.RemoveDiacritics();
                if (!author.Equals(newAuthor) || !title.Equals(newTitle))
                {
                    author = newAuthor;
                    title = newTitle;
                    Log("Accented characters detected. Attempting to search without them.");
                }
            }
            return "";
        }

        private string FindShelfariURL(HtmlDocument shelfariHtmlDoc, string author, string title)
        {
            // Try to find book's page from Shelfari search
            string shelfariBookUrl = "";
            int index = 0;
            List<string> listofthings = new List<string>();
            List<string> listoflinks = new List<string>();
            Dictionary<string, string> retData = new Dictionary<string, string>();

            foreach (HtmlNode bookItems in shelfariHtmlDoc.DocumentNode.SelectNodes("//li[@class='item']/div[@class='text']"))
            {
                if (bookItems == null) continue;
                listofthings.Clear();
                listoflinks.Clear();
                for (var i = 1; i < bookItems.ChildNodes.Count; i++)
                {
                    if (bookItems.ChildNodes[i].GetAttributeValue("class", "") == "series") continue;
                    listofthings.Add(bookItems.ChildNodes[i].InnerText.Trim());
                    listoflinks.Add(bookItems.ChildNodes[i].InnerHtml);
                }
                index = 0;
                foreach (string line in listofthings)
                {
                    // Search for author with spaces removed to avoid situations like "J.R.R. Tolkien" / "J. R. R. Tolkien"
                    // Ignore Collective Work search result.
                    // May cause false matches, we'll see.
                    // Also remove diacritics from titles when matching just in case...
                    // Searching for Children of Húrin will give a false match on the first pass before diacritics are removed from the search URL
                    if ((listofthings.Contains("(Author)") || listofthings.Contains("(Author),")) &&
                        line.RemoveDiacritics().StartsWith(title.RemoveDiacritics(), StringComparison.OrdinalIgnoreCase) &&
                        (listofthings.Contains(author) || listofthings.Exists(r => r.Replace(" ", "") == author.Replace(" ", ""))))
                        if (!listoflinks.Any(c => c.Contains("(collective work)")))
                        {
                            shelfariBookUrl = listoflinks[index].ToString();
                            shelfariBookUrl = Regex.Replace(shelfariBookUrl, "<a href=\"", "", RegexOptions.None);
                            shelfariBookUrl = Regex.Replace(shelfariBookUrl, "\".*?</a>.*", "", RegexOptions.None);
                            if (shelfariBookUrl.ToLower().StartsWith("http://"))
                                return shelfariBookUrl;
                        }
                    index++;
                }
            }
            return "";
        }

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
                // Try to fill in desc, imageurl, and ratings using Shelfari Kindle edition link instead
                if (nextBook == null)
                {
                    HtmlDocument bookDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
                    bookDoc.LoadHtml(HttpDownloader.GetPageHtml(seriesInfo["NextURL"]));
                    Match match = Regex.Match(bookDoc.DocumentNode.InnerHtml, "('B[A-Z0-9]{9}')");
                    if (match.Success)
                    {
                        string cleanASIN = match.Value.Replace("'", String.Empty);
                        nextBook = new BookInfo(title, curBook.author, cleanASIN);
                        nextBook.GetAmazonInfo("http://www.amazon.com/dp/" + cleanASIN);
                    }
                }
                if (nextBook == null)
                    Log("Book was found to be part of a series, but an error occured finding the next book.\r\n" +
                        "Please report this book and the Shelfari URL and output log to improve parsing.");

            }
            else if (curBook.seriesPosition != curBook.totalInSeries)
                Log("An error occured finding the next book in series, the book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out title))
            {
                if (curBook.previousInSeries == null)
                {
                    // Attempt to search Amazon for the book
                    curBook.previousInSeries = Functions.AmazonSearchBook(title, curBook.author);
                    if (curBook.previousInSeries != null)
                        curBook.previousInSeries.GetAmazonInfo(curBook.previousInSeries.amazonUrl); //fill in desc, imageurl, and ratings

                    // Try to fill in desc, imageurl, and ratings using Shelfari Kindle edition link instead
                    if (curBook.previousInSeries == null)
                    {
                        HtmlDocument bookDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
                        bookDoc.LoadHtml(HttpDownloader.GetPageHtml(seriesInfo["PreviousURL"]));
                        Match match = Regex.Match(bookDoc.DocumentNode.InnerHtml, "('B[A-Z0-9]{9}')");
                        if (match.Success)
                        {
                            string cleanASIN = match.Value.Replace("'", String.Empty);
                            curBook.previousInSeries = new BookInfo(title, curBook.author, cleanASIN);
                            curBook.previousInSeries.GetAmazonInfo("http://www.amazon.com/dp/" + cleanASIN);
                        }
                    }
                }
                else
                    Log("Book was found to be part of a series, but an error occured finding the next book.\r\n" +
                        "Please report this book and the Shelfari URL and output log to improve parsing.");
            }

            return nextBook;
        }

        /// <summary>
        /// Search Shelfari page for possible series info, returning the next title in the series without downloading any other pages.
        /// </summary>
        private Dictionary<string, string> GetNextInSeriesTitle(BookInfo curBook, Action<string> Log)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(curBook.dataUrl));
            }
            Dictionary<string, string> results = new Dictionary<string, string>(2);
            //Added estimated reading time and page count from Shelfari, for now...
            HtmlNode pageNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_FirstEdition']");
            if (pageNode == null)
                return results;
            HtmlNode node1 = pageNode.SelectSingleNode(".//div/div");
            if (node1 == null)
                return results;

            //Added highlighted passage from Shelfari, dummy info for now...
            HtmlNode members = sourceHtmlDoc.DocumentNode.SelectSingleNode("//ul[@class='tabs_n tn1']");
            int highlights = 0;
            if (members != null)
            {
                Match match3 = Regex.Match(members.InnerText, @"Reviews \(((\d+)|(\d+,\d+))\)");
                if (match3.Success)
                    curBook.popularPassages = match3.Groups[1].Value.ToString();
                match3 = Regex.Match(members.InnerText, @"Readers \(((\d+)|(\d+,\d+))\)");
                if (match3.Success)
                {
                    curBook.popularHighlights = match3.Groups[1].Value.ToString();
                    highlights = int.Parse(match3.Groups[1].Value, NumberStyles.AllowThousands);
                }
                string textPassages = curBook.popularPassages == "1"
                    ? String.Format("{0} passage has ", curBook.popularPassages)
                    : String.Format("{0} passages have ", curBook.popularPassages);
                string textHighlights = curBook.popularHighlights == "1"
                    ? String.Format("{0} time", curBook.popularHighlights)
                    : String.Format("{0} times", curBook.popularHighlights);

                Log(String.Format("Popular Highlights: {0} been highlighted {1}"
                            , textPassages, textHighlights));
            }

            //If no "highlighted passages" found from Shelfari, add to log
            if (highlights == 0)
            {
                Log("Popular Highlights: No highlighted passages have been found for this book");
                curBook.popularPassages = "";
                curBook.popularHighlights = "";
            }

            //Check if book series is available and displayed in Series & Lists on Shelfari page.
            HtmlNode seriesNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_Series']/div");
            if (seriesNode != null)
            {
                //If multiple Series found, find and use standard series.
                foreach (HtmlNode seriesType in seriesNode.SelectNodes(".//div"))
                {
                    if (seriesType.InnerText.Contains("(standard series)", StringComparison.OrdinalIgnoreCase) && !seriesType.InnerText.Contains("(Reading Order)", StringComparison.OrdinalIgnoreCase))
                    {
                        Match match = Regex.Match(seriesType.InnerText, @"This is book (\d+) of (\d+) in (.+)\.");
                        if (!match.Success || match.Groups.Count != 4)
                            continue;

                        Log("About the series: " + seriesType.InnerText.Replace(". (standard series)", ""));
                        curBook.seriesPosition = match.Groups[1].Value;
                        curBook.totalInSeries = match.Groups[2].Value;
                        curBook.seriesName = match.Groups[3].Value;
                        HtmlNode seriesInfo = seriesNode.SelectSingleNode(".//p");
                        //Parse preceding book
                        if (seriesInfo != null && seriesInfo.InnerText.Contains("Preceded by ", StringComparison.OrdinalIgnoreCase))
                        {
                            match = Regex.Match(seriesInfo.InnerText, @"Preceded by (.*),", RegexOptions.IgnoreCase);
                            if (match.Success && match.Groups.Count == 2)
                            {
                                results["Previous"] = match.Groups[1].Value;
                            }
                            else
                            {
                                match = Regex.Match(seriesInfo.InnerText, @"Preceded by (.*)\.", RegexOptions.IgnoreCase);
                                if (match.Success && match.Groups.Count == 2)
                                {
                                    results["Previous"] = match.Groups[1].Value;
                                }
                            }
                            Log("Preceded by: " + match.Groups[1].Value);
                            //Grab Shelfari Kindle edition link for this book
                            results["PreviousURL"] = seriesInfo.ChildNodes["a"].GetAttributeValue("href", "") + "/editions?binding=Kindle";
                        }
                        // Check if book is the last in the series
                        if (!curBook.seriesPosition.Equals(curBook.totalInSeries))
                        {
                            //Parse following book
                            if (seriesInfo != null && seriesInfo.InnerText.Contains("followed by ", StringComparison.OrdinalIgnoreCase))
                            {
                                match = Regex.Match(seriesInfo.InnerText, @"followed by (.*)\.", RegexOptions.IgnoreCase);
                                if (match.Success && match.Groups.Count == 2)
                                {
                                    Log("Followed by: " + match.Groups[1].Value);
                                    //Grab Shelfari Kindle edition link for this book
                                    results["NextURL"] = seriesInfo.ChildNodes["a"].GetAttributeValue("href", "") + "/editions?binding=Kindle";
                                    results["Next"] = match.Groups[1].Value;
                                    return results;
                                }
                            }
                        }
                        break;
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
            HtmlNode pageNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_FirstEdition']");
            if (pageNode == null)
                return false;
            HtmlNode node1 = pageNode.SelectSingleNode(".//div/div");
            if (node1 == null)
                return false;
            //Parse page count and multiply by average reading time
            Match match1 = Regex.Match(node1.InnerText, @"Page Count: ((\d+)|(\d+,\d+))");
            if (match1.Success)
            {
                double minutes = int.Parse(match1.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                TimeSpan span = TimeSpan.FromMinutes(minutes);
                Log(String.Format("Typical time to read: {0} hours and {1} minutes ({2} pages)", span.Hours, span.Minutes, match1.Groups[1].Value));
                curBook.pagesInBook = match1.Groups[1].Value;
                curBook.readingHours = span.Hours.ToString();
                curBook.readingMinutes = span.Minutes.ToString();
                return true;
            }
            return false;
        }
        
        public override List<XRay.Term> GetTerms(bool useSpoilers, string dataUrl, Action<string> Log)
        {
            List<XRay.Term> terms = new List<XRay.Term>();
            //Download HTML of Shelfari URL, try 3 times just in case it fails the first time
            Log(String.Format("Downloading Shelfari page... {0}", useSpoilers ? "SHOWING SPOILERS!" : ""));
            Log(String.Format("Shelfari URL: {0}", dataUrl));
            HtmlDocument shelfariDoc = new HtmlDocument();

            if (useSpoilers || sourceHtmlDoc == null)
            {
                var tries = 3;
                do
                {
                    try
                    {
                        //Enable cookies
                        var jar = new System.Net.CookieContainer();
                        var client = new HttpDownloader(dataUrl, jar, "", "");

                        if (useSpoilers)
                        {
                            //Grab book ID from url (search for 5 digits between slashes) and create spoiler cookie
                            var bookId = Regex.Match(dataUrl, @"\/\d{5}").Value.Substring(1, 5);
                            var spoilers = new System.Net.Cookie("ShelfariBookWikiSession", "", "/", "www.shelfari.com")
                            {
                                Value = "{\"SpoilerShowAll\":true%2C\"SpoilerShowCharacters\":true%2C\"SpoilerBookId\":" +
                                        bookId +
                                        "%2C\"SpoilerShowPSS\":true%2C\"SpoilerShowQuotations\":true%2C\"SpoilerShowParents\":true%2C\"SpoilerShowThemes\":true}"
                            };
                            jar.Add(spoilers);
                        }
                        shelfariDoc.LoadHtml(client.GetPage());
                        break;
                    }
                    catch
                    {
                        if (tries <= 0)
                        {
                            Log("An error occurred connecting to Shelfari.");
                            return terms;
                        }
                    }
                }
                while (tries-- > 0);
            }
            else
            {
                shelfariDoc = sourceHtmlDoc;
            }

            //Constants for wiki processing
            Dictionary<string, string> sections = new Dictionary<string, string>
            {
                {"WikiModule_Characters", "character"},
                {"WikiModule_Organizations", "topic"},
                {"WikiModule_Settings", "topic"},
                {"WikiModule_Glossary", "topic"}
            }; //, {"WikiModule_Themes", "topic"} };
            string[] patterns = { @"""" };
            //, @"\[\d\]", @"\s*?\(.*\)\s*?" }; //Escape quotes, numbers in brackets, and anything within brackets at all
            string[] replacements = { @"\""" };

            //Parse elements from various headers listed in sections
            foreach (string header in sections.Keys)
            {
                //Select <li> nodes on page from within the <div id=header> tag, under <ul class=li_6>
                HtmlNodeCollection characterNodes =
                    shelfariDoc.DocumentNode.SelectNodes("//div[@id='" + header + "']//ul[@class='li_6']/li");
                if (characterNodes == null) continue; //Skip section if not found on page
                foreach (HtmlNode li in characterNodes)
                {
                    string tmpString = li.InnerText;
                    XRay.Term newTerm = new XRay.Term(sections[header]); //Create term as either character/topic
                    if (tmpString.Contains(":"))
                    {
                        newTerm.TermName = tmpString.Substring(0, tmpString.IndexOf(":"));
                        newTerm.Desc = tmpString.Substring(tmpString.IndexOf(":") + 1).Replace("&amp;", "&").Trim();
                    }
                    else
                    {
                        newTerm.TermName = tmpString;
                    }
                    //newTerm.TermName = newTerm.TermName.PregReplace(patterns, replacements);
                    //newTerm.Desc = newTerm.Desc.PregReplace(patterns, replacements);
                    newTerm.DescSrc = "shelfari";
                    //Use either the associated shelfari URL of the term or if none exists, use the book's url
                    //Could use a wikipedia page instead as the xray plugin/site does but I decided not to
                    newTerm.DescUrl = (li.InnerHtml.IndexOf("<a href") == 0
                        ? li.InnerHtml.Substring(9, li.InnerHtml.IndexOf("\"", 9) - 9)
                        : dataUrl);
                    if (header == "WikiModule_Glossary")
                        newTerm.MatchCase = false;
                    //Default glossary terms to be case insensitive when searching through book
                    if (terms.Select<XRay.Term, string>(t => t.TermName).Contains<string>(newTerm.TermName))
                        Log("Duplicate term \"" + newTerm.TermName + "\" found. Ignoring this duplicate.");
                    else
                        terms.Add(newTerm);
                }
            }
            return terms;
        }

        public override List<string[]> GetNotableQuotes(string dataUrl)
        {
            List<string[]> quotes = new List<string[]>();
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(HttpDownloader.GetPageHtml(dataUrl));
            }
            // Scrape quotes to attempt matching in ExpandRawML
            if (Properties.Settings.Default.useNewVersion)
            {
                HtmlNodeCollection quoteNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@id='WikiModule_Quotations']/div/ul[@class='li_6']/li");
                if (quoteNodes != null)
                {
                    foreach (HtmlNode quoteNode in quoteNodes)
                    {
                        HtmlNode node = quoteNode.SelectSingleNode(".//blockquote");
                        if (node == null) continue;
                        string quote = node.InnerText;
                        string character = "";
                        node = quoteNode.SelectSingleNode(".//cite");
                        if (node != null)
                            character = node.InnerText;
                        // Remove quotes (sometimes people put unnecessary quotes in the quote as well)
                        quote = Regex.Replace(quote, "^(&ldquo;){1,2}", "");
                        quote = Regex.Replace(quote, "(&rdquo;){1,2}$", "");
                        quotes.Add(new string[] { quote, character });
                    }
                }
            }
            return quotes;
        }
    }
}
