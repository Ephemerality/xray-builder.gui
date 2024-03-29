using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack.Mobi;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
    public sealed class ExporterJsonTests
    {
        private ILogger _logger;
        private SecondarySourceFile _file;
        private IAliasesRepository _aliasesRepository;
        private IXRayExporter _xrayExporter;
        private ChaptersService _chaptersService;
        private IXRayService _xrayService;
        private ITermsService _termsService;
        private IDirectoryService _directoryService;

        [OneTimeSetUp]
        public void Setup()
        {
            var config = new XRayBuilderConfig
            {
                UseNewVersion = false
            };
            _logger = new Logger();
            _termsService = new TermsService(config);
            _file = new SecondarySourceFile(_logger, _termsService);
            _directoryService = new DirectoryService(_logger, new XRayBuilderConfig());
            _aliasesRepository = new AliasesRepository(_logger, new AliasesService(_logger), _directoryService);
            _xrayExporter = new XRayExporterJson();
            var appConfig = new ApplicationConfig
            {
                Unattended = true
            };
            _chaptersService = new ChaptersService(_logger, config, _directoryService, appConfig);
            _xrayService = new XRayService(_logger, _chaptersService, _aliasesRepository, _directoryService, _termsService, new ParagraphsService(), config);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXmlSaveOldTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            _xrayService.ExportAndDisplayTerms(xray, _file, true, false);
            await using var fs = new FileStream(book.Bookpath, FileMode.Open, FileAccess.Read);
            var metadata = new MobiMetadata(fs);
            _aliasesRepository.LoadAliasesForXRay(xray);
            await using var bookFs = new FileStream(book.Rawml, FileMode.Open);
            _xrayService.ExpandFromRawMl(xray, metadata, bookFs, null, null, null, CancellationToken.None);
            var filename = _directoryService.GetArtifactFilename(ArtifactType.XRay, book.Asin, book.Db, book.Guid);
            var outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.CreatedAt = new DateTime(2019, 11, 2, 13, 19, 18, DateTimeKind.Utc);
            _xrayExporter.Export(xray, outpath, null, CancellationToken.None);
            FileAssert.AreEqual($"testfiles\\XRAY.entities.{book.Asin}_old.asc", outpath);
        }
    }
}