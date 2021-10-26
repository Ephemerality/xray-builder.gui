using System.Threading;
using Ephemerality.Unpack.KFX;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries.Progress;

namespace XRayBuilder.Core.XRay.Logic
{
    public interface IKfxXrayService
    {
        void AddLocations(XRay xray,
            KfxContainer kfx,
            [CanBeNull] IProgressBar progress,
            CancellationToken token);
    }
}