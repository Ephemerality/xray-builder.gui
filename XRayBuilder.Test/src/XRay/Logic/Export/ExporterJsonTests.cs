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
    public sealed class ExporterJsonTests
    {
        private ILogger _logger;
        private SecondarySourceFile _file;
        private IAliasesRepository _aliasesRepository;
        private IXRayExporter _xrayExporter;
        private ChaptersService _chaptersService;
        private IXRayService _xrayService;
        private ITermsService _termsService;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _termsService = new TermsService();
            _file = new SecondarySourceFile(_logger, _termsService);
            _aliasesRepository = new AliasesRepository(_logger);
            _xrayExporter = new XRayExporterJson();
            _chaptersService = new ChaptersService(_logger);
            _xrayService = new XRayService(new AliasesService(_logger), _logger, _chaptersService);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlSaveOldTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.xml, book.db, book.guid, book.asin, 0, _file, null, CancellationToken.None);
            xray.Unattended = true;
            _xrayService.ExportAndDisplayTerms(xray, xray.AliasPath);
            _aliasesRepository.LoadAliasesForXRay(xray);
            _xrayService.ExpandFromRawMl(xray, new FileStream(book.rawml, FileMode.Open), null, null, CancellationToken.None, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.CreatedAt = new DateTime(2019, 11, 2, 13, 19, 18, DateTimeKind.Utc);
            _xrayExporter.Export(xray, outpath, null, CancellationToken.None);
            FileAssert.AreEqual($"testfiles\\XRAY.entities.{book.asin}_old.asc", outpath);
        }
    }
}