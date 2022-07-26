using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Prompt;

namespace XRayBuilder.Core.XRay.Logic
{
    public interface IXRayService
    {
        void ExportAndDisplayTerms(XRay xray, ISecondarySource dataSource, bool overwriteAliases, bool splitAliases);

        Task<XRay> CreateXRayAsync(string dataLocation, IMetadata metadata, string tld, bool includeTopics, ISecondarySource dataSource, [CanBeNull] IProgressBar progress, CancellationToken cancellationToken);

        Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            string author,
            string title,
            string tld,
            bool includeTopics,
            ISecondarySource dataSource,
            [CanBeNull] IProgressBar progress,
            CancellationToken token = default);

        /// <summary>
        /// Expand the <paramref name="xray"/> using data from the <paramref name="rawMlStream"/>.
        /// Adds chapters, character locations, excerpts, and notable clips.
        /// </summary>
        void ExpandFromRawMl(
            XRay xray,
            IMetadata metadata,
            Stream rawMlStream,
            [CanBeNull] YesNoPrompt yesNoPrompt,
            [CanBeNull] EditCallback editCallback,
            [CanBeNull] IProgressBar progress,
            CancellationToken token);
    }
}