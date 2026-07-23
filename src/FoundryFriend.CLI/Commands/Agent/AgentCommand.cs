using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;

/// <summary>
/// Groups the agent-related subcommands (create, chat, list, delete) under the <c>agent</c> command.
/// </summary>
internal class AgentCommand : CommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentCommand"/> class and registers its subcommands.
    /// </summary>
    /// <param name="sessionManager">The session manager used to access stored configuration.</param>
    /// <param name="agentChatService">The service used to perform agent chat operations.</param>
    /// <param name="agentService">The service used to perform agent administration operations.</param>
    public AgentCommand(ISessionManager sessionManager, IAgentChatService agentChatService, IAgentService agentService) :
        base("agent", "Manages agents in Foundry", sessionManager)
    {
        this.Subcommands.Add(new AgentCreateCommand(sessionManager, agentService));
        this.Subcommands.Add(new AgentChatCommand(sessionManager, agentChatService));
        this.Subcommands.Add(new AgentListCommand(sessionManager, agentService));
        this.Subcommands.Add(new AgentDeleteCommand(sessionManager, agentService));
    }
}
