using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;

/// <summary>
/// Deletes an agent from an Azure AI Foundry project.
/// The command handler is thin — it parses input, validates configuration, and delegates
/// the deletion to <see cref="IAgentService"/>.
/// </summary>
internal class AgentDeleteCommand : CommandBase
{
    private readonly IAgentService _agentService;
    private readonly Option<string> _projectNameOption;
    private readonly Option<string> _agentIdOption;

    public AgentDeleteCommand(ISessionManager sessionManager, IAgentService agentService)
        : base("delete", "Delete an agent from Foundry project", sessionManager)
    {
        _agentService = agentService;

        _agentIdOption = new Option<string>("--agent-id")
        {
            Description = "The id of the agent to delete",
            Required = true
        };
        _agentIdOption.Aliases.Add("-id");
        this.Options.Add(_agentIdOption);

        _projectNameOption = new Option<string>("--project-name")
        {
            Description = "The name of the project in Foundry contains the agent",
            Required = true
        };
        _projectNameOption.Aliases.Add("-p");
        this.Options.Add(_projectNameOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var agentId = parseResult.GetValue(_agentIdOption);
        var projectName = parseResult.GetValue(_projectNameOption);

        // 1. User confirmation for destructive operation
        ConsoleUtility.WriteLine($"Are you sure you want to delete the agent {agentId}? This action cannot be undone. (yes/no)", ConsoleColor.White);
        var confirmation = Console.ReadLine();
        if (confirmation?.ToLower() != "yes" && confirmation?.ToLower() != "y")
        {
            ConsoleUtility.WriteLine("Operation cancelled.", ConsoleColor.Red);
            return;
        }

        // 2. Load and validate session configuration
        await _sessionManager.LoadSettingsAsync();

        var endpoint = _sessionManager.GetEndpoint();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            ConsoleUtility.WriteLine("Error: endpoint not configured. Use 'set' command to configure it.", ConsoleColor.Red);
            return;
        }

        var authMode = _sessionManager.GetAuthenticationMode();
        if (authMode == AuthenticationMode.Key)
        {
            ConsoleUtility.WriteLine("Error: Agent deletion requires Identity authentication.", ConsoleColor.Red);
            return;
        }

        // 3. Initialize the service and delete the agent
        _agentService.Initialize(endpoint, projectName!);

        try
        {
            ConsoleUtility.WriteLine($"Deleting agent '{agentId}'...", ConsoleColor.Cyan);

            await _agentService.DeleteAgentAsync(agentId!, cancellationToken);

            ConsoleUtility.WriteLine($"Agent deleted successfully.", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            ConsoleUtility.WriteLine($"Error deleting agent: {ex.Message}", ConsoleColor.Red);
        }
    }
}