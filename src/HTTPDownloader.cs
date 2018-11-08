using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    // Taken from http://stackoverflow.com/a/2700707
    // Downloads an HTML page using appropriate encoding
    // TODO: More refactoring
    public sealed class HttpDownloader
    {
        private readonly string _referer;
        private readonly string _userAgent;
        private readonly CookieContainer _cookiejar = new CookieContainer();
        private bool _encodingFoundInHeader;

        public Encoding Encoding { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public Uri Url { get; set; }

        private readonly Regex _headerCharsetRegex = new Regex(@";\s*charset\s*=\s*(?<charset>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex _metaCharsetRegex = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Task<string> GetPageHtmlAsync(string url, CancellationToken cancellationToken = default)
        {
            var http = new HttpDownloader(url);
            return Task.Run(async () => await http.GetPageAsync(cancellationToken).ConfigureAwait(false), cancellationToken);
        }

        public static async Task<HtmlDocument> GetHtmlDocAsync(string url, CancellationToken cancellationToken = default)
        {
            var http = new HttpDownloader(url);
            var doc = new HtmlDocument();
            doc.Load(await http.GetPageAsync(cancellationToken).ConfigureAwait(false));
            return doc;
        }

        public static async Task<Bitmap> GetImageAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 2000;
            using (var response = await request.GetResponseAsync(cancellationToken))
            {
                var stream = response.GetResponseStream()
                             ?? throw new Exception($"Failed to download image ({response.StatusCode} {response.StatusDescription})");
                return new Bitmap(stream);
            }
        }

        public HttpDownloader(string url) : this(url, null, null, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.75 Safari/537.36") { }

        public HttpDownloader(string url, CookieContainer jar, string referer, string userAgent)
        {
            Encoding = Encoding.GetEncoding("ISO-8859-1");
            Url = new Uri(url); // verify the uri
            _userAgent = userAgent;
            _referer = referer;
            if (jar != null) _cookiejar = jar;
        }

        public async Task<string> GetPageAsync(CancellationToken cancellationToken = default)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (!string.IsNullOrEmpty(_referer))
                request.Referer = _referer;
            if (!string.IsNullOrEmpty(_userAgent))
                request.UserAgent = _userAgent;

            request.CookieContainer = _cookiejar;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");

            using (HttpWebResponse response = await request.GetResponseAsync(cancellationToken))
            {
                Headers = response.Headers;
                Url = response.ResponseUri;
                return ProcessContent(response);
            }
        }

        private string ProcessContent(HttpWebResponse response)
        {
            SetEncodingFromHeader(response);

            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                return null;
            if (response.ContentEncoding.ToLower().Contains("gzip"))
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

            var memStream = new MemoryStream();
            responseStream.CopyTo(memStream);
            responseStream.Close();
            memStream.Position = 0;
            using (var reader = new StreamReader(memStream, Encoding))
            {
                var html = reader.ReadToEnd().Trim();
                if (!_encodingFoundInHeader)
                    html = CheckMetaCharSetAndReEncode(memStream, html);

                return html;
            }
        }

        private void SetEncodingFromHeader(HttpWebResponse response)
        {
            var charset = response.CharacterSet;
            if (string.IsNullOrEmpty(charset))
            {
                var m = _headerCharsetRegex.Match(response.ContentType);
                if (m.Success)
                {
                    charset = m.Groups["charset"].Value.Trim('\'', '"');
                    _encodingFoundInHeader = true;
                }
            }

            if (string.IsNullOrEmpty(charset))
                return;

            try
            {
                Encoding = Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
            }
        }

        private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
        {
            var match = _metaCharsetRegex.MatchOrNull(html);
            if (match == null)
                return html;

            var charset = match.Groups["charset"].Value.ToLower();
            if (charset == "unicode" || charset == "utf-16")
                charset = "utf-8";

            try
            {
                var metaEncoding = Encoding.GetEncoding(charset);
                if (!Encoding.Equals(metaEncoding))
                {
                    memStream.Position = 0L;
                    var recodeReader = new StreamReader(memStream, metaEncoding);
                    html = recodeReader.ReadToEnd().Trim();
                    recodeReader.Close();
                }
            }
            catch (ArgumentException)
            {
            }

            return html;
        }
    }

    public static partial class ExtensionMethods
    {
        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(request.Abort, false))
            {
                try
                {
                    var response = await request.GetResponseAsync();
                    return (HttpWebResponse)response;
                }
                catch (WebException ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException(ex.Message, ex, cancellationToken);
                    throw;
                }
            }
        }
    }
}
