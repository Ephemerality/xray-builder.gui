using SimpleInjector;
using XRayBuilderGUI.Extras.AuthorProfile;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.Extras.BootstrapExtras
{
    public sealed class BootstrapExtras : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IAuthorProfileGenerator, AuthorProfileGenerator>();
        }
    }
}