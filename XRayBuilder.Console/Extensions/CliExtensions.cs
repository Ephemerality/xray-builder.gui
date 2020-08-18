using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace XRayBuilder.Console.Extensions
{
    public static class CliExtensions
    {
        public static void OnExecute(this CommandLineApplication cli, Func<CancellationToken, Task<int>> onExecute)
        {
            cli.OnExecute(() => ConsoleHost.WaitForShutdownAsync(onExecute));
        }
    }
}