using XRayBuilder.Core.Config;

namespace XRayBuilderGUI.Config
{
    public sealed class XRayBuilderConfig : IXRayBuilderConfig
    {
        public bool BuildForAndroid => Properties.Settings.Default.android;
        public string BaseOutputDirectory => Properties.Settings.Default.outDir;
        public bool OutputToSidecar => Properties.Settings.Default.outputToSidecar;
        public bool UseSubdirectories => Properties.Settings.Default.useSubDirectories;
        public bool UseNewVersion => Properties.Settings.Default.useNewVersion;
        public bool ShortenExcerptsLegacy => !UseNewVersion;
        public string AmazonTld => Properties.Settings.Default.amazonTLD;
        public bool SaveRawl => Properties.Settings.Default.saverawml;
        public bool OverwriteChapters => Properties.Settings.Default.overwriteChapters;
        public bool OverwriteAliases => Properties.Settings.Default.overwriteAliases;
        public bool SplitAliases => Properties.Settings.Default.splitAliases;
        public bool EnableEdit => Properties.Settings.Default.enableEdit;
        public bool SkipNoLikes => Properties.Settings.Default.skipNoLikes;
        public int MinimumClipLength => Properties.Settings.Default.minClipLen;
        public bool IgnoreSoftHyphen => Properties.Settings.Default.ignoresofthyphen;
    }
}