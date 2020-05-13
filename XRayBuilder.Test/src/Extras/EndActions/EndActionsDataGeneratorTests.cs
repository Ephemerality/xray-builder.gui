using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Extras.EndActions;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Test.Extras.EndActions
{
    /// <summary>
    /// Integration tests for the EndActionsDataGenerator
    /// </summary>
    // [TestFixture] todo finish this
    public class EndActionsDataGeneratorTests
    {
        private AuthorProfileGenerator _authorProfileGenerator;
        private SecondarySourceGoodreads _secondarySourceGoodreads;
        private ILogger _logger;
        private IHttpClient _httpClient;
        private IAmazonInfoParser _amazonInfoParser;
        private IAmazonClient _amazonClient;
        private IEndActionsArtifactService _endActionsArtifactService;

        [SetUp]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _authorProfileGenerator = new AuthorProfileGenerator(_httpClient, _logger, _amazonClient);
            _secondarySourceGoodreads = new SecondarySourceGoodreads(_logger, _httpClient, _amazonClient);
            _endActionsArtifactService = new EndActionsArtifactService(_logger);
        }

        // [Test]
        public async Task Test()
        {
            var metadata = MetadataLoader.Load(@"testfiles\A Storm of Swords - George R. R. Martin.mobi");
            var book = new BookInfo(metadata, "https://www.goodreads.com/book/show/62291.A_Storm_of_Swords");
            var authorProfileResponse = await _authorProfileGenerator.GenerateAsync(new AuthorProfileGenerator.Request
            {
                Book = book,
                Settings = new AuthorProfileGenerator.Settings
                {
                    AmazonTld = "com",
                    SaveBio = false,
                    UseNewVersion = true,
                    EditBiography = false,
                    SaveHtml = false
                }
            }, CancellationToken.None);

            var endActionsDataGenerator = new EndActionsDataGenerator(book, _secondarySourceGoodreads, new EndActionsDataGenerator.Settings
            {
                AmazonTld = "com",
                EstimatePageCount = false,
                PromptAsin = false,
                SaveHtml = false,
                UseNewVersion = true
            }, _logger, _httpClient, _amazonClient, _amazonInfoParser, null);

            var endActionsResponse = await endActionsDataGenerator.GenerateNewFormatData(authorProfileResponse, null, metadata, null, CancellationToken.None);
            Assert.NotNull(endActionsResponse);

            var content = _endActionsArtifactService.GenerateNew(new EndActionsArtifactService.Request(
                bookAsin: endActionsResponse.Book.Asin,
                bookImageUrl: endActionsResponse.Book.ImageUrl,
                bookDatabaseName: endActionsResponse.Book.Databasename,
                bookGuid: endActionsResponse.Book.Guid,
                bookErl: metadata.RawMlSize,
                bookAmazonRating: endActionsResponse.Book.AmazonRating,
                bookSeriesInfo: endActionsResponse.Book.Series,
                author: authorProfileResponse.Name,
                authorAsin: authorProfileResponse.Asin,
                authorImageUrl: authorProfileResponse.ImageUrl,
                authorBiography: authorProfileResponse.Biography,
                authorOtherBooks: authorProfileResponse.OtherBooks,
                userPenName: "Anonymous",
                userRealName: "Anonymous",
                customerAlsoBought: endActionsResponse.CustomerAlsoBought));

            var expected = File.ReadAllText(@"testfiles\EndActions.data.B000FBFN1U.asc", Encoding.UTF8);
            File.WriteAllText(@"testfiles\sampleendactions.txt", content);
            Assert.AreEqual(expected, content);
        }
    }
}