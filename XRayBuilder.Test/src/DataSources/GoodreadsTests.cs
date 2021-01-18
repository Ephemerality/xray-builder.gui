using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;

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
            var testMetadata = Substitute.For<IMetadata>();
            testMetadata.Author.Returns("George R. R. Martin");
            testMetadata.Title.Returns("A Feast for Crows");
            var results = (await _goodreads.SearchBookAsync(testMetadata)).ToArray();
            Assert.GreaterOrEqual(results.Length, 1);
            var first = results.First();
            Assert.AreEqual(first.Author, "George R.R. Martin");
            Assert.AreEqual(first.GoodreadsId, "13497");
            Assert.False(string.IsNullOrEmpty(first.ImageUrl));
            Assert.AreEqual(first.Title, "A Feast for Crows (A Song of Ice and Fire, #4)");
            Assert.Greater(first.Editions, 0);
        }

        [Test]
        // TODO Goodreads is currently showing the wrong primary series, will need to fix this test once they fix it
        // The book has a second numbering variation where each book has 2 parts
        // Because they are displaying the wrong series info for books in the standard series, these are all wrong
        // (but the code technically works)
        public async Task GetSeriesInfoTest()
        {
            var result = await _goodreads.GetSeriesInfoAsync("https://www.goodreads.com/book/show/13497");
            Assert.IsNotNull(result);
            Assert.AreEqual("A Song of Ice and Fire (1-in-2)", result.Name);
            Assert.False(string.IsNullOrEmpty(result.Url));
            Assert.AreEqual("8", result.Position);
            Assert.Greater(result.Total, 0);

            Assert.IsNotNull(result.Next);
            Assert.AreEqual(result.Next.Author, "George R.R. Martin");
            Assert.AreEqual("13337715", result.Next.GoodreadsId);
            Assert.AreEqual("A Dance with Dragons: Dreams and Dust", result.Next.Title);

            Assert.IsNotNull(result.Previous);
            Assert.AreEqual(result.Previous.Author, "George R.R. Martin");
            Assert.AreEqual("6383644", result.Previous.GoodreadsId);
            Assert.AreEqual("O Festim dos Corvos", result.Previous.Title);
        }

        [Test]
        public async Task SearchBookAsinTest()
        {
            var result = await _goodreads.SearchBookASINById("13497");
            var possibleAsins = new[] {"B000FCKGPC", "BINU9MFSUG"};
            Assert.IsTrue(possibleAsins.Contains(result), $"{result} was not expected");
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
            var results = (await _goodreads.GetTermsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows", null, "com", true, null)).ToArray();
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
            Assert.GreaterOrEqual(book.NotableClips.Count, 500);
            Assert.GreaterOrEqual(book.Reviews, 1);
        }

        [TestCase("https://www.goodreads.com/book/show/4214.Life_of_Pi", true)]
        [TestCase("https://www.amazon.com/dp/B000000", false)]
        [TestCase("/usr/home/something/file.txt", false)]
        public void IsMatchingUrlTest(string url, bool expected)
        {
            Assert.AreEqual(expected, _goodreads.IsMatchingUrl(url));
        }
    }
}