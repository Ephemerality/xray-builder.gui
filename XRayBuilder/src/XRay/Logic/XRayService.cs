using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Libraries.Progress;
using XRayBuilderGUI.XRay.Logic.Aliases;
using XRayBuilderGUI.XRay.Logic.Chapters;

namespace XRayBuilderGUI.XRay.Logic
{
    [UsedImplicitly]
    public sealed class XRayService : IXRayService
    {
        private readonly IAliasesService _aliasesService;
        private readonly ILogger _logger;
        private readonly ChaptersService _chaptersService;

        public XRayService(IAliasesService aliasesService, ILogger logger, ChaptersService chaptersService)
        {
            _aliasesService = aliasesService;
            _logger = logger;
            _chaptersService = chaptersService;
        }

        public async Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            int locOffset,
            ISecondarySource dataSource,
            IProgressBar progress,
            CancellationToken token = default)
        {
            var xray = new XRay(dataLocation, db, guid, asin, dataSource, _logger, _chaptersService, locOffset)
            {
                Terms = (await dataSource.GetTermsAsync(dataLocation, progress, token)).ToList()
            };
            if (dataSource.SupportsNotableClips)
            {
                _logger.Log("Downloading notable clips...");
                xray.NotableClips = (await dataSource.GetNotableClipsAsync(dataLocation, null, progress, token))?.ToList();
            }
            if (xray.Terms.Count == 0)
            {
                _logger.Log("Warning: No terms found on " + dataSource.Name + ".");
            }

            return xray;
        }

        public void ExportAndDisplayTerms(XRay xray, string path)
        {
            //Export available terms to a file to make it easier to create aliases or import the modified aliases if they exist
            //Could potentially just attempt to automate the creation of aliases, but in some cases it is very subjective...
            //For example, Shelfari shows the character "Artemis Fowl II", but in the book he is either referred to as "Artemis Fowl", "Artemis", or even "Arty"
            //Other characters have one name on Shelfari but can have completely different names within the book
            var aliasesDownloaded = false;
            // TODO: Review this download process
            //if ((!File.Exists(AliasPath) || Properties.Settings.Default.overwriteAliases) && Properties.Settings.Default.downloadAliases)
            //{
            //    aliasesDownloaded = await AttemptAliasDownload();
            //}

            if (!aliasesDownloaded && (!File.Exists(path) || Properties.Settings.Default.overwriteAliases))
            {
                _aliasesService.SaveCharacters(xray.Terms, path);
                _logger.Log($"Characters exported to {path} for adding aliases.");
            }

            if (xray.SkipShelfari)
                _logger.Log(string.Format("{0} {1} found in file:", xray.Terms.Count, xray.Terms.Count > 1 ? "Terms" : "Term"));
            else
                _logger.Log(string.Format("{0} {1} found on {2}:", xray.Terms.Count, xray.Terms.Count > 1 ? "Terms" : "Term", xray.DataSource.Name));
            var str = new StringBuilder(xray.Terms.Count * 32); // Assume that most names will be less than 32 chars
            var termId = 1;
            foreach (var t in xray.Terms)
            {
                str.Append(t.TermName).Append(", ");
                // todo don't set the IDs here...
                t.Id = termId++;
            }
            _logger.Log(str.ToString());
        }
    }
}