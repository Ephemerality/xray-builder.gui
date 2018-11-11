using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    // TODO: DI for this instead of static instance
    // TODO: Investigate if a caching layer is worthwhile
    public sealed class HttpClient : System.Net.Http.HttpClient
    {
        public static HttpClient Instance = null;

        public HttpClient(ILogger logger) : base(
            new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip
            }.DecorateWith(new TimeoutHandler())
            .DecorateWith(new RetryHandler())
            .DecorateWith(new AmazonHandler(logger)))
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        }

        public static async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
        {
            var response = await GetStreamAsync(url, cancellationToken);
            return new StreamReader(response, Encoding.UTF8).ReadToEnd();
        }

        public static async Task<HtmlDocument> GetPageAsync(string url, CancellationToken cancellationToken = default)
        {
            var htmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            var stream = await GetStreamAsync(url, cancellationToken);
            htmlDoc.Load(stream);
            return htmlDoc;
        }

        public static async Task<Bitmap> GetImageAsync(string url, CancellationToken cancellationToken = default)
        {
            var response = await GetStreamAsync(url, cancellationToken);
            return new Bitmap(response);
        }

        public static async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultSettings(request);
            var response = await Instance.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStreamAsync();
        }

        private static void SetDefaultSettings(HttpRequestMessage request)
        {
            request.Headers.Add(KnownHeaders.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0");
        }
    }

    public static class KnownHeaders
    {
        public const string UserAgent = "User-Agent";
    }

    // https://www.thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
    public static class HttpClientExtensions
    {
        private const string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
                return timeout;

            return null;
        }

        public static HttpMessageHandler DecorateWith(this HttpMessageHandler innerHandler, DelegatingHandler delgateHandler)
        {
            delgateHandler.InnerHandler = innerHandler;
            return delgateHandler;
        }
    }

    public sealed class TimeoutHandler : DelegatingHandler
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var cts = GetCancellationTokenSource(request, cancellationToken))
            {
                try
                {
                    return await base.SendAsync(request, cts?.Token ?? cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }
            }
        }

        private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? DefaultTimeout;

            // No need to create a CTS if there's no timeout
            if (timeout == Timeout.InfiniteTimeSpan)
                return null;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            return cts;
        }
    }

    // https://www.thomaslevesque.com/2016/12/08/fun-with-the-httpclient-pipeline/
    public sealed class RetryHandler : DelegatingHandler
    {
        public int MaxRetries { get; set; } = 3;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tries = 1;
            while (true)
            {
                try
                {
                    // call the inner handler
                    var response = await base.SendAsync(request, cancellationToken);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.ServiceUnavailable:
                            await Task.Delay(5000, cancellationToken);
                            continue;
                        // Too many requests
                        case (HttpStatusCode)429:
                            await Task.Delay(1000, cancellationToken);
                            continue;
                    }

                    // Not something we can retry, return the response as is
                    return response;
                }
                catch (Exception ex) when (tries++ < MaxRetries && !(ex is OperationCanceledException))
                {
                    // Wait a bit and try again later
                    await Task.Delay(2000, cancellationToken);
                }
            }
        }
    }

    public sealed class AmazonHandler : DelegatingLoggingHandler
    {
        public AmazonHandler(ILogger logger) : base(logger)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    throw new HttpClientException(response.StatusCode.ToString())
                    {
                        Response = response
                    };
                return response;
            }
            catch (HttpClientException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound
                && request.RequestUri.Host.Contains(".amazon.")
                && !request.RequestUri.Host.EndsWith(".com"))
            {
                var originalHost = request.RequestUri.Host;
                var builder = new UriBuilder(request.RequestUri)
                {
                    Host = "www.amazon.com"
                };
                request.RequestUri = builder.Uri;
                var response = await base.SendAsync(request, cancellationToken);
                Logger?.Log($"Not available from {originalHost}, but found on Amazon US ({request.RequestUri})");
                return response;
            }
        }
    }

    public class DelegatingLoggingHandler : DelegatingHandler
    {
        protected readonly ILogger Logger;

        protected DelegatingLoggingHandler(ILogger logger = null)
        {
            Logger = logger;
        }
    }

    public class HttpClientException : Exception
    {
        public HttpClientException(string message, Exception previous = null) : base(message, previous) { }

        public HttpResponseMessage Response { get; set; }

    }
}
