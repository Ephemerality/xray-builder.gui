﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Newtonsoft.Json;
using XRayBuilderGUI.DataSources.Amazon.Model;

namespace XRayBuilderGUI.DataSources.Amazon
{
    public class AuthorSearchResults
    {
        public string AuthorAsin { get; set; }
        public HtmlDocument AuthorHtmlDoc { get; set; }
    }

    // TODO: Calling SearchAuthor then using the search results for all subsequent calls is kinda weird
    public static class Amazon
    {
        private static readonly Regex RegexAsin = new Regex("(?<asin>B[A-Z0-9]{9})", RegexOptions.Compiled);
        private static readonly Regex RegexAsinUrl = new Regex("(/e/(?<asin>B\\w+)/|dp/(?<asin>B[A-Z0-9]{9})/|/gp/product/(?<asin>B[A-Z0-9]{9}))", RegexOptions.Compiled);
        private static readonly Regex RegexIgnoreHeaders = new Regex(@"(Series|Reading) Order|Checklist|Edition|eSpecial|\([0-9]+ Book Series\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsAsin(string asin) => Regex.IsMatch(asin, "^B[A-Z0-9]{9}$");

        [CanBeNull]
        public static string ParseAsin(string input) => RegexAsin.MatchOrNull(input)?.Groups["asin"].Value;

        [CanBeNull]
        public static string ParseAsinFromUrl(string input) => RegexAsinUrl.MatchOrNull(input)?.Groups["asin"].Value;

        public static string Url(string tld, string asin) => $"https://www.amazon.{tld}/dp/{asin}";

        public static async Task<AuthorSearchResults> SearchAuthor(BookInfo curBook, string TLD, ILogger _logger, CancellationToken cancellationToken = default)
        {
            var results = new AuthorSearchResults();
            //Generate Author search URL from author's name
            var newAuthor = Functions.FixAuthor(curBook.Author);
            var plusAuthorName = newAuthor.Replace(" ", "+");
            //Updated to match Search "all" Amazon
            var amazonAuthorSearchUrl = $"https://www.amazon.{TLD}/s/ref=nb_sb_noss_2?url=search-alias%3Dstripbooks&field-keywords={plusAuthorName}";
            _logger.Log($"Searching for author's page on Amazon.{TLD}...");

            // Search Amazon for Author
            results.AuthorHtmlDoc = await HttpClient.GetPageAsync(amazonAuthorSearchUrl, cancellationToken);

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    _logger.Log("Saving Amazon's author search webpage...");
                    File.WriteAllText(Environment.CurrentDirectory + $"\\dmp\\{curBook.Asin}.authorsearchHtml.txt",
                        results.AuthorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("An error ocurred saving authorsearchHtml.txt: {0}", ex.Message));
                }
            }

            // Check for captcha
            // TODO: Try to prompt for captcha and have user complete it to continue
            if (results.AuthorHtmlDoc.DocumentNode.InnerText.Contains("Robot Check"))
            {
                _logger.Log($"Warning: Amazon.{TLD} is requesting a captcha."
                    + $"You can try visiting Amazon.{TLD} in a real browser first, try another region, or try again later.");
            }
            // Try to find Author's page from Amazon search
            var node = results.AuthorHtmlDoc.DocumentNode.SelectSingleNode("//*[@id='result_1']");
            if (node == null || !node.OuterHtml.Contains("/e/B"))
            {
                _logger.Log($"An error occurred finding author's page on Amazon.{TLD}." +
                                  "\r\nUnable to create Author Profile." +
                                  "\r\nEnsure the author metadata field matches the author's name exactly." +
                                  $"\r\nSearch results can be viewed at {amazonAuthorSearchUrl}" +
                                  "\r\nSometimes Amazon just doesn't return the author and trying a few times will work.");
                return null;
            }

            var properAuthor = "";
            // Check for typical search results, second item is the author page
            if ((node = node.SelectSingleNode("//*[@id='result_1']/div/div/div/div/a")) != null)
            {
                properAuthor = node.GetAttributeValue("href", "");
                results.AuthorAsin = node.GetAttributeValue("data-asin", null)
                    ?? ParseAsinFromUrl(properAuthor);
            }
            // otherwise check for "by so-and-so" text beneath the titles for a possible match
            else if ((node = results.AuthorHtmlDoc.DocumentNode.SelectSingleNode($"//div[@id='resultsCol']//li[@class='s-result-item celwidget  ']//a[text()=\"{newAuthor}\"]")) != null)
            {
                properAuthor = node.GetAttributeValue("href", "");
                results.AuthorAsin = ParseAsinFromUrl(properAuthor);
            }

