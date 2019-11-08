using System.Collections.Generic;
using SimpleInjector;

namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    public interface IBootstrapRegistrar
    {
        void RegisterSegments(IEnumerable<IBootstrapSegment> segments, Container container);
    }
}