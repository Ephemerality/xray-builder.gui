using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    [UsedImplicitly]
    public sealed class SecondarySourceShelfari : ISecondarySource
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;

        private HtmlDocument sourceHtmlDoc;

        public SecondarySourceShelfari(ILogger logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public string Name => "Shelfari";
        public bool SearchEnabled { get; } = false;
        public int UrlLabelPosition { get; } = 6;
        public bool SupportsNotableClips { get; } = true;

        // private string FindShelfariURL(HtmlDocument shelfariHtmlDoc, string author, string title)
        // {
        //     // Try to find book's page from Shelfari search
        //     var listofthings = new List<string>();
        //     var listoflinks = new List<string>();
        //
        //     var nodeResultCheck = shelfariHtmlDoc.DocumentNode.SelectSingleNode("//li[@class='item']/div[@class='text']");
        //     if (nodeResultCheck == null)
        //         return "";
        //     foreach (var bookItems in shelfariHtmlDoc.DocumentNode.SelectNodes("//li[@class='item']/div[@class='text']"))
        //     {
        //         if (bookItems == null) continue;
        //         listofthings.Clear();
        //         listoflinks.Clear();
        //         for (var i = 1; i < bookItems.ChildNodes.Count; i++)
        //         {
        //             if (bookItems.ChildNodes[i].GetAttributeValue("class", "") == "series") continue;
        //             listofthings.Add(bookItems.ChildNodes[i].InnerText.Trim());
        //             listoflinks.Add(bookItems.ChildNodes[i].InnerHtml);
        //         }
        //         var index = 0;
        //         foreach (var line in listofthings)
        //         {
        //             // Search for author with spaces removed to avoid situations like "J.R.R. Tolkien" / "J. R. R. Tolkien"
        //             // Ignore Collective Work search result.
        //             // May cause false matches, we'll see.
        //             // Also remove diacritics from titles when matching just in case...
        //             // Searching for Children of Húrin will give a false match on the first pass before diacritics are removed from the search URL
        //             if ((listofthings.Contains("(Author)") || listofthings.Contains("(Author),")) &&
        //                 line.RemoveDiacritics().StartsWith(title.RemoveDiacritics(), StringComparison.OrdinalIgnoreCase) &&
        //                 (listofthings.Contains(author) || listofthings.Exists(r => r.Replace(" ", "") == author.Replace(" ", ""))))
        //                 if (!listoflinks.Any(c => c.Contains("(collective work)")))
        //                 {
        //                     var shelfariBookUrl = listoflinks[index];
        //                     shelfariBookUrl = Regex.Replace(shelfariBookUrl, "<a href=\"", "", RegexOptions.None);
        //                     shelfariBookUrl = Regex.Replace(shelfariBookUrl, "\".*?</a>.*", "", RegexOptions.None);
        //                     if (shelfariBookUrl.ToLower().StartsWith("http://"))
        //                         return shelfariBookUrl;
        //                 }
        //             index++;
        //         }
        //     }
        //     return "";
        // }

        public bool IsMatchingUrl(string url)
        {
            return false;
        }

        public Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
            => Task.FromResult((SeriesInfo) null);

        public async Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = await _httpClient.GetPageAsync(curBook.DataUrl, cancellationToken);
            }
            var pageNode = sourceHtmlDoc.DocumentNode.SelectSingleNode("//div[@id='WikiModule_FirstEdition']");
            var node1 = pageNode?.SelectSingleNode(".//div/div");
            if (node1 == null)
                return false;
            //Parse page count and multiply by average reading time
            var match1 = Regex.Match(node1.InnerText, @"Page Count: ((\d+)|(\d+,\d+))");
            if (match1.Success)
            {
                var minutes = int.Parse(match1.Groups[1].Value, NumberStyles.AllowThousands) * 1.2890625;
                var span = TimeSpan.FromMinutes(minutes);
                _logger.Log(string.Format("Typical time to read: {0} hours and {1} minutes ({2} pages)", span.Hours, span.Minutes, match1.Groups[1].Value));
                curBook.PagesInBook = int.Parse(match1.Groups[1].Value);
                curBook.ReadingHours = span.Hours;
                curBook.ReadingMinutes = span.Minutes;
                return true;
            }
            return false;
        }

        public Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public async Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            _logger.Log("Downloading Shelfari page...");
            var terms = new List<Term>();

            if (sourceHtmlDoc == null)
            {
                sourceHtmlDoc = await _httpClient.GetPageAsync(dataUrl, cancellationToken);
            }

            //Constants for wiki processing
            var sections = new Dictionary<string, string>
            {
                {"WikiModule_Characters", "character"},
                {"WikiModule_Organizations", "topic"},
                {"WikiModule_Settings", "topic"},
                {"WikiModule_Glossary", "topic"}
            };

            foreach (var header in sections.Keys)
            {
                var characterNodes =
                    sourceHtmlDoc.DocumentNode.SelectNodes("//div[@id='" + header + "']//ul[@class='li_6']/li");
                if (characterNodes == null) continue; //Skip section if not found on page
                foreach (var li in characterNodes)
                {
                    var tmpString = li.InnerText;
                    //Create term as either character/topic
                    var newTerm = new Term
                    {
                        Type = sections[header]
                    };
                    if (tmpString.Contains(":"))
                    {
                        newTerm.TermName = tmpString.Substring(0, tmpString.IndexOf(":", StringComparison.Ordinal));
                        newTerm.Desc = tmpString.Substring(tmpString.IndexOf(":", StringComparison.Ordinal) + 1).Replace("&amp;", "&").Trim();
                    }
                    else
                        newTerm.TermName = tmpString;
                    newTerm.DescSrc = "shelfari";
                    //Use either the associated shelfari URL of the term or if none exists, use the book's url
                    newTerm.DescUrl = li.InnerHtml.IndexOf("<a href", StringComparison.Ordinal) == 0
                        ? li.InnerHtml.Substring(9, li.InnerHtml.IndexOf("\"", 9, StringComparison.Ordinal) - 9)
                        : dataUrl;
                    if (header == "WikiModule_Glossary")
                        newTerm.MatchCase = false;
                    //Default glossary terms to be case insensitive when searching through book
                    if (terms.Select(t => t.TermName).Contains(newTerm.TermName))
                        _logger.Log("Duplicate term \"" + newTerm.TermName + "\" found. Ignoring this duplicate.");
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
                srcDoc = await _httpClient.GetPageAsync(url, cancellationToken);
            }
            var quoteNodes = srcDoc.DocumentNode.SelectNodes("//div[@id='WikiModule_Quotations']/div/ul[@class='li_6']/li");
            if (quoteNodes == null)
                return Enumerable.Empty<NotableClip>();

            return quoteNodes
                .Select(quoteNode => quoteNode.SelectSingleNode(".//blockquote"))
                .Where(node => node != null)
                .Select(node => node.InnerText)
                .Select(quote => Regex.Replace(quote, "^(&ldquo;){1,2}", ""))
                .Select(quote => Regex.Replace(quote, "(&rdquo;){1,2}$", ""))
                .Select(quote => new NotableClip
                {
                    Text = quote,
                    Likes = 0
                });
        }
    }
}
