using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;

namespace XRayBuilder.Test.DataSources.Amazon
{
    [TestFixture]
    public class AmazonInfoParserTests
    {
        private IHttpClient _httpClient;
        private ILogger _logger;
        private IAmazonInfoParser _amazonInfoParser;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
        }

        [Test()]
        public async Task GetAmazonInfoTest()
        {
            var response = await _amazonInfoParser.GetAndParseAmazonDocument("https://www.amazon.ca/Game-Thrones-Song-Fire-Book-ebook/dp/B000QCS8TW/");
            ClassicAssert.Greater(response.Reviews, 0);
            ClassicAssert.Greater(response.Rating, 0);
            ClassicAssert.IsNotEmpty(response.ImageUrl);
            ClassicAssert.IsNotEmpty(response.Description);
        }

        [Test()]
        public async Task CoverImageTest()
        {
            var response = await _amazonInfoParser.GetAndParseAmazonDocument("https://www.amazon.ca/Game-Thrones-Song-Fire-Book-ebook/dp/B000QCS8TW/");
            ClassicAssert.IsNotNull(response.ImageUrl);
            ClassicAssert.IsNotNull(await _httpClient.GetImageAsync(response.ImageUrl));
            ClassicAssert.Greater(response.Rating, 0);
            ClassicAssert.Greater(response.Reviews, 0);
        }
    }
}
