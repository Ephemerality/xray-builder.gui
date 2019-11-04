using System.Collections.Generic;
using SimpleInjector;

namespace XRayBuilderGUI.Libraries.Bootstrap.Model
{
    public interface IBootstrapRegistrar
    {
        void RegisterSegments(IEnumerable<IBootstrapSegment> segments, Container container);
    }
}