using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            _readingTimeService = new ReadingTimeService();
            _goodreads = new SecondarySourceGoodreads(_logger, _httpClient, _amazonClient, _readingTimeService);
        }

        [Test]
        public void NameTest()
        {
            ClassicAssert.AreEqual("Goodreads", _goodreads.Name);
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
            ClassicAssert.GreaterOrEqual(results.Length, 1);
            var first = results.First();
            ClassicAssert.AreEqual(first.Author, "George R.R. Martin");
            ClassicAssert.AreEqual("13497", first.GoodreadsId);
            ClassicAssert.False(string.IsNullOrEmpty(first.ImageUrl));
            ClassicAssert.AreEqual("A Feast for Crows (A Song of Ice and Fire, #4)", first.Title);
            ClassicAssert.Greater(first.Editions, 0);
        }

        [Test]
        public async Task GetSeriesInfoTest()
        {
            var result = await _goodreads.GetSeriesInfoAsync("https://www.goodreads.com/book/show/13497");
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("A Song of Ice and Fire", result.Name);
            ClassicAssert.False(string.IsNullOrEmpty(result.Url));
            ClassicAssert.AreEqual("4", result.Position);
            ClassicAssert.Greater(result.Total, 0);

            ClassicAssert.IsNotNull(result.Next);
            ClassicAssert.AreEqual(result.Next.Author, "George R.R. Martin");
            ClassicAssert.AreEqual("10664113", result.Next.GoodreadsId);
            ClassicAssert.AreEqual("A Dance with Dragons", result.Next.Title);

            ClassicAssert.IsNotNull(result.Previous);
            ClassicAssert.AreEqual(result.Previous.Author, "George R.R. Martin");
            ClassicAssert.AreEqual("62291", result.Previous.GoodreadsId);
            ClassicAssert.AreEqual("A Storm of Swords", result.Previous.Title);
        }

        [Test]
        public async Task SearchBookAsinTest()
        {
            var result = await _goodreads.SearchBookASINById("13497");
            var possibleAsins = new[] {"B000FCKGPC", "BINU9MFSUG"};
            ClassicAssert.IsTrue(possibleAsins.Contains(result), $"{result} was not expected");
        }

        [Test]
        public async Task GetPageCountTest()
        {
            var book = new BookInfo("", "", "")
            {
                DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows"
            };
            var result = await _goodreads.GetPageCountAsync(book);
            ClassicAssert.True(result);
            ClassicAssert.AreEqual(1061, book.PageCount);
            ClassicAssert.AreEqual(19, book.ReadingHours);
            ClassicAssert.AreEqual(25, book.ReadingMinutes);
        }

        [Test]
        public async Task GetTermsTest()
        {
            var results = (await _goodreads.GetTermsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows", null, "com", true, null)).ToArray();
            ClassicAssert.AreEqual(15, results.Length);
        }

        [Test]
        public async Task GetNotableClipsTest()
        {
            var results = (await _goodreads.GetNotableClipsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows")).ToArray();
            ClassicAssert.GreaterOrEqual(results.Length, 500);
        }

        [Test]
        public async Task GetExtrasTest()
        {
            var book = new BookInfo("", "", "")
            {
                DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows"
            };
            await _goodreads.GetExtrasAsync(book);
            ClassicAssert.NotNull(book.AmazonRating);
            ClassicAssert.Greater(book.AmazonRating, 0);
            ClassicAssert.NotNull(book.NotableClips);
            ClassicAssert.GreaterOrEqual(book.NotableClips.Count, 500);
            ClassicAssert.NotNull(book.Reviews);
            ClassicAssert.GreaterOrEqual(book.Reviews, 1);
        }

        [TestCase("https://www.goodreads.com/book/show/4214.Life_of_Pi", true)]
        [TestCase("https://www.amazon.com/dp/B000000", false)]
        [TestCase("/usr/home/something/file.txt", false)]
        public void IsMatchingUrlTest(string url, bool expected)
        {
            ClassicAssert.AreEqual(expected, _goodreads.IsMatchingUrl(url));
        }
    }
}