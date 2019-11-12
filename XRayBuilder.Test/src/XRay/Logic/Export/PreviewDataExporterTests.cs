using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Export;
using XRayBuilder.Core.XRay.Logic.Terms;

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
            _xrayService = new XRayService(_logger, _chaptersService, new AliasesRepository(_logger, new AliasesService(_logger)));
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlPreviewDataTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, _file, null, CancellationToken.None);
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", $"XRAY.{book.Asin}.previewData");
            _previewDataExporter.Export(xray, outpath);
            FileAssert.AreEqual($"testfiles\\XRAY.{book.Asin}.previewData", outpath);
        }
    }
}