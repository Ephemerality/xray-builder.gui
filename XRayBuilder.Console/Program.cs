using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SimpleInjector;
using XRayBuilder.Console.Bootstrap;
using XRayBuilder.Console.Command;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Bootstrap.Logic;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;

namespace XRayBuilder.Console
{
    internal static class Program
    {
        private static Task<int> Main(string[] args)
        {
            var cli = new CommandLineApplication
            {
                Name = "xray-console",
                FullName = "X-Ray Builder Console",
                Description = "Command-line access to X-Ray Builder"
            };
            cli.HelpOption("-h|--help");

            var baseConfigOptions = AddBaseConfig(cli);

            var commands = new[]
            {
                new CommandXRay(baseConfigOptions, Bootstrap)
            };
            foreach (var command in commands)
            {
                cli.Command(command.Name, app => command.Configure(app));
            }

            return cli.ExecuteAsync(args);
        }

        private static BaseOptions AddBaseConfig(CommandLineApplication cli)
            => new BaseOptions
            {
                Book = cli.Argument("book", "Path to the book to be processed (mobi, azw3, kfx only).").IsRequired(),
                Android = cli.Option("--android", "Build the X-Ray for Android.", CommandOptionType.NoValue),
                BaseOutputDirectory = cli.Option("-o|--output", "Specify the base output directory. Default is /out.", CommandOptionType.SingleValue),
                OutputToSidecar = cli.Option("--sidecar", "Output to a sidecar directory within the output directory, eg Book.sdr", CommandOptionType.NoValue),
                UseSubdirectories = cli.Option("--subdir", "Use author/title subdirectories within the output directory.", CommandOptionType.NoValue),
                FixAsin = cli.Option("--fixasin", "When specified, invalid ASINs will be searched on Amazon and fixed, if possible. (This will modify the book meaning it will need to be re-copied to your Kindle device) -- THIS FEATURE IS EXPERIMENTAL AND COULD DESTROY YOUR BOOK", CommandOptionType.NoValue),
                AmazonTld = cli.Option("--tld", "Amazon TLD, eg com, co,uk, de, etc. Default is com.", CommandOptionType.SingleValue)
            };

        private static Container Bootstrap(IXRayBuilderConfig xrayBuilderConfig)
        {
            var container = new Container();

            container.RegisterSingleton(() => xrayBuilderConfig);

            var builder = new BootstrapBuilder(container);
            builder.Register<BootstrapConsole>();

            return builder.Build();
        }
    }
}
