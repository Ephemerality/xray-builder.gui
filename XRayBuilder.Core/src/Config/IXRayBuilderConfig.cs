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
        string AmazonTld { get; }
        /// <summary>
        /// Save RAWML during extraction if possible
        /// </summary>
        bool SaveRawl { get; }
        bool OverwriteChapters { get; }
        bool OverwriteAliases { get; }
        bool SplitAliases { get; }
        bool EnableEdit { get; }
        bool SkipNoLikes { get; }
        int MinimumClipLength { get; }
        bool IgnoreSoftHyphen { get; }
    }
}