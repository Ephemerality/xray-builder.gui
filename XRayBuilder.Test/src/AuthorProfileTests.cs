﻿using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Test
{
    [TestFixture]
    public class AuthorProfileTests
    {
        private IAuthorProfileGenerator _authorProfileGenerator;
        private IHttpClient _httpClient;
        private ILogger _logger;
        private IAmazonClient _amazonClient;
        private IAmazonInfoParser _amazonInfoParser;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _authorProfileGenerator = new AuthorProfileGenerator(_httpClient, _logger, _amazonClient);
        }

        [TestCase(@"A Game of Thrones", "George R. R. Martin", "B000QCS8TW", "B000APIGH4", "George R. R. Martin")]
        [TestCase(@"Tiamat's Wrath", "James S. A. Corey", "B07BVNVWL6", "B004AQ1W8Y", "James S. A. Corey")]
        public async Task GenerateAsyncTest(string bookTitle, string authorName, string asin, string expectedAuthorAsin, string expectedAuthorName)
        {
            var response = await _authorProfileGenerator.GenerateAsync(
                new AuthorProfileGenerator.Request
                {
                    Book = new BookInfo(bookTitle, authorName, asin),
                    Settings = new AuthorProfileGenerator.Settings
                    {
                        AmazonTld = "com",
                        SaveBio = false,
                        UseNewVersion = true,
                        EditBiography = false
                    }
                }, _ => false);
            ClassicAssert.NotNull(response);
            ClassicAssert.AreEqual(expectedAuthorAsin, response.Asin);
            ClassicAssert.AreEqual(expectedAuthorName, response.Name);
            ClassicAssert.NotNull(response.Image);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(response.ImageUrl));
            ClassicAssert.IsFalse(string.IsNullOrEmpty(response.Biography));
            ClassicAssert.IsNotEmpty(response.OtherBooks);
        }

        [Test]
        public async Task GenerateAsyncTest_Uk()
        {
            var response = await _authorProfileGenerator.GenerateAsync(
                new AuthorProfileGenerator.Request
                {
                    Book = new BookInfo("A Game of Thrones", "George R. R. Martin", "B004GJXQ20"),
                    Settings = new AuthorProfileGenerator.Settings
                    {
                        AmazonTld = "co.uk",
                        SaveBio = false,
                        UseNewVersion = true,
                        EditBiography = false
                    }
                }, _ => false);
            ClassicAssert.NotNull(response);
            ClassicAssert.AreEqual(response.Asin, "B000APIGH4");
            ClassicAssert.AreEqual(response.Name, "George R. R. Martin");
            ClassicAssert.NotNull(response.Image);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(response.ImageUrl));
            ClassicAssert.IsFalse(string.IsNullOrEmpty(response.Biography));
            ClassicAssert.IsNotEmpty(response.OtherBooks);
            // TODO: Try to make UK page not require captcha as often
            //ClassicAssert.AreEqual(response.AmazonTld, "co.uk");
        }
    }
}