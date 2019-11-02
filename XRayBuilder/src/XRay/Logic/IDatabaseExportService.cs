using System.Threading;
using XRayBuilderGUI.Libraries.Progress;

namespace XRayBuilderGUI.XRay.Logic
{
    public interface IDatabaseExportService
    {
        void Export(XRay xray, string path, IProgressBar progress, CancellationToken cancellationToken = default);
    }
}