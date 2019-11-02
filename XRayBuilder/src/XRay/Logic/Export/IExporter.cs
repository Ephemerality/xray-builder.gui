using System.Threading;
using XRayBuilderGUI.Libraries.Progress;

namespace XRayBuilderGUI.XRay.Logic.Export
{
    public interface IExporter
    {
        void Export(XRay xray, string path, IProgressBar progress, CancellationToken cancellationToken = default);
    }
}