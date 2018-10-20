using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilderGUI.DataSources.Secondary.Model;

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

        public Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
            => Task.FromResult((SeriesInfo) null);

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
