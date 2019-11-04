using System.Collections.Generic;
using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.Libraries.Bootstrap.Logic
{
    public sealed class BootstrapBuilder : IBootstrapBuilder
    {
        private readonly List<IBootstrapSegment> _segments = new List<IBootstrapSegment>();
        private readonly IBootstrapRegistrar _registrar = new BootstrapRegistrar();
        private readonly Container _container;

        public BootstrapBuilder(Container container)
        {
            _container = container;
        }

        public void Register<TBootstrapSegment>() where TBootstrapSegment : IBootstrapSegment, new()
        {
            var segment = new TBootstrapSegment();
            segment.Register(this);
            _segments.Add(segment);
        }

        public Container Build()
        {
            _registrar.RegisterSegments(_segments, _container);
            _container.Verify();
            return _container;
        }
    }
}