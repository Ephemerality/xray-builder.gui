using Microsoft.Extensions.CommandLineUtils;

namespace XRayBuilder.Console.Command
{
    public interface ICommand
    {
        string Name { get; }
        void Configure(CommandLineApplication cli);
    }
}