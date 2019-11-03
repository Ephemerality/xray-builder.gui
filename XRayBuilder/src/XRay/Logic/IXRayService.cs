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
    }
}