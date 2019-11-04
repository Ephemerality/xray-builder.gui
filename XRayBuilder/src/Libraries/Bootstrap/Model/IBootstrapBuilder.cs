using SimpleInjector;

namespace XRayBuilderGUI.Libraries.Bootstrap.Model
{
    public interface IBootstrapBuilder
    {
        void Register<TBootstrap>() where TBootstrap : IBootstrapSegment, new();
        Container Build();
    }
}