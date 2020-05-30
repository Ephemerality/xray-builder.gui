using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Unpack.Mobi;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Export;
using XRayBuilder.Core.XRay.Logic.Terms;

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
            _aliasesRepository = new AliasesRepository(_logger, new AliasesService(_logger));
            _xrayExporter = new XRayExporterJson();
            _chaptersService = new ChaptersService(_logger);
            _xrayService = new XRayService(_logger, _chaptersService, _aliasesRepository);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlSaveOldTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, "com", true, _file, null, CancellationToken.None);
            xray.Unattended = true;
            _xrayService.ExportAndDisplayTerms(xray, xray.AliasPath, true, false);
            using var fs = new FileStream(book.Bookpath, FileMode.Open, FileAccess.Read);
            var metadata = new Metadata(fs);
            _aliasesRepository.LoadAliasesForXRay(xray);
            using var bookFs = new FileStream(book.Rawml, FileMode.Open);
            _xrayService.ExpandFromRawMl(xray, metadata, bookFs, false, false, true, 0, true, null, null, CancellationToken.None, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.CreatedAt = new DateTime(2019, 11, 2, 13, 19, 18, DateTimeKind.Utc);
            _xrayExporter.Export(xray, outpath, null, CancellationToken.None);
            FileAssert.AreEqual($"testfiles\\XRAY.entities.{book.Asin}_old.asc", outpath);
        }
    }
}