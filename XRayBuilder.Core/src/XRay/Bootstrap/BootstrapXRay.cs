using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;
using XRayBuilder.Core.XRay.Logic;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Export;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Core.XRay.Model.Export;

namespace XRayBuilder.Core.XRay.Bootstrap
{
    public sealed class BootstrapXRay : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.AutoregisterConcreteFromInterface<IXRayExporter>(Lifestyle.Singleton);
            container.RegisterSingleton<IXRayService, XRayService>();
            container.RegisterSingleton<IKfxXrayService, KfxXrayService>();
            container.RegisterSingleton<ITermsService, TermsService>();
            container.RegisterSingleton<IAliasesService, AliasesService>();
            container.RegisterSingleton<IAliasesRepository, AliasesRepository>();
            container.RegisterSingleton<IPreviewDataExporter, PreviewDataExporter>();
            container.RegisterSingleton<ChaptersService>();
            container.RegisterSingleton<XRayExporterFactory>();
        }
    }
}