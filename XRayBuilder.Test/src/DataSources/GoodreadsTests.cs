using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Test.DataSources
{
    [TestFixture]
    public class GoodreadsTests
    {
        private ILogger _logger;
        private IHttpClient _httpClient;
        private IAmazonClient _amazonClient;
        private SecondarySourceGoodreads _goodreads;
        private IAmazonInfoParser _amazonInfoParser;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _goodreads = new SecondarySourceGoodreads(_logger, _httpClient, _amazonClient);
        }

        [Test]
        public void NameTest()
        {
            Assert.AreEqual("Goodreads", _goodreads.Name);
        }

        [Test]
        public async Task SearchBookTest()
        {
            var results = (await _goodreads.SearchBookAsync("George R. R. Martin", "A Feast for Crows")).ToArray();
            Assert.GreaterOrEqual(results.Length, 1);
            var first = results.First();
            Assert.AreEqual(first.Author, "George R.R. Martin");
            Assert.AreEqual(first.GoodreadsId, "13497");
            Assert.False(string.IsNullOrEmpty(first.ImageUrl));
            Assert.AreEqual(first.Title, "A Feast for Crows (A Song of Ice and Fire, #4)");
            Assert.Greater(first.Editions, 0);
        }

        [Test]
        public async Task GetSeriesInfoTest()
        {
            var result = await _goodreads.GetSeriesInfoAsync("https://www.goodreads.com/book/show/13497");
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Name, "A Song of Ice and Fire");
            Assert.False(string.IsNullOrEmpty(result.Url));
            Assert.AreEqual(result.Position, "4");
            Assert.Greater(result.Total, 0);

            Assert.IsNotNull(result.Next);
            Assert.AreEqual(result.Next.Author, "George R.R. Martin");
            Assert.AreEqual(result.Next.GoodreadsId, "10664113");
            Assert.AreEqual(result.Next.Title, "A Dance with Dragons");

            Assert.IsNotNull(result.Previous);
            Assert.AreEqual(result.Previous.Author, "George R.R. Martin");
            Assert.AreEqual(result.Previous.GoodreadsId, "62291");
            Assert.AreEqual(result.Previous.Title, "A Storm of Swords");
        }

        [Test]
        public async Task SearchBookAsinTest()
        {
            var result = await _goodreads.SearchBookASINById("13497");
            Assert.AreEqual("B004P1JEXE", result);
        }

        [Test]
        public async Task GetPageCountTest()
        {
            var book = new BookInfo("", "", "") { DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows" };
            var result = await _goodreads.GetPageCountAsync(book);
            Assert.True(result);
            Assert.AreEqual(book.PagesInBook, 1061);
            Assert.AreEqual(book.ReadingHours, 22);
            Assert.AreEqual(book.ReadingMinutes, 47);
        }

        [Test]
        public async Task GetTermsTest()
        {
            var results = (await _goodreads.GetTermsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows", null)).ToArray();
            Assert.AreEqual(results.Length, 15);
        }

        [Test]
        public async Task GetNotableClipsTest()
        {
            var results = (await _goodreads.GetNotableClipsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows")).ToArray();
            Assert.GreaterOrEqual(results.Length, 500);
        }

        [Test]
        public async Task GetExtrasTest()
        {
            var book = new BookInfo("", "", "") { DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows" };
            await _goodreads.GetExtrasAsync(book);
            Assert.Greater(book.AmazonRating, 0);
            Assert.GreaterOrEqual(book.notableClips.Count, 500);
            Assert.GreaterOrEqual(book.Reviews, 1);
        }
    }
}