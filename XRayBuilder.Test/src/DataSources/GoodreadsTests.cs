using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using NSubstitute;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Logic;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Logic.ReadingTime;
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
        private IReadingTimeService _readingTimeService;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _goodreads = new SecondarySourceGoodreads(_logger, _httpClient, _amazonClient, _readingTimeService);
            _readingTimeService = new ReadingTimeService();
        }

        [Test]
        public void NameTest()
        {
            Assert.AreEqual("Goodreads", _goodreads.Name);
        }

        // Todo separate test for goodreads search book vs booksearchservice
        [Test]
        public async Task SearchBookTest()
        {
            var testMetadata = Substitute.For<IMetadata>();
            testMetadata.Author.Returns("George R. R. Martin");
            testMetadata.Title.Returns("A Feast for Crows");
            var bookSearchService = new BookSearchService();
            var results = await bookSearchService.SearchSecondarySourceAsync(_goodreads, testMetadata, CancellationToken.None);
            Assert.GreaterOrEqual(results.Length, 1);
            var first = results.First();
            Assert.AreEqual(first.Author, "George R.R. Martin");
            Assert.AreEqual("13497", first.GoodreadsId);
            Assert.False(string.IsNullOrEmpty(first.ImageUrl));
            Assert.AreEqual("A Feast for Crows (A Song of Ice and Fire, #4)", first.Title);
            Assert.Greater(first.Editions, 0);
        }

        [Test]
        public async Task GetSeriesInfoTest()
        {
            var result = await _goodreads.GetSeriesInfoAsync("https://www.goodreads.com/book/show/13497");
            Assert.IsNotNull(result);
            Assert.AreEqual("A Song of Ice and Fire", result.Name);
            Assert.False(string.IsNullOrEmpty(result.Url));
            Assert.AreEqual("4", result.Position);
            Assert.Greater(result.Total, 0);

            Assert.IsNotNull(result.Next);
            Assert.AreEqual(result.Next.Author, "George R.R. Martin");
            Assert.AreEqual("10664113", result.Next.GoodreadsId);
            Assert.AreEqual("A Dance with Dragons", result.Next.Title);

            Assert.IsNotNull(result.Previous);
            Assert.AreEqual(result.Previous.Author, "George R.R. Martin");
            Assert.AreEqual("62291", result.Previous.GoodreadsId);
            Assert.AreEqual("A Storm of Swords", result.Previous.Title);
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
            var book = new BookInfo("", "", "")
            {
                DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows"
            };
            var result = await _goodreads.GetPageCountAsync(book);
            Assert.True(result);
            Assert.AreEqual(1061, book.PageCount);
            Assert.AreEqual(19, book.ReadingHours);
            Assert.AreEqual(25, book.ReadingMinutes);
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
            var book = new BookInfo("", "", "")
            {
                DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows"
            };
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