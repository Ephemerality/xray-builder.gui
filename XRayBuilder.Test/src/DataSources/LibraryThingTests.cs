using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            var result = await _libraryThing.GetSeriesInfoAsync("https://www.librarything.com/work/3203350");

            ClassicAssert.AreEqual("The Lord of the Rings", result.Name);
            ClassicAssert.AreEqual("2", result.Position);
            ClassicAssert.NotNull(result.Next);
            ClassicAssert.AreEqual("J. R. R. Tolkien", result.Next.Author);
            ClassicAssert.AreEqual("The Return of The King", result.Next.Title);
            ClassicAssert.AreEqual("https://www.librarything.com/work/3203356", result.Next.DataUrl);
            ClassicAssert.NotNull(result.Previous);
            ClassicAssert.AreEqual("J. R. R. Tolkien", result.Previous.Author);
            ClassicAssert.AreEqual("The Fellowship of the Ring", result.Previous.Title);
            ClassicAssert.AreEqual("https://www.librarything.com/work/3203347", result.Previous.DataUrl);
            ClassicAssert.AreEqual(3, result.Total);
            ClassicAssert.AreEqual("https://www.librarything.com/nseries/2/The-Lord-of-the-Rings", result.Url);
        }

        [Test]
        public async Task SearchIsbnTest()
        {
            var testMetadata = Substitute.For<IMetadata>();
            testMetadata.Isbn.Returns("9780061952838");
            var result = await _libraryThing.SearchBookAsync(testMetadata, CancellationToken.None);

            ClassicAssert.AreEqual("https://www.librarything.com/work/3203347", result.Single().DataUrl);
        }

        [Test]
        public async Task SearchBookTest()
        {
            var testMetadata = Substitute.For<IMetadata>();
            testMetadata.Author.Returns("J. R. R. Tolkien");
            testMetadata.Title.Returns("The Fellowship of the Ring");
            var results = (await _libraryThing.SearchBookAsync(testMetadata, CancellationToken.None)).ToArray();

            ClassicAssert.Greater(results.Length, 0);
            ClassicAssert.AreEqual("https://www.librarything.com/work/3203347", results[0].DataUrl);
            ClassicAssert.AreEqual("J. R. R. Tolkien", results[0].Author);
            ClassicAssert.AreEqual("The Fellowship of the Ring", results[0].Title);
        }

        [TestCase(true, 166)]
        [TestCase(false, 150)]
        public async Task GetTermsTest(bool includeTopics, int expectedCount)
        {
            var results = (await _libraryThing.GetTermsAsync("https://www.librarything.com/work/3203347", null, null, includeTopics, null, CancellationToken.None)).ToArray();

            ClassicAssert.AreEqual(expectedCount, results.Length);
        }

        [TestCase("http://www.librarything.com/work/3203347", true)]
        [TestCase("https://www.amazon.com/dp/B000000", false)]
        [TestCase("/usr/home/something/file.txt", false)]
        public void IsMatchingUrlTest(string url, bool expected)
        {
            ClassicAssert.AreEqual(expected, _libraryThing.IsMatchingUrl(url));
        }
    }
}