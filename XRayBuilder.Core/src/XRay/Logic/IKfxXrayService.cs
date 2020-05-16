using System.Threading;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Unpack.KFX;

namespace XRayBuilder.Core.XRay.Logic
{
    public interface IKfxXrayService
    {
        void AddLocations(XRay xray,
            KfxContainer kfx,
            bool skipNoLikes,
            int minClipLen,
            IProgressBar progress,
            CancellationToken token);
    }
}