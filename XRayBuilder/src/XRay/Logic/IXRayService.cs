using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Progress;

namespace XRayBuilderGUI.XRay.Logic
{
    public interface IXRayService
    {
        void ExportAndDisplayTerms(XRay xray, string path);

        Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            int locOffset,
            ISecondarySource dataSource,
            IProgressBar progress,
            CancellationToken token = default);

        /// <summary>
        /// Expand the <paramref name="xray"/> using data from the <paramref name="rawMlStream"/>.
        /// Adds chapters, character locations, excerpts, and notable clips.
        /// </summary>
        void ExpandFromRawMl(XRay xray, Stream rawMlStream, XRay.SafeShowDelegate safeShow, IProgressBar progress, CancellationToken token, bool ignoreSoftHypen = false, bool shortEx = true);
    }
}