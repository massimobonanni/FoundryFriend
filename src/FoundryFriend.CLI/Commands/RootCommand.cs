using FoundryFriend.CLI.Commands.Set;
using FoundryFriend.CLI.Commands.Chat;
using FoundryFriend.CLI.Commands.Agent;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands;

internal class RootCommand : System.CommandLine.RootCommand
{
    public RootCommand(ServiceProvider serviceProvider) : base("Foundry Friend")
    {
        this.Description = "The command-line interface for Foundry demos.";

        this.Subcommands.Add(new SetCommand(serviceProvider.GetSessionManager()));
        this.Subcommands.Add(new ChatCommand(serviceProvider.GetSessionManager(), serviceProvider.GetChatService()));
        this.Subcommands.Add(new AgentCommand(serviceProvider.GetSessionManager(), serviceProvider.GetAgentChatService(), serviceProvider.GetAgentService()));

        for (int i = 0; i < this.Options.Count; i++)
        {
            if (this.Options[i] is VersionOption defaultVersionOption)
            {
                defaultVersionOption.Action = new CustomVersionAction();
            }
        }
    }
}
