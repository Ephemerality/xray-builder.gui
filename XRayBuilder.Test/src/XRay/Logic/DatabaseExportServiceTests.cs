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

namespace XRayBuilder.Test.XRay.Logic
{
    public class DatabaseExportServiceTests
    {
        private ILogger _logger;
        private IDatabaseExportService _databaseExportService;
        private Goodreads _goodreads;
        private IAliasesService _aliasesService;
        private IHttpClient _httpClient;
        private IAmazonClient _amazonClient;
        private IAmazonInfoParser _amazonInfoParser;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _httpClient = new HttpClient(_logger);
            _amazonInfoParser = new AmazonInfoParser(_logger, _httpClient);
            _amazonClient = new AmazonClient(_httpClient, _amazonInfoParser, _logger);
            _databaseExportService = new DatabaseExportService(_logger);
            _goodreads = new Goodreads(_logger, _httpClient, _amazonClient);
            _aliasesService = new AliasesService(_logger);
        }

        private XRayBuilderGUI.XRay.XRay CreateXRayFromXML(string path, string db, string guid, string asin)
            => new XRayBuilderGUI.XRay.XRay(path, db, guid, asin, _goodreads, _logger, _aliasesService, 0, "") { Unattended = true };

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public async Task XRayXMLSaveNewTest(Book book)
        {
            var xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, CancellationToken.None);
            xray.ExportAndDisplayTerms();
            xray.LoadAliases();
            xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, CancellationToken.None, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            _databaseExportService.Export(xray, outpath, null, CancellationToken.None);
        }
    }
}