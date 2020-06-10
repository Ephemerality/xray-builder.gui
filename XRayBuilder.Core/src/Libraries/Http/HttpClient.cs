﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilder.Core.Libraries.Images.Extensions;
using XRayBuilder.Core.Libraries.Logging;

namespace XRayBuilder.Core.Libraries.Http
{
    // TODO: Investigate if a caching layer is worthwhile
    public sealed class HttpClient : System.Net.Http.HttpClient, IHttpClient
    {
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

        public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
        {
            var response = await GetStreamAsync(url, cancellationToken);
            return await new StreamReader(response, Encoding.UTF8).ReadToEndAsync();
        }

        public async Task<HtmlDocument> GetPageAsync(string url, CancellationToken cancellationToken = default)
        {
            var htmlDoc = new HtmlDocument {OptionAutoCloseOnEnd = true};
            var stream = await GetStreamAsync(url, cancellationToken);
            htmlDoc.Load(stream, Encoding.UTF8);
            return htmlDoc;
        }

        // TODO should this return image not bitmap?
        public async Task<Bitmap> GetImageAsync(string url, bool greyscale = false, CancellationToken cancellationToken = default)
        {
            var response = await GetStreamAsync(url, cancellationToken);
            var image = new Bitmap(response);

            image = greyscale
                ? (Bitmap) image.ToGrayscale3()
                : image;

            return image;
        }

        public async IAsyncEnumerable<Bitmap> GetImages(IEnumerable<string> urls, bool greyscale = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var url in urls.Where(url => !string.IsNullOrEmpty(url)))
            {
                yield return await GetImageAsync(url, greyscale, cancellationToken);
            }
        }

        public async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // TODO Put this somewhere else
            if (url.Contains("amazon.com"))
            {
                request.Headers.Add(KnownHeaders.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            }
            SetDefaultSettings(request);
            var response = await SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStreamAsync();
        }

        private static void SetDefaultSettings(HttpRequestMessage request)
        {
            request.Headers.Add(KnownHeaders.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36");
            request.Headers.Add(KnownHeaders.AcceptEncoding,"gzip, deflate, br");
        }
    }

    public static class KnownHeaders
    {
        public const string UserAgent = "User-Agent";
        public const string Accept = "Accept";
        public const string AcceptEncoding = "Accept-Encoding";
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
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(15);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var cts = GetCancellationTokenSource(request, cancellationToken);
            try
            {
                return await base.SendAsync(request, cts?.Token ?? cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }

        private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? _defaultTimeout;

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
        private const int MaxRetries = 3;

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
                        case (HttpStatusCode) 418:
                            await Task.Delay(5000, cancellationToken);
                            continue;
                        // Too many requests
                        case (HttpStatusCode) 429:
                            await Task.Delay(1000, cancellationToken);
                            continue;
                    }

                    // Not something we can retry, return the response as is
                    return response;
                }
                catch (HttpRequestException ex) when (ex.InnerException is WebException webEx && webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    // No internet, just give up
                    throw;
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

    public sealed class HttpClientException : Exception
    {
        public HttpClientException(string message, Exception previous = null) : base(message, previous) { }

        public HttpResponseMessage Response { get; set; }

    }
}
