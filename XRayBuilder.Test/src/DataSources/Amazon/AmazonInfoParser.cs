using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;

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
            BookInfo bk = new BookInfo("A Game of Thrones", "George R. R. Martin", "B000QCS8TW");
            var response = await _amazonInfoParser.GetAndParseAmazonDocument("https://www.amazon.ca/Game-Thrones-Song-Fire-Book-ebook/dp/B000QCS8TW/");
            Assert.Greater(response.Reviews, 0);
            Assert.Greater(response.Rating, 0);
            Assert.IsNotEmpty(response.ImageUrl);
            Assert.IsNotEmpty(response.Description);
        }

        [Test()]
        public async Task CoverImageTest()
        {
            BookInfo bk = new BookInfo("A Game of Thrones", "George R. R. Martin", "B000QCS8TW");
            var response = await _amazonInfoParser.GetAndParseAmazonDocument("https://www.amazon.ca/Game-Thrones-Song-Fire-Book-ebook/dp/B000QCS8TW/");
            Assert.IsNotNull(response.ImageUrl);
            Assert.IsNotNull(await _httpClient.GetImageAsync(response.ImageUrl));
            Assert.Greater(response.Rating, 0);
            Assert.Greater(response.Reviews, 0);
        }
    }
}
