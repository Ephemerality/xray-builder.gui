using SimpleInjector;

namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    public interface IBootstrapBuilder
    {
        void Register<TBootstrap>() where TBootstrap : IBootstrapSegment, new();
        Container Build();
    }
}