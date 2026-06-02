using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;


internal class AgentCommand : CommandBase
{
    public AgentCommand(ISessionManager sessionManager, IAgentChatService agentChatService, IAgentService agentService) :
        base("agent", "Manages agents in Foundry", sessionManager)
    {
        this.Subcommands.Add(new AgentCreateCommand(sessionManager, agentService));
        this.Subcommands.Add(new AgentChatCommand(sessionManager, agentChatService));
        this.Subcommands.Add(new AgentListCommand(sessionManager, agentService));
        this.Subcommands.Add(new AgentDeleteCommand(sessionManager, agentService));
    }
}
