namespace XRayBuilder.Core.Config
{
    public sealed class XRayBuilderConfig : IXRayBuilderConfig
    {
        public bool BuildForAndroid { get; set; }
        public string BaseOutputDirectory { get; set; }
        public bool OutputToSidecar { get; set; }
        public bool UseSubdirectories { get; set; }
        public bool UseNewVersion { get; set; } = true;
        public bool ShortenExcerptsLegacy { get; set; }
        public string AmazonTld { get; set; }
        public bool SaveRawl { get; set; }
        public bool OverwriteChapters { get; set; }
        public bool OverwriteAliases { get; set; }
        public bool SplitAliases { get; set; }
        public bool EnableEdit { get; set; }
        public bool SkipNoLikes { get; set; }
        public int MinimumClipLength { get; set; }
        public bool IgnoreSoftHyphen { get; set; }
    }
}