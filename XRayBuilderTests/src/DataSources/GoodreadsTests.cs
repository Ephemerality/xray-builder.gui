using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilderGUI;
using XRayBuilderGUI.DataSources.Secondary;

namespace XRayBuilderTests.DataSources
{
    [TestFixture]
    public class GoodreadsTests
    {
        [Test]
        public void NameTest()
        {
            Assert.AreEqual(new Goodreads(new Logger()).Name, nameof(Goodreads));
        }

        [Test]
        public async Task SearchBookTest()
        {
            var gr = new Goodreads(new Logger());
            var results = (await gr.SearchBookAsync("George R. R. Martin", "A Feast for Crows")).ToArray();
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
            var gr = new Goodreads(new Logger());
            var result = await gr.GetSeriesInfoAsync("https://www.goodreads.com/book/show/13497");
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
            var gr = new Goodreads(new Logger());
            var result = await gr.SearchBookASIN("13497");
            Assert.AreEqual(result, "B000FCKGPC");
        }

        [Test]
        public async Task GetPageCountTest()
        {
            var gr = new Goodreads(new Logger());
            var book = new BookInfo("", "", "") { DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows" };
            var result = await gr.GetPageCountAsync(book);
            Assert.True(result);
            Assert.AreEqual(book.PagesInBook, 1061);
            Assert.AreEqual(book.ReadingHours, 22);
            Assert.AreEqual(book.ReadingMinutes, 47);
        }

        [Test]
        public async Task GetTermsTest()
        {
            var gr = new Goodreads(new Logger());
            var results = (await gr.GetTermsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows", null)).ToArray();
            Assert.AreEqual(results.Length, 15);
        }

        [Test]
        public async Task GetNotableClipsTest()
        {
            var gr = new Goodreads(new Logger());
            var results = (await gr.GetNotableClipsAsync("https://www.goodreads.com/book/show/13497.A_Feast_for_Crows")).ToArray();
            Assert.AreEqual(results.Length, 538);
        }

        [Test]
        public async Task GetExtrasTest()
        {
            var gr = new Goodreads(new Logger());
            var book = new BookInfo("", "", "") { DataUrl = "https://www.goodreads.com/book/show/13497.A_Feast_for_Crows" };
            await gr.GetExtrasAsync(book);
            Assert.Greater(book.AmazonRating, 0);
            Assert.AreEqual(book.notableClips.Count, 538);
            Assert.GreaterOrEqual(book.Reviews, 1);
        }
    }
}