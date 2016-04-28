using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources
{
    class GoodReads : DataSource
    {
        public override string Name { get { return "GoodReads"; } }

        public override string SearchBook(string author, string title)
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
    }
}
