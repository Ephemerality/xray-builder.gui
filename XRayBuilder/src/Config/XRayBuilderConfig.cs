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
        public bool ShortenExcerptsLegacy => false;
    }
}