using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using NSubstitute;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;

namespace XRayBuilder.Test.DataSources
{
    [TestFixture]
    public sealed class LibraryThingTests
    {
        private ILogger _logger;
        private SecondarySourceLibraryThing _libraryThing;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _libraryThing = new SecondarySourceLibraryThing(_logger);
        }

        [Test]
        public async Task GetSeriesInfoTest()
        {
            var result = await _libraryThing.GetSeriesInfoAsync("http://www.librarything.com/work/3203350");

            Assert.AreEqual("The Lord of the Rings", result.Name);
            Assert.AreEqual("2", result.Position);
            Assert.NotNull(result.Next);
            Assert.AreEqual("J. R. R. Tolkien", result.Next.Author);
            Assert.AreEqual("The Return of The King", result.Next.Title);
            Assert.AreEqual("https://www.librarything.com/work/3203356", result.Next.DataUrl);
            Assert.NotNull(result.Previous);
            Assert.AreEqual("J. R. R. Tolkien", result.Previous.Author);
            Assert.AreEqual("The Fellowship of the Ring", result.Previous.Title);
            Assert.AreEqual("https://www.librarything.com/work/3203347", result.Previous.DataUrl);
            Assert.AreEqual(3, result.Total);
            Assert.AreEqual("https://www.librarything.com/nseries/2/The-Lord-of-the-Rings", result.Url);
        }

        [Test]
        public async Task SearchIsbnTest()
        {
            var testMetadata = Substitute.For<IMetadata>();
            testMetadata.Isbn.Returns("9780061952838");
            var result = await _libraryThing.SearchBookAsync(testMetadata, CancellationToken.None);

            Assert.AreEqual("https://www.librarything.com/work/3203347", result.Single().DataUrl);
        }

        [Test]
        public async Task SearchBookTest()
        {
            var testMetadata = Substitute.For<IMetadata>();
            testMetadata.Author.Returns("J. R. R. Tolkien");
            testMetadata.Title.Returns("The Fellowship of the Ring");
            var results = (await _libraryThing.SearchBookAsync(testMetadata, CancellationToken.None)).ToArray();

            Assert.AreEqual(12, results.Length);
            Assert.AreEqual("https://www.librarything.com/work/3203347", results[0].DataUrl);
            Assert.AreEqual("J. R. R. Tolkien", results[0].Author);
            Assert.AreEqual("The Fellowship of the Ring", results[0].Title);
        }

        [TestCase(true, 159)]
        [TestCase(false, 143)]
        public async Task GetTermsTest(bool includeTopics, int expectedCount)
        {
            var results = (await _libraryThing.GetTermsAsync("https://www.librarything.com/work/3203347", null, null, includeTopics, null, CancellationToken.None)).ToArray();

            Assert.AreEqual(expectedCount, results.Length);
        }

        [TestCase("http://www.librarything.com/work/3203347", true)]
        [TestCase("https://www.amazon.com/dp/B000000", false)]
        [TestCase("/usr/home/something/file.txt", false)]
        public void IsMatchingUrlTest(string url, bool expected)
        {
            Assert.AreEqual(expected, _libraryThing.IsMatchingUrl(url));
        }
    }
}