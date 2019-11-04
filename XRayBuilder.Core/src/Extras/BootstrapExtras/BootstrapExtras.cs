using SimpleInjector;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Extras.BootstrapExtras
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