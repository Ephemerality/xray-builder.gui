using SimpleInjector;
using XRayBuilder.Core.DataSources.Logic;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Language.Localization;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;
using XRayBuilder.Core.Logic;

namespace XRayBuilder.Core.Bootstrap
{
    public sealed class BootstrapXRayBuilder : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IDirectoryService, DirectoryService>();
            container.RegisterSingleton<IBookSearchService, BookSearchService>();
            container.AutoregisterConcreteCollectionFromInterface<ILanguage>(Lifestyle.Singleton);
        }
    }
}