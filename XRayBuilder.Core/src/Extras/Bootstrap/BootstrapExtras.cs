using SimpleInjector;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Extras.EndActions;
using XRayBuilder.Core.Extras.StartActions;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Extras.Bootstrap
{
    public sealed class BootstrapExtras : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IAuthorProfileGenerator, AuthorProfileGenerator>();
            container.RegisterSingleton<IStartActionsArtifactService, StartActionsArtifactService>();
            container.RegisterSingleton<IEndActionsArtifactService, EndActionsArtifactService>();
            container.RegisterSingleton<IEndActionsAuthorConverter, EndActionsAuthorConverter>();
            container.RegisterSingleton<IEndActionsDataGenerator, EndActionsDataGenerator>();
        }
    }
}