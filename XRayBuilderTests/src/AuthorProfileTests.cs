﻿using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilderGUI;

namespace XRayBuilderTests
{
    [TestFixture]
    public class AuthorProfileTests
    {
        [Test]
        public async Task GenerateAsyncTest()
        {
            var response = await AuthorProfile.GenerateAsync(
                new AuthorProfile.Request
                {
                    Book = new BookInfo("A Game of Thrones", "George R. R. Martin", "B000QCS8TW"),
                    Settings = new AuthorProfile.Settings
                    {
                        AmazonTld = "com",
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
            Assert.AreEqual(response.AmazonTld, "co.uk");
        }
    }
}