namespace XRayBuilder.Core.Config
{
    public sealed class XRayBuilderConfig : IXRayBuilderConfig
    {
        public bool BuildForAndroid { get; set; }
        public string BaseOutputDirectory { get; set; }
        public bool OutputToSidecar { get; set; }
        public bool UseSubdirectories { get; set; }
        public bool UseNewVersion { get; set; }
        public bool ShortenExcerptsLegacy { get; set; }
    }
}