            if (node == null || string.IsNullOrEmpty(properAuthor) || properAuthor.IndexOf('/', 1) < 3 || string.IsNullOrEmpty(results.AuthorAsin))
            {
                _logger.Log("Unable to parse author's page URL properly. Try again later or report this URL on the MobileRead thread: " + amazonAuthorSearchUrl);
                return null;
            }
            properAuthor = properAuthor.Substring(1, properAuthor.IndexOf('/', 1) - 1);
            var authorAmazonWebsiteLocationLog = $"https://www.amazon.{TLD}/{properAuthor}/e/{results.AuthorAsin}";
            var authorAmazonWebsiteLocation = $"https://www.amazon.{TLD}{node.GetAttributeValue("href", "")}";

            curBook.AuthorAsin = results.AuthorAsin;
            _logger.Log($"Author page found on Amazon!\r\nAuthor's Amazon Page URL: {authorAmazonWebsiteLocationLog}");

            // Load Author's Amazon page
            var tempDoc = new HtmlDocument();
            var authorPage = await HttpClient.GetStreamAsync(authorAmazonWebsiteLocation, cancellationToken);
            tempDoc.Load(authorPage);

            // Try to find the Kindle Edition link
            // TODO: don't handle individual regions here...
            if (TLD == "com")
            {
                var kindleNode = tempDoc.DocumentNode.SelectSingleNode(".//a[@class='a-link-normal formatSelector']");
                if (kindleNode != null && kindleNode.InnerText.Trim() == "Kindle Edition")
                {
                    authorPage = await HttpClient.GetStreamAsync($"https://www.amazon.com{kindleNode.GetAttributeValue("href", "")}", cancellationToken);
                    tempDoc.Load(authorPage);
                }
            }
            else if (TLD == "co.uk")
            {
                var kindleNode = tempDoc.DocumentNode.SelectSingleNode(".//a[@class='a-link-normal']");
                if (kindleNode != null && kindleNode.InnerText.Trim() == "Kindle Books")
                {
                    authorPage = await HttpClient.GetStreamAsync($"https://www.amazon.co.uk{kindleNode.GetAttributeValue("href", "")}", cancellationToken);
                    tempDoc.Load(authorPage);
                }
            }

            // Either use the new one w/ only Kindle editions or the original
            results.AuthorHtmlDoc = tempDoc;

            return results;
        }

        // Get biography from results page; TLD included in case different Amazon sites have different formatting
        public static HtmlNode GetBioNode(AuthorSearchResults searchResults, string TLD)
        {
            return searchResults.AuthorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-bio' and @class='a-row']/div/div/span")
                   ?? searchResults.AuthorHtmlDoc.DocumentNode.SelectSingleNode("//span[@id='author_biography']")
                   ?? throw new FormatChangedException(nameof(Amazon), "author bio");
        }

