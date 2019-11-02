using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Extras.Artifacts;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Http;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Model;
using XRayBuilderGUI.XRay.Logic;
using EndActions = XRayBuilderGUI.Extras.EndActions.EndActions;

namespace XRayBuilder.Test
{
    [TestFixture]
    public class XRayTests
    {
        private ILogger _logger;
        private IHttpClient _httpClient;
        private IAmazonClient _amazonClient;
        private Goodreads _goodreads;
        private IAmazonInfoParser _amazonInfoParser;
        private IAliasesService _aliasesService;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _goodreads = new Goodreads(_logger, _httpClient, _amazonClient);
            _aliasesService = new AliasesService(_logger);
        }

        private static readonly CancellationTokenSource tokens = new CancellationTokenSource();

        private static List<Book> books = new List<Book>
        {
            new Book(@"testfiles\A Storm of Swords - George R. R. Martin.rawml", @"testfiles\A Storm of Swords - George R. R. Martin.xml", "A_Storm_of_Swords", "171927873", "B000FBFN1U"),
            new Book(@"testfiles\Tick Tock - James Patterson.rawml", @"testfiles\Tick Tock - James Patterson.xml", "Tick_Tock", "2219522925", "B0047Y16MG")
        };

        private XRayBuilderGUI.XRay.XRay CreateXRayFromXML(string path, string db, string guid, string asin)
        {
            return new XRayBuilderGUI.XRay.XRay(path, db, guid, asin, _goodreads, _logger, _aliasesService, 0, "") { unattended = true };
        }

        [Test, TestCaseSource(nameof(books))]
        public async Task XRayXMLTest(Book book)
        {
            XRayBuilderGUI.XRay.XRay xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            Assert.NotNull(xray);
            Assert.AreEqual(await xray.CreateXray(null, tokens.Token), 0);
        }

        [Test, TestCaseSource(nameof(books))]
        public async Task XRayXMLAliasTest(Book book)
        {
            var xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            xray.ExportAndDisplayTerms();
            FileAssert.AreEqual($"ext\\{book.asin}.aliases", $"testfiles\\{book.asin}.aliases");
        }

        [Test, TestCaseSource(nameof(books))]
        public async Task XRayXMLExpandRawMLTest(Book book)
        {
            var xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            Assert.AreEqual(xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, tokens.Token, false, false), 0);
            FileAssert.AreEqual($"ext\\{book.asin}.chapters", $"testfiles\\{book.asin}.chapters");
        }

        [Test, TestCaseSource(nameof(books))]
        public async Task XRayXMLSaveNewTest(Book book)
        {
            var xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            xray.ExportAndDisplayTerms();
            xray.LoadAliases();
            xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, tokens.Token, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.SaveToFileNew(outpath, null, tokens.Token);
        }

        [Test, TestCaseSource(nameof(books))]
        public async Task XRayXmlSaveOldTest(Book book)
        {
            var xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            xray.ExportAndDisplayTerms();
            xray.LoadAliases();
            xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, tokens.Token, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.CreatedAt = new DateTime(2019, 11, 2, 13, 19, 18, DateTimeKind.Utc);
            xray.SaveToFileOld(outpath);
            FileAssert.AreEqual($"testfiles\\XRAY.entities.{book.asin}_old.asc", outpath);
        }

        [Test, TestCaseSource(nameof(books))]
        public async Task XRayXmlPreviewDataTest(Book book)
        {
            var xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", $"XRAY.{book.asin}.previewData");
            xray.SavePreviewToFile(outpath);
            FileAssert.AreEqual($"testfiles\\XRAY.{book.asin}.previewData", outpath);
        }
    }

    [TestFixture]
    public class DeserializeTests
    {
        private static List<BookInfo> books = new List<BookInfo>
        {
            new BookInfo("A Storm of Swords", "George R. R. Martin",
                "B000FBFN1U", "171927873", "A_Storm_of_Swords", Path.Combine(Environment.CurrentDirectory, "out"), "https://www.goodreads.com/book/show/62291",
                @"testfiles\A Storm of Swords - George R. R. Martin.rawml")
        };

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
        public string rawml;
        public string xml;
        public string db;
        public string guid;
        public string asin;

        public Book(string rawml, string xml, string db, string guid, string asin)
        {
            this.rawml = rawml;
            this.xml = xml;
            this.db = db;
            this.guid = guid;
            this.asin = asin;
        }
    }
}