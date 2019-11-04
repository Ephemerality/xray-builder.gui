using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.Libraries.Bootstrap.Logic
{
    public sealed class BootstrapRegistrar : IBootstrapRegistrar
    {
        public void RegisterSegments(IEnumerable<IBootstrapSegment> segments, Container container)
        {
            foreach (var segment in segments.OfType<IContainerSegment>())
                segment.Register(container);
        }
    }
}