using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.XRay.Logic;
using XRayBuilderGUI.XRay.Logic.Aliases;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Logic.Export;
using XRayBuilderGUI.XRay.Logic.Terms;

namespace XRayBuilder.Test.XRay.Logic.Export
{
    [TestFixture]
    public sealed class PreviewDataExporterTests
    {
        private ILogger _logger;
        private SecondarySourceFile _file;
        private ChaptersService _chaptersService;
        private IPreviewDataExporter _previewDataExporter;
        private IXRayService _xrayService;
        private ITermsService _termsService;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _termsService = new TermsService();
            _file = new SecondarySourceFile(_logger, _termsService);
            _previewDataExporter = new PreviewDataExporter();
            _chaptersService = new ChaptersService(_logger);
            _xrayService = new XRayService(new AliasesService(_logger), _logger, _chaptersService);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlPreviewDataTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.xml, book.db, book.guid, book.asin, 0, _file, null, CancellationToken.None);
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", $"XRAY.{book.asin}.previewData");
            _previewDataExporter.Export(xray, outpath);
            FileAssert.AreEqual($"testfiles\\XRAY.{book.asin}.previewData", outpath);
        }
    }
}