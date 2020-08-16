namespace XRayBuilder.Core.Config
{
    public interface IXRayBuilderConfig
    {
        bool BuildForAndroid { get; }
        string BaseOutputDirectory { get; }
        bool OutputToSidecar { get; }
        bool UseSubdirectories { get; }
    }
}