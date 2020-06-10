using System.Collections.Generic;
using SimpleInjector;

namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    /// <summary>
    /// Handles the dependency registration of the individual distributed <see cref="IBootstrapSegment"/>
    /// </summary>
    public interface IBootstrapRegistrar
    {
        /// <summary>
        /// Calls the registration function on all <paramref name="segments"/> to populate the <paramref name="container"/>
        /// </summary>
        void RegisterSegments(IEnumerable<IBootstrapSegment> segments, Container container);
    }
}