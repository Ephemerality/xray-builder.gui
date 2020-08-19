using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using SimpleInjector;
using XRayBuilder.Console.Extensions;
using XRayBuilder.Core.Config;

namespace XRayBuilder.Console.Command
{
    public sealed class CommandXRay : ICommand
    {
        private readonly BaseOptions _baseOptions;
        private readonly Func<IXRayBuilderConfig, Container> _bootstrap;

        public string Name { get; } = "xray";

        public CommandXRay(BaseOptions baseOptions, Func<IXRayBuilderConfig, Container> bootstrap)
        {
            _baseOptions = baseOptions;
            _bootstrap = bootstrap;
        }

        public void Configure(CommandLineApplication cli)
        {
            var xrayOptions = new XRayBuildOptions
            {
                DataUrl = cli.Option("--dataurl", "URL (or path to an XML/TXT file) from which the terms should be loaded. If not specified, terms will come from Roentgen if possible.", CommandOptionType.SingleValue),
                AliasFile = cli.Option("--aliases", "Optional path to an alias file. If not specified, the default path will be used.", CommandOptionType.SingleValue),
                IncludeTopics = cli.Option("--topics", "When set, topics will be included with the set of imported terms. Default is false.", CommandOptionType.NoValue),
                SplitAliases = cli.Option("--splitaliases", "When set, aliases will be automatically split apart using a set of common titles, prefixes, etc.", CommandOptionType.NoValue)
            };
            cli.OnExecute(cancellationToken => OnExecute(xrayOptions, cancellationToken));
        }

        private sealed class XRayBuildOptions
        {
            public CommandOption DataUrl { get; set; }
            public CommandOption AliasFile { get; set; }
            public CommandOption IncludeTopics { get; set; }
            public CommandOption SplitAliases { get; set; }
        }

        private async Task<int> OnExecute(XRayBuildOptions xrayBuildOptions, CancellationToken cancellationToken)
        {
            var config = new XRayBuilderConfig
            {
                UseSubdirectories = _baseOptions.UseSubdirectories.HasValue(),
                BaseOutputDirectory = _baseOptions.BaseOutputDirectory.HasValue()
                    ? _baseOptions.BaseOutputDirectory.Value()
                    : $@"{Environment.CurrentDirectory}\out",
                BuildForAndroid = _baseOptions.Android.HasValue(),
                OutputToSidecar = _baseOptions.OutputToSidecar.HasValue()
            };

            using var container = _bootstrap(config);

            return 0;
        }
    }
}