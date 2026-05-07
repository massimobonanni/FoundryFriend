using FoundryFriend.CLI.Commands.Set;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands;

internal class RootCommand : System.CommandLine.RootCommand
{
    public RootCommand(ServiceProvider serviceProvider) : base("Foundry Friend")
    {
        this.Description = "The command-line interface for Foundry demos.";

        this.Subcommands.Add(new SetCommand(serviceProvider.GetSessionManager()));
        
        for (int i = 0; i < this.Options.Count; i++)
        {
            if (this.Options[i] is VersionOption defaultVersionOption)
            {
                defaultVersionOption.Action = new CustomVersionAction();
            }
        }
    }
}
