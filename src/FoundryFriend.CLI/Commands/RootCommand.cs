using FoundryFriend.CLI.Commands.Set;
using FoundryFriend.CLI.Commands.Chat;
using FoundryFriend.CLI.Commands.Agent;
using FoundryFriend.CLI.Commands.Login;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands;

/// <summary>
/// Represents the root command of the Foundry Friend CLI, registering all top-level subcommands
/// (set, chat, agent, login) and customizing the default version action.
/// </summary>
internal class RootCommand : System.CommandLine.RootCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootCommand"/> class, resolving and wiring up
    /// all subcommands from the dependency injection container.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve command dependencies.</param>
    public RootCommand(ServiceProvider serviceProvider) : base("Foundry Friend")
    {
        this.Description = "The command-line interface for Foundry demos.";

        this.Subcommands.Add(new SetCommand(serviceProvider.GetSessionManager()));
        this.Subcommands.Add(new ChatCommand(serviceProvider.GetSessionManager(), serviceProvider.GetChatService()));
        this.Subcommands.Add(new AgentCommand(serviceProvider.GetSessionManager(), serviceProvider.GetAgentChatService(), serviceProvider.GetAgentService()));
        this.Subcommands.Add(new LoginCommand(serviceProvider.GetSessionManager(), serviceProvider.GetAzCliService()));

        for (int i = 0; i < this.Options.Count; i++)
        {
            if (this.Options[i] is VersionOption defaultVersionOption)
            {
                defaultVersionOption.Action = new CustomVersionAction();
            }
        }
    }
}