        public static HtmlNode GetAuthorImageNode(AuthorSearchResults searchResults, string TLD)
        {
            return searchResults.AuthorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='ap-image']/img")
                   ?? searchResults.AuthorHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='authorImage']/img")
                   ?? throw new FormatChangedException(nameof(Amazon), "author image");
        }

        /// <summary>
        /// As of 2018-07-31, format changed. For some amount of time, keep both just in case.
        /// </summary>
        public static List<BookInfo> GetAuthorBooksNew(AuthorSearchResults searchResults, string curTitle, string curAuthor, string TLD)
        {
            var resultsNodes = searchResults.AuthorHtmlDoc.DocumentNode.SelectNodes("//div[@id='searchWidget']/div");
            if (resultsNodes == null) return null;
            var bookList = new List<BookInfo>(resultsNodes.Count);
            foreach (var result in resultsNodes)
            {
                if (result.InnerHtml.Contains("a-pagination"))
                    continue;
                var bookNodes = result.SelectNodes(".//div[@class='a-fixed-right-grid-inner']/div/div")
                    ?? throw new FormatChangedException(nameof(Amazon), "book results - title nodes");
                var name = bookNodes.FirstOrDefault()?.SelectSingleNode("./a")?.InnerText.Trim()
                    ?? throw new FormatChangedException(nameof(Amazon), "book results - title");
                //Exclude the current book title
                if (name.ContainsIgnorecase(curTitle)
                    || RegexIgnoreHeaders.IsMatch(name))
                    continue;

                // Get first Kindle ASIN
                var asin = "";
                foreach (var bookNode in bookNodes)
                {
                    var match = RegexAsinUrl.Match(bookNode.OuterHtml);
                    if (!match.Success)
                        continue;
                    asin = match.Groups["asin"].Value;
                    break;
                }

                // TODO: This should be removable when the Kindle Only page is parsed instead
                if (asin == "")
                    continue; //throw new DataSource.FormatChangedException(nameof(Amazon), "book results - kindle edition asin");
                bookList.Add(new BookInfo(name, curAuthor, asin));
            }
            return bookList;
        }

        public static List<BookInfo> GetAuthorBooks(AuthorSearchResults searchResults, string curTitle, string curAuthor, string TLD)
        {
            var resultsNodes = searchResults.AuthorHtmlDoc.DocumentNode.SelectNodes("//div[@id='mainResults']/ul/li");
            if (resultsNodes == null) return null;
            var bookList = new List<BookInfo>(resultsNodes.Count);
            // TODO: Reduce this duplication
            foreach (var result in resultsNodes)
            {
                if (!result.Id.StartsWith("result_")) continue;
                var otherBook = result.SelectSingleNode(".//div[@class='a-row a-spacing-small']/div/a/h2");
                if (otherBook == null) continue;
                //Exclude the current book title from other books search
                if (otherBook.InnerText.ContainsIgnorecase(curTitle) || RegexIgnoreHeaders.IsMatch(otherBook.InnerText))
                    continue;
                var name = otherBook.InnerText.Trim();
                otherBook = result.SelectSingleNode(".//*[@title='Kindle Edition']");
                var asin = ParseAsinFromUrl(otherBook.OuterHtml);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(asin))
                    bookList.Add(new BookInfo(name, curAuthor, asin));
            }
            // If no kindle books returned, try the top carousel
            if (bookList.Count == 0)
            {
                resultsNodes = searchResults.AuthorHtmlDoc.DocumentNode.SelectNodes("//ol[@class='a-carousel' and @role ='list']/li");
                if (resultsNodes == null) return null;
                foreach (var result in resultsNodes)
                {
                    var otherBook = result.SelectSingleNode(".//a/img");
                    if (otherBook == null) continue;
                    var name = otherBook.GetAttributeValue("alt", "");
                    //Exclude the current book title from other books search
                    if (name.ContainsIgnorecase(curTitle) || RegexIgnoreHeaders.IsMatch(name))
                        continue;
                    otherBook = result.SelectSingleNode(".//a");
                    if (otherBook == null) continue;
                    var asin = ParseAsinFromUrl(otherBook.OuterHtml);
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(asin))
                        bookList.Add(new BookInfo(name, curAuthor, asin));
                }
            }
            return bookList;
        }

        // TODO: All calls to Amazon should check for the captcha page (or ideally avoid it somehow)
        public static async Task<BookInfo> SearchBook(string title, string author, string TLD, CancellationToken cancellationToken = default)
        {
            BookInfo result = null;

            if (title.IndexOf(" (") >= 0)
                title = title.Substring(0, title.IndexOf(" ("));
            //Search "all" Amazon
            var searchUrl = string.Format(@"https://www.amazon.{0}/s/ref=nb_sb_noss?url=search-alias%3Daps&field-keywords={1}",
                TLD, Uri.EscapeDataString(title + " " + author));
            var searchDoc = await HttpClient.GetPageAsync(searchUrl, cancellationToken);
            var node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_0']");
            var nodeASIN = node?.SelectSingleNode(".//a[@title='Kindle Edition']");
            if (nodeASIN == null)
            {
                node = searchDoc.DocumentNode.SelectSingleNode("//li[@id='result_1']");
                nodeASIN = node?.SelectSingleNode(".//a[@title='Kindle Edition']");
            }
            //At least attempt to verify it might be the same book?
            if (node != null && nodeASIN != null && node.InnerText.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var foundAsin = ParseAsinFromUrl(nodeASIN.OuterHtml);
                node = node.SelectSingleNode(".//div/div/div/div[@class='a-fixed-left-grid-col a-col-right']/div/a");
                if (node != null)
                    result = new BookInfo(node.InnerText, author, foundAsin);
            }
            return result;
        }

        public static Task<string> DownloadStartActions(string asin, CancellationToken cancellationToken = default)
            => HttpClient.GetStringAsync($"https://www.revensoftware.com/amazon/sa/{asin}", cancellationToken);

        public static async Task<NextBookResult> DownloadNextInSeries(string asin, CancellationToken cancellationToken = default)
        {
            var response = await HttpClient.GetStringAsync($"https://www.revensoftware.com/amazon/next/{asin}", cancellationToken);
            return JsonConvert.DeserializeObject<NextBookResult>(response);
        }
    }
}
