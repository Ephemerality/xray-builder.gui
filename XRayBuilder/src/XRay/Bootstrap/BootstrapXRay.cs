using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;
using XRayBuilderGUI.Libraries.SimpleInjector.Extensions;
using XRayBuilderGUI.XRay.Logic;
using XRayBuilderGUI.XRay.Logic.Aliases;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Logic.Export;
using XRayBuilderGUI.XRay.Logic.Terms;

namespace XRayBuilderGUI.XRay.Bootstrap
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
            container.RegisterSingleton<ITermsService, TermsService>();
            container.RegisterSingleton<IAliasesService, AliasesService>();
            container.RegisterSingleton<IAliasesRepository, AliasesRepository>();
            container.RegisterSingleton<IPreviewDataExporter, PreviewDataExporter>();
            container.RegisterSingleton<ChaptersService>();
        }
    }
}