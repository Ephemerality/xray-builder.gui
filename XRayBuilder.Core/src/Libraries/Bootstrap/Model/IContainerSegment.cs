using SimpleInjector;

namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    public interface IContainerSegment
    {
        void Register(Container container);
    }
}