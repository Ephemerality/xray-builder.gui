using SimpleInjector;

namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    /// <summary>
    /// A segment containing specific dependency registrations
    /// </summary>
    public interface IContainerSegment
    {
        /// <summary>
        /// Allows registration of regular dependencies into the <paramref name="container"/>
        /// </summary>
        /// <param name="container"></param>
        void Register(Container container);
    }
}