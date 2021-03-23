using System.Threading;
using Ephemerality.Unpack.KFX;
using XRayBuilder.Core.Libraries.Progress;

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