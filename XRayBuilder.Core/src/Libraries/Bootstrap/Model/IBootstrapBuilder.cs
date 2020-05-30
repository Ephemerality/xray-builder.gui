using SimpleInjector;

namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    /// <summary>
    /// Enables the registration of various distributed bootstrap files that can register their own depedencies per domain
    /// </summary>
    public interface IBootstrapBuilder
    {
        /// <summary>
        /// Registers the <typeparamref name="TBootstrap"/> to ensure its own dependencies are also registered
        /// </summary>
        void Register<TBootstrap>() where TBootstrap : IBootstrapSegment, new();

        /// <summary>
        /// Builds the final container
        /// </summary>
        Container Build();
    }
}