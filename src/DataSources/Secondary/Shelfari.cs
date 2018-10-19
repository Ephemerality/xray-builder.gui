using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources.Secondary
{
    public class Shelfari : ISecondarySource
    {
        private HtmlDocument sourceHtmlDoc;

        public string Name => "Shelfari";

        private string FindShelfariURL(HtmlDocument shelfariHtmlDoc, string author, string title)
        {
            // Try to find book's page from Shelfari search
            List<string> listofthings = new List<string>();
            List<string> listoflinks = new List<string>();
            Dictionary<string, string> retData = new Dictionary<string, string>();

            HtmlNode nodeResultCheck = shelfariHtmlDoc.DocumentNode.SelectSingleNode("//li[@class='item']/div[@class='text']");
            if (nodeResultCheck == null)
                return "";
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
                var index = 0;
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
                            var shelfariBookUrl = listoflinks[index];
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

        public Task<IEnumerable<BookInfo>> SearchBookAsync(string author, string title, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<BookInfo> GetNextInSeriesAsync(BookInfo curBook, AuthorProfile authorProfile, string TLD, CancellationToken cancellationToken = default)
        {
            BookInfo nextBook = null;

            if (curBook.dataUrl == "") return null;
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }

            // Get title of next book
            Dictionary<string, string> seriesInfo = await GetNextInSeriesTitle(curBook).ConfigureAwait(false);
            if (seriesInfo.TryGetValue("Next", out var title))
            {
                // Search author's other books for the book (assumes next in series was written by the same author...)
                // Returns the first one found, though there should probably not be more than 1 of the same name anyway
                nextBook = authorProfile.otherBooks.FirstOrDefault(bk => bk.title == title);
                if (nextBook == null)
                {
                    // Attempt to search Amazon for the book instead
                    nextBook = await Amazon.SearchBook(title, curBook.author, TLD);
                    if (nextBook != null)
                        await nextBook.GetAmazonInfo(nextBook.amazonUrl); //fill in desc, imageurl, and ratings
                }
                // Try to fill in desc, imageurl, and ratings using Shelfari Kindle edition link instead
                if (nextBook == null)
                {
                    HtmlDocument bookDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
                    bookDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(seriesInfo["NextURL"]));
                    Match match = Regex.Match(bookDoc.DocumentNode.InnerHtml, "('B[A-Z0-9]{9}')");
                    if (match.Success)
                    {
                        string cleanASIN = match.Value.Replace("'", String.Empty);
                        nextBook = new BookInfo(title, curBook.author, cleanASIN);
                        await nextBook.GetAmazonInfo("http://www.amazon.com/dp/" + cleanASIN);
                    }
                }
                if (nextBook == null)
                    Logger.Log("Book was found to be part of a series, but an error occured finding the next book.\r\n" +
                        "Please report this book and the Shelfari URL and output log to improve parsing.");

            }
            else if (curBook.seriesPosition != curBook.totalInSeries.ToString())
                Logger.Log("An error occured finding the next book in series, the book may not be part of a series, or it is the latest release.");

            if (seriesInfo.TryGetValue("Previous", out title))
            {
                if (curBook.previousInSeries == null)
                {
                    // Attempt to search Amazon for the book
                    curBook.previousInSeries = await Amazon.SearchBook(title, curBook.author, TLD);
                    if (curBook.previousInSeries != null)
                        await curBook.previousInSeries.GetAmazonInfo(curBook.previousInSeries.amazonUrl); //fill in desc, imageurl, and ratings

                    // Try to fill in desc, imageurl, and ratings using Shelfari Kindle edition link instead
                    if (curBook.previousInSeries == null)
                    {
                        HtmlDocument bookDoc = new HtmlDocument() { OptionAutoCloseOnEnd = true };
                        bookDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(seriesInfo["PreviousURL"]));
                        Match match = Regex.Match(bookDoc.DocumentNode.InnerHtml, "('B[A-Z0-9]{9}')");
                        if (match.Success)
                        {
                            string cleanASIN = match.Value.Replace("'", String.Empty);
                            curBook.previousInSeries = new BookInfo(title, curBook.author, cleanASIN);
                            await curBook.previousInSeries.GetAmazonInfo("http://www.amazon.com/dp/" + cleanASIN);
                        }
                    }
                }
                else
                    Logger.Log("Book was found to be part of a series, but an error occured finding the next book.\r\n" +
                        "Please report this book and the Shelfari URL and output log to improve parsing.");
            }

            return nextBook;
        }

        /// <summary>
        /// Search Shelfari page for possible series info, returning the next title in the series without downloading any other pages.
        /// </summary>
        private async Task<Dictionary<string, string>> GetNextInSeriesTitle(BookInfo curBook)
        {
            if (curBook.dataUrl == "") return null;
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }
            Dictionary<string, string> results = new Dictionary<string, string>(2);
            //Added estimated reading time and page count from Shelfari, for now...
            HtmlNode pageNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_FirstEdition']");
            if (pageNode == null)
                return results;
            HtmlNode node1 = pageNode.SelectSingleNode(".//div/div");
            if (node1 == null)
                return results;

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

                        Logger.Log("About the series: " + seriesType.InnerText.Replace(". (standard series)", ""));
                        curBook.seriesPosition = match.Groups[1].Value;
                        curBook.totalInSeries = int.Parse(match.Groups[2].Value);
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
                            Logger.Log("Preceded by: " + match.Groups[1].Value);
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
                                    Logger.Log("Followed by: " + match.Groups[1].Value);
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

        public async Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(curBook.dataUrl));
            }
            HtmlNode pageNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_FirstEdition']");
            HtmlNode node1 = pageNode?.SelectSingleNode(".//div/div");
            if (node1 == null)
                return false;
            //Parse page count and multiply by average reading time
            Match match1 = Regex.Match(node1.InnerText, @"Page Count: ((\d+)|(\d+,\d+))");
            if (match1.Success)
            {
                double minutes = int.Parse(match1.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                TimeSpan span = TimeSpan.FromMinutes(minutes);
                Logger.Log(String.Format("Typical time to read: {0} hours and {1} minutes ({2} pages)", span.Hours, span.Minutes, match1.Groups[1].Value));
                curBook.pagesInBook = int.Parse(match1.Groups[1].Value);
                curBook.readingHours = span.Hours;
                curBook.readingMinutes = span.Minutes;
                return true;
            }
            return false;
        }

        public Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<XRay.Term>> GetTermsAsync(string dataUrl, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            Logger.Log("Downloading Shelfari page...");
            List<XRay.Term> terms = new List<XRay.Term>();

            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = new HtmlDocument();
                sourceHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(dataUrl));
            }

            //Constants for wiki processing
            Dictionary<string, string> sections = new Dictionary<string, string>
            {
                {"WikiModule_Characters", "character"},
                {"WikiModule_Organizations", "topic"},
                {"WikiModule_Settings", "topic"},
                {"WikiModule_Glossary", "topic"}
            };

            foreach (string header in sections.Keys)
            {
                HtmlNodeCollection characterNodes =
                    sourceHtmlDoc.DocumentNode.SelectNodes("//div[@id='" + header + "']//ul[@class='li_6']/li");
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
                        newTerm.TermName = tmpString;
                    newTerm.DescSrc = "shelfari";
                    //Use either the associated shelfari URL of the term or if none exists, use the book's url
                    newTerm.DescUrl = (li.InnerHtml.IndexOf("<a href") == 0
                        ? li.InnerHtml.Substring(9, li.InnerHtml.IndexOf("\"", 9) - 9)
                        : dataUrl);
                    if (header == "WikiModule_Glossary")
                        newTerm.MatchCase = false;
                    //Default glossary terms to be case insensitive when searching through book
                    if (terms.Select(t => t.TermName).Contains(newTerm.TermName))
                        Logger.Log("Duplicate term \"" + newTerm.TermName + "\" found. Ignoring this duplicate.");
                    else
                        terms.Add(newTerm);
                }
            }
            return terms;
        }

        public async Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            if (srcDoc == null)
            {
                srcDoc = new HtmlDocument();
                srcDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(url));
            }
            List<NotableClip> result = new List<NotableClip>();
            HtmlNodeCollection quoteNodes = sourceHtmlDoc.DocumentNode.SelectNodes("//div[@id='WikiModule_Quotations']/div/ul[@class='li_6']/li");
            if (quoteNodes != null)
            {
                foreach (HtmlNode quoteNode in quoteNodes)
                {
                    HtmlNode node = quoteNode.SelectSingleNode(".//blockquote");
                    if (node == null) continue;
                    string quote = node.InnerText;
                    // Remove quotes (sometimes people put unnecessary quotes in the quote as well)
                    quote = Regex.Replace(quote, "^(&ldquo;){1,2}", "");
                    quote = Regex.Replace(quote, "(&rdquo;){1,2}$", "");
                    result.Add(new NotableClip { Text = quote, Likes = 0 });
                }
            }
            return result;
        }
    }
}
