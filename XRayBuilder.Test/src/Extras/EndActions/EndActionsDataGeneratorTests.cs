using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Extras.EndActions;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Logic.PageCount;
using XRayBuilder.Core.Logic.ReadingTime;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Logic.Parsing;

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
        private IReadingTimeService _readingTimeService;
        private IPageCountService _pageCountService;

        [SetUp]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _authorProfileGenerator = new AuthorProfileGenerator(_httpClient, _logger, _amazonClient);
            _secondarySourceGoodreads = new SecondarySourceGoodreads(_logger, _httpClient, _amazonClient, _readingTimeService);
            _endActionsArtifactService = new EndActionsArtifactService(_logger);
            _readingTimeService = new ReadingTimeService();
            _pageCountService = new PageCountService(new ParagraphsService());
        }

        // [Test]
        public async Task Test()
        {
            var metadata = MetadataReader.Load(@"testfiles\A Storm of Swords - George R. R. Martin.mobi");
            var book = new BookInfo(metadata, "https://www.goodreads.com/book/show/62291.A_Storm_of_Swords");
            var authorProfileResponse = await _authorProfileGenerator.GenerateAsync(new AuthorProfileGenerator.Request
            {
                Book = book,
                Settings = new AuthorProfileGenerator.Settings
                {
                    AmazonTld = "com",
                    SaveBio = false,
                    UseNewVersion = true,
                    EditBiography = false
                }
            }, _ => false, null, CancellationToken.None);

            var endActionsDataGenerator = new EndActionsDataGenerator(_logger, _httpClient, _amazonClient, _amazonInfoParser, null, _readingTimeService, _pageCountService);

            var settings = new EndActionsDataGenerator.Settings
            {
                AmazonTld = "com",
                EstimatePageCount = false,
                PromptAsin = false,
                SaveHtml = false,
                UseNewVersion = true
            };

            var endActionsResponse = await endActionsDataGenerator.GenerateNewFormatData(book, settings, _secondarySourceGoodreads, authorProfileResponse, null, metadata, null, CancellationToken.None);
            ClassicAssert.NotNull(endActionsResponse);

            var content = _endActionsArtifactService.GenerateNew(new EndActionsArtifactService.Request(
                bookAsin: endActionsResponse.Book.Asin,
                bookImageUrl: endActionsResponse.Book.ImageUrl,
                bookDatabaseName: endActionsResponse.Book.Databasename,
                bookGuid: endActionsResponse.Book.Guid ?? "",
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

            var expected = await File.ReadAllTextAsync(@"testfiles\EndActions.data.B000FBFN1U.asc", Encoding.UTF8);
            await File.WriteAllTextAsync(@"testfiles\sampleendactions.txt", content);
            ClassicAssert.AreEqual(expected, content);
        }
    }
}