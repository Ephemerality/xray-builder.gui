using System.Threading;
using XRayBuilder.Core.Libraries.Progress;

namespace XRayBuilder.Core.XRay.Logic.Export
{
    public interface IXRayExporter
    {
        void Export(XRay xray, string path, IProgressBar progress, CancellationToken cancellationToken = default);
    }
}