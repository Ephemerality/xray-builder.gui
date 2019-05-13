using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilderGUI;

namespace XRayBuilder.Test
{
    [TestFixture]
    public class AuthorProfileTests
    {
        [TestCase(@"A Game of Thrones", "George R. R. Martin", "B000QCS8TW", "B000APIGH4", "George R. R. Martin")]
        [TestCase(@"Tiamat's Wrath", "James S. A. Corey", "B07BVNVWL6", "B004AQ1W8Y", "James S. A. Corey")]
        public async Task GenerateAsyncTest(string bookTitle, string authorName, string asin, string expectedAuthorAsin, string expectedAuthorName)
        {
            var response = await AuthorProfile.GenerateAsync(
                new AuthorProfile.Request
                {
                    Book = new BookInfo(bookTitle, authorName, asin),
                    Settings = new AuthorProfile.Settings
                    {
                        AmazonTld = "com",
                        SaveBio = false,
                        UseNewVersion = true,
                        EditBiography = false
                    }
                }, new Logger());
            Assert.NotNull(response);
            Assert.AreEqual(expectedAuthorAsin, response.Asin);
            Assert.AreEqual(expectedAuthorName, response.Name);
            Assert.NotNull(response.Image);
            Assert.IsFalse(string.IsNullOrEmpty(response.ImageUrl));
            Assert.IsFalse(string.IsNullOrEmpty(response.Biography));
            Assert.IsNotEmpty(response.OtherBooks);
        }

        [Test]
        public async Task GenerateAsyncTest_Uk()
        {
            var response = await AuthorProfile.GenerateAsync(
                new AuthorProfile.Request
                {
                    Book = new BookInfo("A Game of Thrones", "George R. R. Martin", "B004GJXQ20"),
                    Settings = new AuthorProfile.Settings
                    {
                        AmazonTld = "co.uk",
                        SaveBio = false,
                        UseNewVersion = true,
                        EditBiography = false
                    }
                }, new Logger());
            Assert.AreEqual(response.Asin, "B000APIGH4");
            Assert.AreEqual(response.Name, "George R. R. Martin");
            Assert.NotNull(response.Image);
            Assert.IsFalse(string.IsNullOrEmpty(response.ImageUrl));
            Assert.IsFalse(string.IsNullOrEmpty(response.Biography));
            Assert.IsNotEmpty(response.OtherBooks);
            // TODO: Try to make UK page not require captcha as often
            //Assert.AreEqual(response.AmazonTld, "co.uk");
        }
    }
}