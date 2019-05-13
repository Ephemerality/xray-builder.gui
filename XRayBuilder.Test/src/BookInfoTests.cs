using NUnit.Framework;
using XRayBuilderGUI;
using System.Threading.Tasks;

namespace XRayBuilder.Test
{
    [TestFixture()]
    public class BookInfoTests
    {
        [Test()]
        public async Task GetAmazonInfoTest()
        {
            BookInfo bk = new BookInfo("A Game of Thrones", "George R. R. Martin", "B000QCS8TW");
            await bk.GetAmazonInfo("https://www.amazon.ca/Game-Thrones-Song-Fire-Book-ebook/dp/B000QCS8TW/");
            Assert.Greater(bk.Reviews, 0);
            Assert.IsNotEmpty(bk.ImageUrl);
            Assert.IsNotEmpty(bk.Description);
        }

        [Test()]
        public async Task CoverImageTest()
        {
            BookInfo bk = new BookInfo("A Game of Thrones", "George R. R. Martin", "B000QCS8TW");
            await bk.GetAmazonInfo("https://www.amazon.ca/Game-Thrones-Song-Fire-Book-ebook/dp/B000QCS8TW/");
            Assert.IsNotNull(bk.CoverImage());
        }
    }
}