using SimpleInjector;
using XRayBuilder.Core.DataSources.Amazon.Bootstrap;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Extras.EndActions;
using XRayBuilder.Core.Extras.StartActions;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Http.Bootstrap;

namespace XRayBuilder.Core.Extras.Bootstrap
{
    public sealed class BootstrapExtras : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
            builder.Register<BootstrapHttp>();
            builder.Register<BootstrapAmazon>();
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