using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.IO.Extensions;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Parsing;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Test.XRay;

namespace XRayBuilder.Test
{
    [TestFixture]
    public class XRayTests
    {
        private ILogger _logger;
        private SecondarySourceFile _file;
        private ChaptersService _chaptersService;
        private IXRayService _xrayService;
        private ITermsService _termsService;
        private IDirectoryService _directoryService;
        private XRayBuilderConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = new XRayBuilderConfig
            {
                UseNewVersion = true,
                SkipNoLikes = true
            };
            _logger = new Logger();
            _termsService = new TermsService(_config);
            _file = new SecondarySourceFile(_logger, _termsService);
            _chaptersService = new ChaptersService(_logger, _config);
            _directoryService = new DirectoryService(_logger, null);
            _xrayService = new XRayService(_logger, _chaptersService, new AliasesRepository(_logger, new AliasesService(_logger), _directoryService), _directoryService, _termsService, new ParagraphsService(), _config);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            Assert.NotNull(xray);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLAliasTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            _xrayService.ExportAndDisplayTerms(xray, _file, true, false);
            FileAssert.AreEqual($"ext\\{book.Asin}.aliases", $"testfiles\\{book.Asin}.aliases");
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLExpandRawMLNewVersionTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            xray.Unattended = true;
            // todo refactor this substitute
            using var fs = new FileStream(book.Rawml, FileMode.Open);
            var fakeMetadata = Substitute.For<IMetadata>();
            fakeMetadata.IsAzw3.Returns(false);
            fakeMetadata.GetRawMlStream().Returns(new MemoryStream(fs.ReadToEnd()));
            fs.Seek(0, SeekOrigin.Begin);
            _config.UseNewVersion = true;
            _xrayService.ExpandFromRawMl(xray, fakeMetadata, fs,null, null, null, CancellationToken.None);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLExpandRawMLOldVersionTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            xray.Unattended = true;
            using var fs = new FileStream(book.Rawml, FileMode.Open);
            var fakeMetadata = Substitute.For<IMetadata>();
            fakeMetadata.IsAzw3.Returns(false);
            fakeMetadata.GetRawMlStream().Returns(new MemoryStream(fs.ReadToEnd()));
            fs.Seek(0, SeekOrigin.Begin);
            _config.UseNewVersion = false;
            _xrayService.ExpandFromRawMl(xray, fakeMetadata, fs, null, null, null, CancellationToken.None);
            FileAssert.AreEqual($"ext\\{book.Asin}.chapters", $"testfiles\\{book.Asin}.chapters");
        }
    }

    [TestFixture]
    public class DeserializeTests
    {
        // private static List<BookInfo> books = new List<BookInfo>
        // {
        //     new BookInfo("A Storm of Swords", "George R. R. Martin",
        //         "B000FBFN1U", "171927873", "A_Storm_of_Swords", Path.Combine(Environment.CurrentDirectory, "out"), "https://www.goodreads.com/book/show/62291",
        //         @"testfiles\A Storm of Swords - George R. R. Martin.rawml")
        // };

        // TODO: Compare the actual contents (objects) rather than the string itself due to the order of books changing
        //[Test(), TestCaseSource(nameof(books))]
        //public async Task AuthorProfileBuildTest(BookInfo book)
        //{
        //    AuthorProfile ap = new AuthorProfile(book, new AuthorProfile.Settings
        //    {
        //        AmazonTld = "com",
        //        Android = false,
        //        OutDir = Path.Combine(Environment.CurrentDirectory, "out"),
        //        SaveBio = false,
        //        UseNewVersion = true,
        //        UseSubDirectories = false
        //    });
        //    if (!await ap.Generate()) return;

        //    using (StreamReader streamReader = new StreamReader(@"out\AuthorProfile.profile.B000FBFN1U.asc", Encoding.UTF8))
        //    using (StreamReader streamReader2 = new StreamReader(@"testfiles\AuthorProfile.profile.B000FBFN1U.asc", Encoding.UTF8))
        //        Assert.AreEqual(streamReader.ReadToEnd(), streamReader2.ReadToEnd());
        //}

        [Test]
        public void AuthorProfileDeserializeTest()
        {
            using (StreamReader streamReader = new StreamReader(@"testfiles\AuthorProfile.profile.B000FBFN1U.asc", Encoding.UTF8))
            {
                var ap = JsonConvert.DeserializeObject<AuthorProfile>(streamReader.ReadToEnd());
                var outtxt = JsonConvert.SerializeObject(ap);
                File.WriteAllText(@"sampleap.txt", outtxt);
            }
        }

        [Test]
        public void StartActionsDeserializeTest()
        {
            using (StreamReader streamReader = new StreamReader(@"testfiles\StartActions.data.B000FBFN1U.asc", Encoding.UTF8))
            {
                var sa = JsonConvert.DeserializeObject<StartActions>(streamReader.ReadToEnd());
                var outtxt = Functions.ExpandUnicode(JsonConvert.SerializeObject(sa));
                File.WriteAllText(@"samplesa.txt", outtxt, Encoding.UTF8);
            }
        }

        [Test]
        public void EndActionsDeserializeTest()
        {
            using StreamReader streamReader = new StreamReader(@"testfiles\EndActions.data.B000FBFN1U.asc", Encoding.UTF8);
            var ea = JsonConvert.DeserializeObject<EndActions>(streamReader.ReadToEnd());
            var outtxt = Functions.ExpandUnicode(JsonConvert.SerializeObject(ea));
            File.WriteAllText(@"sampleea.txt", outtxt, Encoding.UTF8);
        }
    }

    public class Book
    {
        public string Rawml { get; set; }
        public string Xml { get; set; }
        public string Db { get; set; }
        public string Guid { get; set; }
        public string Asin { get; set; }
        public string Bookpath { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
    }
}