using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using HtmlAgilityPack;
using Newtonsoft.Json;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Secondary
{
    public sealed class SecondarySourceLibraryThing : SecondarySource
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly Random _random;

        public SecondarySourceLibraryThing(ILogger logger)
        {
            _logger = logger;
            _httpClient = new Libraries.Http.HttpClient(logger, false);
            _random = new Random();
        }

        public override string Name { get; } = "LibraryThing";
        public override bool SearchEnabled { get; } = true;
        public override int UrlLabelPosition { get; } = 14;
        public override bool SupportsNotableClips { get; } = false;

        private const string BaseUrl = "https://www.librarything.com";
        private string IsbnSearchEndpoint(string isbn) => $"/isbn/{isbn}";

        private readonly Regex _metadataRegex = new Regex(@"((?<reviews>\d+(,\d+)?) reviews?, )?((?<editions>\d+(,\d+)?) editions?, )?.+((?<rating>\d+(\.\d+)?) stars?)?", RegexOptions.Compiled);
        private readonly Regex _idRegex = new Regex(@"(?<id>\d+)", RegexOptions.Compiled);
        private readonly Regex _seriesRegex = new Regex(@"(?<name>.+) \((?<index>\d+)\)$", RegexOptions.Compiled);
        private readonly Regex _orderRegex = new Regex(@"Order: (?<index>\d+)$", RegexOptions.Compiled);

        private const int MaxConcurrentRequests = 10;

        private string SearchEndpoint(string author, string title)
        {
            title = Uri.EscapeDataString(title);
            author = Uri.EscapeDataString(Functions.FixAuthor(author));
            return $"/ajax_newsearch.php?search={author}%20{title}&searchtype=newwork_titles&page=1&sortchoice=0&optionidpotential=0&optionidreal=0&randomnumber={_random.Next()}";
        }

        private string GetCookies()
            => $"LTAnonSessionID={_random.Next()}; LTUnifiedCookie=%7B%22areyouhuman%22%3A1%7D; cookie_from=https%3A%2F%2Fwww.librarything.com%2F; canuseStaticDomain=0; gdpr_notice_clicked=1";

        public override bool IsMatchingUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.ToLowerInvariant().EndsWith("librarything.com");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override async Task<IEnumerable<BookInfo>> SearchBookAsync(IMetadata metadata, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(metadata.Isbn))
            {
                var isbnResult = await SearchIsbnAsync(metadata.Isbn, cancellationToken);
                if (isbnResult != null)
                {
                    return new[] {isbnResult};
                }
            }

            var result = await GetAsync($"{BaseUrl}{SearchEndpoint(metadata.Author, metadata.Title)}", cancellationToken);
            var response = JsonUtil.Deserialize<SearchResultPayload>(await result.Content.ReadAsStringAsync());
            var htmlPayload = Encoding.UTF8.GetString(Convert.FromBase64String(response.Text));

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlPayload);

            IEnumerable<BookInfo> ParseResults()
            {
                var resultNodes = htmlDoc.DocumentNode.SelectNodes("//table/p/tr");
                foreach (var node in resultNodes)
                {
                    var dataUrl = node.SelectSingleNode(".//a").GetAttributeValue("href", null);
                    var imageUrl = node.SelectSingleNode(".//img").GetAttributeValue("src", null);
                    var titleAuthorNodes = node.SelectNodes(".//p[@class='item']/a");
                    var dataNode = node.SelectSingleNode(".//p[@class='si']");
                    var metadataMatch = _metadataRegex.Match(dataNode.InnerText);
                    var idMatch = _idRegex.Match(dataUrl);

                    if (!metadataMatch.Success || !idMatch.Success || titleAuthorNodes.Count != 2)
                        continue;

                    yield return new BookInfo(titleAuthorNodes[0].InnerText, titleAuthorNodes[1].InnerText, null)
                    {
                        AmazonRating = double.TryParse(metadataMatch.Groups["rating"].Value, out var rating)
                            ? rating
                            : 0.0,
                        Editions = metadataMatch.Groups["editions"].Value.TryParseInt() ?? 0,
                        Reviews = metadataMatch.Groups["reviews"].Value.TryParseInt() ?? 0,
                        DataUrl = $"{BaseUrl}{dataUrl}",
                        ImageUrl = imageUrl,
                        GoodreadsId = idMatch.Groups["id"].Value
                    };
                }
            }

            return ParseResults().ToArray();
        }

        private sealed class SearchResultPayload
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("script")]
            public string Script { get; set; }
        }

        private async Task<BookInfo> SearchIsbnAsync(string isbn, CancellationToken cancellationToken)
        {
            try
            {
                await GetAsync($"{BaseUrl}{IsbnSearchEndpoint(isbn)}", cancellationToken);
            }
            catch (HttpClientException ex) when (ex.Response.StatusCode == HttpStatusCode.Found)
            {
                string path;
                if (ex.Response.Headers.TryGetValues("location", out var location) && (path = location.First()).Contains("/work/"))
                {
                    return new BookInfo("", "", "")
                    {
                        DataUrl = $"{BaseUrl}{path}"
                    };
                }
            }

            return null;
        }

        public override async Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            var page = await GetPageAsync(dataUrl, cancellationToken);
            var seriesNode = page.DocumentNode
                .SelectSingleNode("//div[@id='seriesforwork_container']/h2[contains(text(), 'Belongs to Series')]")
                ?.NextSibling;
            if (seriesNode == null)
                return null;
            var seriesUrl = seriesNode.SelectSingleNode("./a").GetAttributeValue("href", null);
            var seriesMatch = _seriesRegex.Match(seriesNode.InnerText);
            if (!seriesMatch.Success || seriesUrl == null)
                return null;

            var seriesIndex = int.Parse(seriesMatch.Groups["index"].Value);
            seriesUrl = $"{BaseUrl}{seriesUrl}";

            var seriesPage = await GetPageAsync(seriesUrl, cancellationToken);
            var coreNode = seriesPage.DocumentNode.SelectSingleNode("//h2[contains(text(), 'Core')]")?.NextSibling;
            if (coreNode == null)
                return null;
            var tableNodes = coreNode.SelectNodes(".//tr");
            var seriesBooks = tableNodes.Select(node =>
            {
                var columns = node.SelectNodes("./td");
                if (columns.Count != 2 || !_orderRegex.IsMatch(columns[1].InnerText))
                    return null;
                var authorTitleNodes = columns[0].SelectNodes(".//a");
                if (authorTitleNodes.Count != 2)
                    return null;
                var url = authorTitleNodes[0].GetAttributeValue("href", null);
                if (dataUrl == null)
                    return null;

                return new BookInfo(authorTitleNodes[0].InnerText, authorTitleNodes[1].InnerText, null)
                {
                    DataUrl = $"{BaseUrl}{url}"
                };
            }).Where(book => book != null).ToArray();

            if (seriesBooks.Length == 0 || seriesIndex - 1 > seriesBooks.Length)
                return null;

            var previous = seriesIndex - 2 >= 0
                ? seriesBooks[seriesIndex - 2]
                : null;
            var next = seriesIndex < seriesBooks.Length
                ? seriesBooks[seriesIndex]
                : null;

            return new SeriesInfo
            {
                Name = seriesMatch.Groups["name"].Value,
                Url = seriesUrl,
                Position = seriesIndex.ToString(),
                Total = seriesBooks.Length,
                Previous = previous,
                Next = next
            };
        }

        public override Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public override Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public override async Task<IEnumerable<Term>> GetTermsAsync(string dataUrl, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            _logger.Log($"Downloading {Name} page…");
            var page = await GetPageAsync(dataUrl, cancellationToken);
            var characterNodes = page.DocumentNode.SelectNodes("//div[@class='fwikiItem divcharacternames']//a");
            if (characterNodes == null)
                return Enumerable.Empty<Term>();
            _logger.Log($"Gathering term information from {Name}… ({characterNodes.Count})");
            progress?.Set(0, characterNodes.Count);
            if (characterNodes.Count > 20)
                _logger.Log("More than 20 characters found. Consider using the 'download to XML' option if you need to build repeatedly.");

            var terms = new ConcurrentBag<Term>();
            await characterNodes.ParallelForEachAsync(async charNode =>
            {
                var url = charNode.GetAttributeValue("href", null);
                if (url == null)
                    return;

                try
                {
                    terms.AddNotNull(await GetTermAsync(charNode.InnerText, url, cancellationToken).ConfigureAwait(false));
                    progress?.Add(1);
                }
                catch (Exception ex) when (ex.Message.Contains("(404)"))
                {
                    _logger.Log($"Error getting page for character - {charNode.InnerText}. URL: {url}\r\nMessage: {ex.Message}\r\n{ex.StackTrace}");
                }
            }, MaxConcurrentRequests, cancellationToken);

            if (includeTopics)
            {
                var placeNodes = page.DocumentNode.SelectNodes("//div[@class='fwikiItem divplacesmentioned']//a");
                foreach (var node in placeNodes)
                {
                    var placeUrl = node.GetAttributeValue("href", null);
                    terms.Add(new Term
                    {
                        Type = "topic",
                        DescSrc = Name,
                        Desc = "",
                        TermName = node.InnerText,
                        DescUrl = placeUrl != null ? $"{BaseUrl}{placeUrl}" : null
                    });
                }
            }

            return terms.ToArray();
        }

        private async Task<Term> GetTermAsync(string termName, string relativeUrl, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}{relativeUrl}";
            var charDoc = await GetPageAsync(url, cancellationToken);
            var description = charDoc.DocumentNode.SelectSingleNode("//div[@class='fwikiItem divdescription']")?.InnerText.Clean();

            return new Term
            {
                Type = "character",
                DescSrc = Name,
                Desc = description,
                TermName = termName,
                DescUrl = url
            };
        }

        public override Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public async Task<HtmlDocument> GetPageAsync(string url, CancellationToken cancellationToken)
        {
            var htmlDoc = new HtmlDocument
            {
                OptionAutoCloseOnEnd = true
            };
            var result = await GetAsync(url, cancellationToken);
            var stream = await result.Content.ReadAsStreamAsync();
            htmlDoc.Load(stream, Encoding.UTF8);
            return htmlDoc;
        }

        private Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Cookie", GetCookies());
            return _httpClient.SendAsync(request, cancellationToken);
        }
    }
}