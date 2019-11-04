using SimpleInjector;

namespace XRayBuilderGUI.Libraries.Bootstrap.Model
{
    public interface IContainerSegment
    {
        void Register(Container container);
    }
}