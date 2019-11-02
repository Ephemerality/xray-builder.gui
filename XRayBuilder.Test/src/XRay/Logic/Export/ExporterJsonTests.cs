using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Http;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.XRay.Logic;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Logic.Export;

namespace XRayBuilder.Test.XRay.Logic.Export
{
    public sealed class ExporterJsonTests
    {
        private ILogger _logger;
        private Goodreads _goodreads;
        private IAliasesService _aliasesService;
        private IHttpClient _httpClient;
        private IAmazonClient _amazonClient;
        private IAmazonInfoParser _amazonInfoParser;
        private IXRayExporter _xrayExporter;
        private ChaptersService _chaptersService;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _goodreads = new Goodreads(_logger, _httpClient, _amazonClient);
            _aliasesService = new AliasesService(_logger);
            _xrayExporter = new XRayExporterJson();
            _chaptersService = new ChaptersService(_logger);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlSaveOldTest(Book book)
        {
            var xray = TestData.CreateXRayFromXML(book.xml, book.db, book.guid, book.asin, _goodreads, _logger, _chaptersService);
            await xray.CreateXray(null, CancellationToken.None);
            xray.ExportAndDisplayTerms();
            _aliasesService.LoadAliasesForXRay(xray);
            xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, CancellationToken.None, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.CreatedAt = new DateTime(2019, 11, 2, 13, 19, 18, DateTimeKind.Utc);
            _xrayExporter.Export(xray, outpath, null, CancellationToken.None);
            FileAssert.AreEqual($"testfiles\\XRAY.entities.{book.asin}_old.asc", outpath);
        }
    }
}