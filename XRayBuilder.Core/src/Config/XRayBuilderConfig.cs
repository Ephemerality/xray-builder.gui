namespace XRayBuilder.Core.Config
{
    public sealed class XRayBuilderConfig
    {
        public bool BuildForAndroid { get; set; }
        public string BaseOutputDirectory { get; set; }
        public bool OutputToSidecar { get; set; }
        public bool UseSubdirectories { get; set; }
    }
}