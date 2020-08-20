using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.XRay.Logic
{
    public interface IXRayService
    {
        void ExportAndDisplayTerms(XRay xray, string path, bool overwriteAliases, bool splitAliases);

        Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            string tld,
            bool includeTopics,
            ISecondarySource dataSource,
            IProgressBar progress,
            CancellationToken token = default);

        /// <summary>
        /// Expand the <paramref name="xray"/> using data from the <paramref name="rawMlStream"/>.
        /// Adds chapters, character locations, excerpts, and notable clips.
        /// </summary>
        void ExpandFromRawMl(
            XRay xray,
            IMetadata metadata,
            Stream rawMlStream,
            bool useNewVersion,
            bool skipNoLikes,
            int minClipLen,
            bool overwriteChapters,
            [CanBeNull] Func<bool> editChaptersCallback,
            IProgressBar progress,
            CancellationToken token,
            bool ignoreSoftHypen = false,
            bool shortEx = true);
    }
}