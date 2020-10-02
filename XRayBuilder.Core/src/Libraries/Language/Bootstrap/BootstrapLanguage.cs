using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Language.Localization;

namespace XRayBuilder.Core.Libraries.Language.Bootstrap
{
    public sealed class BootstrapLanguage : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<LanguageFactory>();
        }
    }
}
