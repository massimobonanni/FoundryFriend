using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;


internal class AgentCommand : CommandBase
{
   
    public AgentCommand(ISessionManager sessionManager) :
        base("agent", "Manages agents in Foundry", sessionManager)
    {
        this.Subcommands.Add(new AgentCreateCommand(sessionManager));
        this.Subcommands.Add(new AgentChatCommand(sessionManager));

    }

}
