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

namespace XRayBuilder.Test.XRay.Logic.Export
{
    public sealed class ExporterSqliteTests
    {
        private ILogger _logger;
        private IXRayExporter _xrayExporter;
        private SecondarySourceFile _file;
        private IAliasesRepository _aliasesRepository;
        private ChaptersService _chaptersService;
        private IXRayService _xrayService;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _file = new SecondarySourceFile(_logger);
            _xrayExporter = new XRayExporterSqlite(_logger);
            _aliasesRepository = new AliasesRepository(_logger);
            _chaptersService = new ChaptersService(_logger);
            _xrayService = new XRayService(new AliasesService(_logger), _logger, _chaptersService);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLSaveNewTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.xml, book.db, book.guid, book.asin, 0, _file, null, CancellationToken.None);
            _xrayService.ExportAndDisplayTerms(xray, xray.AliasPath);
            _aliasesRepository.LoadAliasesForXRay(xray);
            xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, CancellationToken.None, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            _xrayExporter.Export(xray, outpath, null, CancellationToken.None);
        }
    }
}