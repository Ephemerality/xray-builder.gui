using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources
{
    public class AuthorSearchResults
    {
        public string authorAsin;
        public HtmlDocument authorHtmlDoc;
    }

    public static class Amazon
    {
        public static bool IsAsin(string asin) => Regex.IsMatch(asin, "^B[A-Z0-9]{9}$");

        public static async Task<AuthorSearchResults> SearchAuthor(BookInfo curBook, string TLD)
        {
            AuthorSearchResults results = new AuthorSearchResults();
            //Generate Author search URL from author's name
            string newAuthor = Functions.FixAuthor(curBook.author);
            string plusAuthorName = newAuthor.Replace(" ", "+");
            //Updated to match Search "all" Amazon
            string amazonAuthorSearchUrl = $"https://www.amazon.{TLD}/s/ref=nb_sb_noss_2?url=search-alias%3Dstripbooks&field-keywords={plusAuthorName}";
            Logger.Log($"Searching for author's page on Amazon.{TLD}...");

            // Search Amazon for Author
            results.authorHtmlDoc = new HtmlDocument { OptionAutoCloseOnEnd = true };
            results.authorHtmlDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(amazonAuthorSearchUrl));

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    Logger.Log("Saving Amazon's author search webpage...");
                    File.WriteAllText(Environment.CurrentDirectory + $"\\dmp\\{curBook.asin}.authorsearchHtml.txt",
                        results.authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("An error ocurred saving authorsearchHtml.txt: {0}", ex.Message));
                }
            }

            // Check for captcha
            // TODO: Try to prompt for captcha and have user complete it to continue
            if (results.authorHtmlDoc.DocumentNode.InnerText.Contains("Robot Check"))
            {
                Logger.Log($"Warning: Amazon.{TLD} is requesting a captcha."
                    + $"You can try visiting Amazon.{TLD} in a real browser first, try another region, or try again later.");
            }
            // Try to find Author's page from Amazon search
            HtmlNode node = results.authorHtmlDoc.DocumentNode.SelectSingleNode("//*[@id='result_1']");
            if (node == null || !node.OuterHtml.Contains("/e/B"))
            {
                Logger.Log($"An error occurred finding author's page on Amazon.{TLD}." +
                                  "\r\nUnable to create Author Profile." +
                                  "\r\nEnsure the author metadata field matches the author's name exactly." +
                                  $"\r\nSearch results can be viewed at {amazonAuthorSearchUrl}");
                return null;
            }

            string properAuthor = "";
            // Check for typical search results, second item is the author page
            if ((node = node.SelectSingleNode("//*[@id='result_1']/div/div/div/div/a")) != null)
            {
                properAuthor = node.GetAttributeValue("href", "");
                results.authorAsin = node.GetAttributeValue("data-asin", null)
                    ?? AsinFromUrl(properAuthor);
            }
            // otherwise check for "by so-and-so" text beneath the titles for a possible match
            else if ((node = results.authorHtmlDoc.DocumentNode.SelectSingleNode($"//div[@id='resultsCol']//li[@class='s-result-item celwidget  ']//a[text()=\"{newAuthor}\"]")) != null)
            {
                properAuthor = node.GetAttributeValue("href", "");
                results.authorAsin = AsinFromUrl(properAuthor);
            }
            
            if (string.IsNullOrEmpty(properAuthor) || properAuthor.IndexOf('/', 1) < 3 || results.authorAsin == "")
            {
                Logger.Log("Unable to parse author's page URL properly. Try again later or report this URL on the MobileRead thread: " + amazonAuthorSearchUrl);
                return null;
            }
            properAuthor = properAuthor.Substring(1, properAuthor.IndexOf('/', 1) - 1);
            string authorAmazonWebsiteLocationLog = @"https://www.amazon." + TLD + "/" + properAuthor + "/e/" + results.authorAsin;
            string authorAmazonWebsiteLocation = @"https://www.amazon." + TLD + "/" + properAuthor + "/e/" + results.authorAsin +
                                              "/ref=la_" + results.authorAsin +
                                              "_rf_p_n_feature_browse-b_2?fst=as%3Aoff&rh=n%3A283155%2Cp_82%3A" +
                                              results.authorAsin +
                                              "%2Cp_n_feature_browse-bin%3A618073011&bbn=283155&ie=UTF8&qid=1432378570&rnid=618072011";

            curBook.authorAsin = results.authorAsin;
            Logger.Log($"Author page found on Amazon!\r\nAuthor's Amazon Page URL: {authorAmazonWebsiteLocationLog}");

            // Load Author's Amazon page
            string authorpageHtml;
            try
            {
                authorpageHtml = await HttpDownloader.GetPageHtmlAsync(authorAmazonWebsiteLocation);
            }
            catch
            {
                // If page not found (on co.uk at least, the long form does not seem to work) fallback to short form
                // and pray the formatting/item display suits our needs. If short form not found, crash back to caller.
                authorpageHtml = await HttpDownloader.GetPageHtmlAsync(authorAmazonWebsiteLocationLog);
            }
            results.authorHtmlDoc.LoadHtml(authorpageHtml);
            return results;
        }

        // Get biography from results page; TLD included in case different Amazon sites have different formatting
        public static HtmlNode GetBioNode(AuthorSearchResults searchResults, string TLD)
        {
            return searchResults.authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-bio' and @class='a-row']/div/div/span")
                   ?? searchResults.authorHtmlDoc.DocumentNode.SelectSingleNode("//span[@id='author_biography']")
                   ?? throw new DataSource.FormatChangedException(nameof(Amazon), "author bio");
        }

        public static HtmlNode GetAuthorImageNode(AuthorSearchResults searchResults, string TLD)
        {
            return searchResults.authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-image']/img")
                   ?? searchResults.authorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='authorImage']/img")
                   ?? throw new DataSource.FormatChangedException(nameof(Amazon), "author image");
        }

        /// <summary>
        /// As of 2018-07-31, format changed. For some amount of time, keep both just in case.
        /// TODO: Switch to Kindle section and grab only those instead
        /// </summary>
        public static List<BookInfo> GetAuthorBooksNew(AuthorSearchResults searchResults, string curTitle, string curAuthor, string TLD)
        {
            var resultsNodes = searchResults.authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='searchWidget']/div");
            if (resultsNodes == null) return null;
            var bookList = new List<BookInfo>(resultsNodes.Count);
            foreach (var result in resultsNodes)
            {
                if (result.InnerHtml.Contains("a-pagination"))
                    continue;
                var bookNodes = result.SelectNodes(".//div[@class='a-fixed-right-grid-inner']/div/div")
                    ?? throw new DataSource.FormatChangedException(nameof(Amazon), "book results - title nodes");
                var name = bookNodes.FirstOrDefault()?.SelectSingleNode("./a")?.InnerText.Trim()
                    ?? throw new DataSource.FormatChangedException(nameof(Amazon), "book results - title");
                //Exclude the current book title
                if (name.ContainsIgnorecase(curTitle)
                    || name.ContainsIgnorecase(@"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)"))
                    continue;

                // Get Kindle ASIN
                var asin = "";
                foreach (var bookNode in bookNodes)
                {
                    var match = Regex.Match(bookNode.OuterHtml, "(dp/(?<asin>B[A-Z0-9]{9})/|/gp/product/(?<asin>B[A-Z0-9]{9}))", RegexOptions.Compiled);
                    if (!match.Success)
                        continue;
                    asin = match.Groups["asin"].Value;
                    break;
                }

                // TODO: This should be removable when the Kindle Only page is parsed instead
                if (asin == "")
                    continue; //throw new DataSource.FormatChangedException(nameof(Amazon), "book results - kindle edition asin");
                bookList.Add(new BookInfo(name, curAuthor, asin)
                {
                    amazonUrl = $"https://www.amazon.{TLD}/dp/{asin}"
                });
            }
            return bookList;
        }

        public static List<BookInfo> GetAuthorBooks(AuthorSearchResults searchResults, string curTitle, string curAuthor, string TLD)
        {
            HtmlNodeCollection resultsNodes = searchResults.authorHtmlDoc.DocumentNode.SelectNodes("//div[@id='mainResults']/ul/li");
            if (resultsNodes == null) return null;
            List<BookInfo> bookList = new List<BookInfo>(resultsNodes.Count);
            foreach (HtmlNode result in resultsNodes)
            {
                if (!result.Id.StartsWith("result_")) continue;
                string asin = "";
                HtmlNode otherBook = result.SelectSingleNode(".//div[@class='a-row a-spacing-small']/a/h2");
                if (otherBook == null) continue;
                //Exclude the current book title from other books search
                if (Regex.Match(otherBook.InnerText, curTitle, RegexOptions.IgnoreCase).Success
                    || Regex.Match(otherBook.InnerText, @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)", RegexOptions.IgnoreCase).Success)
                    continue;
                var name = otherBook.InnerText.Trim();
                otherBook = result.SelectSingleNode(".//*[@title='Kindle Edition']");
                Match match = Regex.Match(otherBook.OuterHtml, "dp/(B[A-Z0-9]{9})/");
                if (match.Success)
                    asin = match.Groups[1].Value;
                var url = $"https://www.amazon.{TLD}/dp/{asin}";
                if (name != "" && url != "" && asin != "")
                {
                    BookInfo newBook = new BookInfo(name, curAuthor, asin) { amazonUrl = url };
                    bookList.Add(newBook);
                }
            }
            // If no kindle books returned, try the top carousel
            if (bookList.Count == 0)
            {
                resultsNodes = searchResults.authorHtmlDoc.DocumentNode.SelectNodes("//ol[@class='a-carousel' and @role ='list']/li");
                if (resultsNodes == null) return null;
                foreach (HtmlNode result in resultsNodes)
                {
                    string asin = "";
                    HtmlNode otherBook = result.SelectSingleNode(".//a/img");
                    if (otherBook == null) continue;
                    var name = otherBook.GetAttributeValue("alt", "");
                    //Exclude the current book title from other books search
                    if (Regex.Match(name, curTitle, RegexOptions.IgnoreCase).Success
                        || Regex.Match(name, @"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)", RegexOptions.IgnoreCase).Success)
                        continue;
                    otherBook = result.SelectSingleNode(".//a");
                    if (otherBook == null) continue;
                    Match match = Regex.Match(otherBook.OuterHtml, "dp/(B[A-Z0-9]{9})/");
                    if (match.Success)
                        asin = match.Groups[1].Value;
                    var url = $"https://www.amazon.{TLD}/dp/{asin}";
                    if (name != "" && url != "" && asin != "")
                    {
                        BookInfo newBook = new BookInfo(name, curAuthor, asin) { amazonUrl = url };
                        bookList.Add(newBook);
                    }
                }
            }
            return bookList;
        }

        // TODO: All calls to Amazon should check for the captcha page (or ideally avoid it somehow)
        public static async Task<BookInfo> SearchBook(string title, string author, string TLD)
        {
            BookInfo result = null;

            if (title.IndexOf(" (") >= 0)
                title = title.Substring(0, title.IndexOf(" ("));
            //Search "all" Amazon
            string searchUrl = String.Format(@"https://www.amazon.{0}/s/ref=nb_sb_noss?url=search-alias%3Daps&field-keywords={1}",
                TLD, Uri.EscapeDataString(title + " " + author));
            HtmlDocument searchDoc = new HtmlDocument();
            searchDoc.LoadHtml(await HttpDownloader.GetPageHtmlAsync(searchUrl));
            HtmlNode node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_0']");
            HtmlNode nodeASIN = node?.SelectSingleNode(".//a[@title='Kindle Edition']");
            if (nodeASIN == null)
            {
                node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_1']");
                nodeASIN = node?.SelectSingleNode(".//a[@title='Kindle Edition']");
            }
            //At least attempt to verify it might be the same book?
            if (node != null && nodeASIN != null && node.InnerText.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Match foundASIN = Regex.Match(nodeASIN.OuterHtml, "(B[A-Z0-9]{9})");
                node = node.SelectSingleNode(".//div/div/div/div[@class='a-fixed-left-grid-col a-col-right']/div/a");
                if (node != null)
                {
                    result = new BookInfo(node.InnerText, author, foundASIN.Value);
                    string trimUrl = nodeASIN.GetAttributeValue("href", "");
                    trimUrl = trimUrl.Substring(0, trimUrl.IndexOf(foundASIN.Value) + foundASIN.Length);
                    result.amazonUrl = trimUrl; // Grab the true link for good measure
                }
            }
            return result;
        }

        private static string AsinFromUrl(string url)
        {
            var asinMatch = Regex.Match(url, @"/e/(B\w+)/", RegexOptions.Compiled);
            return asinMatch.Success ? asinMatch.Groups[1].Value : "";
        }

        public static Task<string> DownloadStartActions(string asin)
            => HttpDownloader.GetPageHtmlAsync($"https://www.revensoftware.com/amazon/sa/{asin}");
    }
}
