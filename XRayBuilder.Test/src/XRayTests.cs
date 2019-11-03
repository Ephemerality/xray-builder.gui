using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using XRayBuilder.Test.XRay;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Extras.Artifacts;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Model;
using XRayBuilderGUI.XRay.Logic;
using XRayBuilderGUI.XRay.Logic.Aliases;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Logic.Terms;
using EndActions = XRayBuilderGUI.Extras.EndActions.EndActions;

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

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _termsService = new TermsService();
            _file = new SecondarySourceFile(_logger, _termsService);
            _chaptersService = new ChaptersService(_logger);
            _xrayService = new XRayService(_logger, _chaptersService, new AliasesRepository(_logger, new AliasesService(_logger)));
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.xml, book.db, book.guid, book.asin, 0, _file, null, CancellationToken.None);
            Assert.NotNull(xray);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLAliasTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.xml, book.db, book.guid, book.asin, 0, _file, null, CancellationToken.None);
            _xrayService.ExportAndDisplayTerms(xray, xray.AliasPath);
            FileAssert.AreEqual($"ext\\{book.asin}.aliases", $"testfiles\\{book.asin}.aliases");
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLExpandRawMLTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.xml, book.db, book.guid, book.asin, 0, _file, null, CancellationToken.None);
            xray.Unattended = true;
            _xrayService.ExpandFromRawMl(xray, new FileStream(book.rawml, FileMode.Open), null, null, CancellationToken.None, false, false);
            FileAssert.AreEqual($"ext\\{book.asin}.chapters", $"testfiles\\{book.asin}.chapters");
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