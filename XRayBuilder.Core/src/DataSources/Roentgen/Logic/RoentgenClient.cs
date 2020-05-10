using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XRayBuilder.Core.DataSources.Amazon.Model;
using XRayBuilder.Core.DataSources.Roentgen.Model;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Roentgen.Logic
{
    public sealed class RoentgenClient : IRoentgenClient
    {
        private readonly IHttpClient _httpClient;

        public RoentgenClient(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private const string BaseUrl = "https://www.revensoftware.com/roentgen";
        private const string DownloadEndpoint = "/download";
        private string StartActionsEndpoint(string asin) => $"/startactions/{asin}";
        private string SeriesEndpoint(string asin) => $"/next/{asin}";
        private string PreloadEndpoint(string asin) => $"/preload/{asin}";

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

        public Task<StartActions> DownloadStartActionsAsync(string asin, string regionTld, CancellationToken cancellationToken)
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

        public Task<Term[]> DownloadTermsAsync(string asin, string regionTld, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(async () =>
            {
                var request = new StringContent(JsonUtil.Serialize(new DownloadRequest
                {
                    Asin = asin,
                    Type = DownloadRequest.TypeEnum.Terms,
                    Region = regionTld
                }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}{DownloadEndpoint}", request, cancellationToken);
                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseString = await new StreamReader(responseStream, Encoding.UTF8).ReadToEndAsync();
                return XmlUtil.Deserialize<Term[]>(responseString);
            });
        }

        public Task<EndActions> DownloadEndActionsAsync(string asin, string regionTld, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(() => DownloadArtifactAsync<EndActions>(asin, regionTld, DownloadRequest.TypeEnum.EndActions, cancellationToken));
        }

        public Task<AuthorProfile> DownloadAuthorProfileAsync(string asin, string regionTld, CancellationToken cancellationToken)
        {
            return HandleDownloadExceptionsAsync(() => DownloadArtifactAsync<AuthorProfile>(asin, regionTld, DownloadRequest.TypeEnum.AuthorProfile, cancellationToken));
        }

        private async Task<T> DownloadArtifactAsync<T>(string asin, string regionTld, DownloadRequest.TypeEnum type, CancellationToken cancellationToken)
        {
            var request = new StringContent(JsonUtil.Serialize(new DownloadRequest
            {
                Asin = asin,
                Type = type,
                Region = regionTld
            }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}{DownloadEndpoint}", request, cancellationToken);
            var responseStream = await response.Content.ReadAsStreamAsync();
            var responseString = await new StreamReader(responseStream, Encoding.UTF8).ReadToEndAsync();
            return JsonUtil.Deserialize<T>(responseString);
        }
    }
}