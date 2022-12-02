using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Export;
using XRayBuilder.Core.XRay.Logic.Parsing;
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
        private IDirectoryService _directoryService;

        [SetUp]
        public void Setup()
        {
            var config = new XRayBuilderConfig
            {
                UseNewVersion = true
            };
            _logger = new Logger();
            _termsService = new TermsService(config);
            _file = new SecondarySourceFile(_logger, _termsService);
            _previewDataExporter = new PreviewDataExporter();
            _directoryService = new DirectoryService(_logger, null);
            var appConfig = new ApplicationConfig
            {
                Unattended = true
            };
            _chaptersService = new ChaptersService(_logger, config, _directoryService, appConfig);
            _xrayService = new XRayService(_logger, _chaptersService, new AliasesRepository(_logger, new AliasesService(_logger), _directoryService), _directoryService, _termsService, new ParagraphsService(), config);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlPreviewDataTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", $"XRAY.{book.Asin}.previewData");
            _previewDataExporter.Export(xray, outpath);
            FileAssert.AreEqual($"testfiles\\XRAY.{book.Asin}.previewData", outpath);
        }
    }
}