using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Http;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Logic.Export;

namespace XRayBuilder.Test.XRay.Logic.Export
{
    [TestFixture]
    public sealed class PreviewDataExporterTests
    {
        private ILogger _logger;
        private Goodreads _goodreads;
        private ChaptersService _chaptersService;
        private IHttpClient _httpClient;
        private IAmazonClient _amazonClient;
        private IAmazonInfoParser _amazonInfoParser;
        private IPreviewDataExporter _previewDataExporter;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _goodreads = new Goodreads(_logger, _httpClient, _amazonClient);
            _previewDataExporter = new PreviewDataExporter();
            _chaptersService = new ChaptersService(_logger);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlPreviewDataTest(Book book)
        {
            var xray = TestData.CreateXRayFromXML(book.xml, book.db, book.guid, book.asin, _goodreads, _logger, _chaptersService);
            await xray.CreateXray(null, CancellationToken.None);
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", $"XRAY.{book.asin}.previewData");
            _previewDataExporter.Export(xray, outpath);
            FileAssert.AreEqual($"testfiles\\XRAY.{book.asin}.previewData", outpath);
        }
    }
}