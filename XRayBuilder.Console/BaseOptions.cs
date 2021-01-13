using McMaster.Extensions.CommandLineUtils;

namespace XRayBuilder.Console
{
    public sealed class BaseOptions
    {
        public CommandArgument Book { get; set; }
        public CommandOption Android { get; set; }
        public CommandOption OutputToSidecar { get; set; }
        public CommandOption BaseOutputDirectory { get; set; }
        public CommandOption UseSubdirectories { get; set; }
        public CommandOption FixAsin { get; set; }
        public CommandOption AmazonTld { get; set; }
    }
}