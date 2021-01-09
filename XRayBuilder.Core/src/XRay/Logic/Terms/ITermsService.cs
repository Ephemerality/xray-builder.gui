using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Logic.Parsing;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic.Terms
{
    public interface ITermsService
    {
        /// <summary>
        /// Extract terms from the given db.
        /// </summary>
        /// <param name="xrayDb">Connection to any db containing the proper dataset.</param>
        /// <param name="singleUse">If set, will close the connection when complete.</param>
        IEnumerable<Term> ExtractTermsNew(DbConnection xrayDb, bool singleUse);

        /// <summary>
        /// Extract terms from the old JSON X-Ray format
        /// </summary>
        IEnumerable<Term> ExtractTermsOld(string path);

        /// <summary>
        /// Downloads terms from the <paramref name="dataSource"/> and saves them to <paramref name="outFile"/>
        /// </summary>
        Task DownloadAndSaveAsync(ISecondarySource dataSource, string dataUrl, string outFile, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken token = default);

        /// <summary>
        /// Read terms from a <param-ref name="txtFile"/>
        /// </summary>
        IEnumerable<Term> ReadTermsFromTxt(string txtFile);

        [NotNull]
        HashSet<Occurrence> FindOccurrences([NotNull] IMetadata metadata, [NotNull] Term term, [NotNull] Paragraph paragraph);
    }
}