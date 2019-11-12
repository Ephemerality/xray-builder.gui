using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            bool enableEdit,
            bool useNewVersion,
            bool skipNoLikes,
            int minClipLen,
            bool overwriteChapters,
            SafeShowDelegate safeShow,
            IProgressBar progress,
            CancellationToken token,
            bool ignoreSoftHypen = false,
            bool shortEx = true);
    }

    // TODO: Make this not rely on Windows.Forms
    public delegate DialogResult SafeShowDelegate(string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def);
}