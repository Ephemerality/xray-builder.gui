using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XRayBuilder.Core.DataSources.Amazon.Model;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Roentgen.Logic
{
    public sealed class RoentgenClient
    {
        private readonly IHttpClient _httpClient;

        public RoentgenClient(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private const string BaseUrl = "https://www.revensoftware.com/roentgen";
        private string StartActionsEndpoint(string asin) => $"/startactions/{asin}";
        private string SeriesEndpoint(string asin) => $"/next/{asin}";
        private string PreloadEndpoint(string asin) => $"/preload/{asin}";
        private string DownloadEndpoint(string asin, string type) => $"/download/{asin}/{type}";

        private async Task<T> HandleDownloadExceptionsAsync<T>(Func<Task<T>> downloadTask) where T : class
        {
            try
            {
                return await downloadTask();
            }
            catch (HttpClientException ex) when (ex.Response.StatusCode == HttpStatusCode.BadRequest || ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // Invalid ASIN or not found
                return null;
            }
        }

        public Task<StartActions> DownloadStartActionsAsync(string asin, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(async () =>
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}{StartActionsEndpoint(asin)}", cancellationToken);
                return JsonUtil.Deserialize<StartActions>(response);
            });
        }

        public Task<NextBookResult> DownloadNextInSeriesAsync(string asin, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(async () =>
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}{SeriesEndpoint(asin)}", cancellationToken);
                return JsonConvert.DeserializeObject<NextBookResult>(response);
            });
        }

        public async Task PreloadAsync(string asin, CancellationToken cancellationToken)
        {
            try
            {
                await _httpClient.GetAsync($"{BaseUrl}{PreloadEndpoint(asin)}", cancellationToken);
            }
            catch
            {
                // Just ignore errors
            }
        }

        public Task<Term[]> DownloadTermsAsync(string asin, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(async () =>
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}{DownloadEndpoint(asin, "Terms")}", cancellationToken);
                return XmlUtil.Deserialize<Term[]>(response);
            });
        }

        public Task<EndActions> DownloadEndActionsAsync(string asin, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(async () =>
            {
                var response = await _httpClient.GetStringAsync($"{BaseUrl}{DownloadEndpoint(asin, "EndActions")}", cancellationToken);
                return JsonUtil.Deserialize<EndActions>(response);
            });
        }
    }
}