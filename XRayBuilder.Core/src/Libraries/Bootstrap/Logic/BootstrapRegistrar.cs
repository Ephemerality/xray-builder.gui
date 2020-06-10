using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Libraries.Bootstrap.Logic
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