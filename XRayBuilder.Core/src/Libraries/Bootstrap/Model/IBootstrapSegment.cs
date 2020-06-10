namespace XRayBuilder.Core.Libraries.Bootstrap.Model
{
    /// <summary>
    /// A segment containing additional bootstrap registrations, ie this bootstrap file requires other bootstrap files
    /// </summary>
    public interface IBootstrapSegment
    {
        /// <summary>
        /// Allows registration of the additional bootstrap files
        /// </summary>
        void Register(IBootstrapBuilder builder);
    }
}