using System;
using System.Collections.Generic;
using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Libraries.Bootstrap.Logic
{
    public sealed class BootstrapBuilder : IBootstrapBuilder
    {
        private readonly List<IBootstrapSegment> _segments = new List<IBootstrapSegment>();
        private readonly IBootstrapRegistrar _registrar = new BootstrapRegistrar();
        private readonly Container _container;

        private readonly HashSet<Type> _types = new HashSet<Type>();

        public BootstrapBuilder(Container container)
        {
            _container = container;
        }

        public void Register<TBootstrapSegment>() where TBootstrapSegment : IBootstrapSegment, new()
        {
            var type = typeof(TBootstrapSegment);
            if (_types.Contains(type))
                return;

            _types.Add(type);

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