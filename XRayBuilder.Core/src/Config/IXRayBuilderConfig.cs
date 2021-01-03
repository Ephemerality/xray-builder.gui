namespace XRayBuilder.Core.Config
{
    public interface IXRayBuilderConfig
    {
        bool BuildForAndroid { get; }
        string BaseOutputDirectory { get; }
        bool OutputToSidecar { get; }
        bool UseSubdirectories { get; }
        bool UseNewVersion { get; }
        /// <summary>
        /// When the old version is being built, enables excerpt shortening
        /// </summary>
        bool ShortenExcerptsLegacy { get; }
    }
}