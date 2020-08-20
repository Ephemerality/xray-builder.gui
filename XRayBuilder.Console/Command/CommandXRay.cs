using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SimpleInjector;
using XRayBuilder.Console.Logic;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;

namespace XRayBuilder.Console.Command
{
    public sealed class CommandXRay : ICommand
    {
        private readonly Func<CommandLineApplication, BaseOptions> _configureBaseOptions;
        private readonly Func<IXRayBuilderConfig, Container> _bootstrap;

        public string Name { get; } = "xray";

        public CommandXRay(Func<CommandLineApplication, BaseOptions> configureBaseOptions, Func<IXRayBuilderConfig, Container> bootstrap)
        {
            _configureBaseOptions = configureBaseOptions;
            _bootstrap = bootstrap;
        }

        public void Configure(CommandLineApplication cli)
        {
            var baseOptions = _configureBaseOptions(cli);
            var xrayOptions = new XRayBuildOptions
            {
                DataUrl = cli.Option("--dataurl", "URL (or path to an XML/TXT file) from which the terms should be loaded. If not specified, terms will come from Roentgen if possible.", CommandOptionType.SingleValue),
                // AliasFile = cli.Option("--aliases", "Optional path to an alias file. If not specified, the default path will be used.", CommandOptionType.SingleValue),
                IncludeTopics = cli.Option("--topics", "When set, topics will be included with the set of imported terms. Default is false.", CommandOptionType.NoValue),
                SplitAliases = cli.Option("--splitaliases", "When set, aliases will be automatically split apart using a set of common titles, prefixes, etc.", CommandOptionType.NoValue)
            };
            cli.OnExecuteAsync(cancellationToken => ConsoleHost.WaitForShutdownAsync(ct => OnExecute(baseOptions, xrayOptions, ct)));
        }

        private sealed class XRayBuildOptions
        {
            public CommandOption DataUrl { get; set; }
            // public CommandOption AliasFile { get; set; }
            public CommandOption IncludeTopics { get; set; }
            public CommandOption SplitAliases { get; set; }
        }

        private async Task<int> OnExecute(BaseOptions baseOptions, XRayBuildOptions xrayBuildOptions, CancellationToken cancellationToken)
        {
            var config = new XRayBuilderConfig
            {
                UseSubdirectories = baseOptions.UseSubdirectories.HasValue(),
                BaseOutputDirectory = baseOptions.BaseOutputDirectory.HasValue()
                    ? baseOptions.BaseOutputDirectory.Value()
                    : $@"{AppDomain.CurrentDomain.BaseDirectory}\out",
                BuildForAndroid = baseOptions.Android.HasValue(),
                OutputToSidecar = baseOptions.OutputToSidecar.HasValue()
            };
            using var container = _bootstrap(config);
            var logger = container.GetInstance<ILogger>();

            var bookPath = (string) baseOptions.Book.Value;
            if (bookPath == null || !File.Exists(bookPath))
            {
                logger.Log($"Book not found: {bookPath}");
                return 1;
            }

            var xrayBuildService = container.GetInstance<XRay>();
            var request = new XRay.Request(
                bookPath: bookPath,
                dataUrl: xrayBuildOptions.DataUrl.Value() ?? SecondarySourceRoentgen.FakeUrl,
                includeTopics: xrayBuildOptions.IncludeTopics.HasValue(),
                splitAliases: xrayBuildOptions.SplitAliases.HasValue(),
                amazonTld: baseOptions.AmazonTld.Value());
            var xrayPath = await xrayBuildService.BuildAsync(request, cancellationToken);

            return 0;
        }
    }
}