using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using NSubstitute;
using NUnit.Framework;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.IO.Extensions;
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
    public sealed class ExporterSqliteTests
    {
        private ILogger _logger;
        private IXRayExporter _xrayExporter;
        private SecondarySourceFile _file;
        private IAliasesRepository _aliasesRepository;
        private ChaptersService _chaptersService;
        private IXRayService _xrayService;
        private ITermsService _termsService;
        private IDirectoryService _directoryService;

        [OneTimeSetUp]
        public void Setup()
        {
            var config = new XRayBuilderConfig
            {
                UseNewVersion = true
            };
            _logger = new Logger();
            _termsService = new TermsService(config);
            _file = new SecondarySourceFile(_logger, _termsService);
            _xrayExporter = new XRayExporterSqlite(_logger);
            _directoryService = new DirectoryService(_logger, new XRayBuilderConfig());
            _aliasesRepository = new AliasesRepository(_logger, new AliasesService(_logger), _directoryService);
            var appConfig = new ApplicationConfig
            {
                Unattended = true
            };
            _chaptersService = new ChaptersService(_logger, config, _directoryService, appConfig);
            _xrayService = new XRayService(_logger, _chaptersService, _aliasesRepository, _directoryService, _termsService, new ParagraphsService(), config);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLSaveNewTest(Book book)
        {
            var xray = await _xrayService.CreateXRayAsync(book.Xml, book.Db, book.Guid, book.Asin, book.Author, book.Title, "com", true, _file, null, CancellationToken.None);
            _xrayService.ExportAndDisplayTerms(xray, _file, true, false);
            await using var fs = new FileStream(book.Rawml, FileMode.Open);
            var fakeMetadata = Substitute.For<IMetadata>();
            fakeMetadata.IsAzw3.Returns(false);
            fakeMetadata.GetRawMlStream().Returns(new MemoryStream(fs.ReadToEnd()));
            fs.Seek(0, SeekOrigin.Begin);
            _aliasesRepository.LoadAliasesForXRay(xray);
            _xrayService.ExpandFromRawMl(xray, fakeMetadata, fs, null, null, null, CancellationToken.None);
            var filename = _directoryService.GetArtifactFilename(ArtifactType.XRay, book.Asin, book.Db, book.Guid);
            var outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            _xrayExporter.Export(xray, outpath, null, CancellationToken.None);
        }
    }
}