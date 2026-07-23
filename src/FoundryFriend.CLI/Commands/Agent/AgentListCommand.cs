using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;

/// <summary>
/// Lists all agents in an Azure AI Foundry project.
/// The command handler is thin — it parses input, validates configuration, and delegates
/// the listing to <see cref="IAgentService"/>.
/// </summary>
internal class AgentListCommand : CommandBase
{
    private readonly IAgentService _agentService;
    private readonly Option<string> _projectNameOption;

    public AgentListCommand(ISessionManager sessionManager, IAgentService agentService)
        : base("list", "Display the list of the agent configured", sessionManager)
    {
        _agentService = agentService;

        _projectNameOption = new Option<string>("--project-name")
        {
            Description = "The name of the project in Foundry",
            Required = true
        };
        _projectNameOption.Aliases.Add("-p");
        this.Options.Add(_projectNameOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var projectName = parseResult.GetValue(_projectNameOption)!;

        // 1. Load and validate session configuration
        await _sessionManager.LoadSettingsAsync();

        var endpoint = _sessionManager.GetEndpoint();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            ConsoleUtility.WriteLine("Error: endpoint not configured. Use 'set' command to configure it.", ConsoleColor.Red);
            return;
        }

        // 2. Initialize the service and list agents
        ConsoleUtility.WriteLine("Connecting to Microsoft Foundry", ConsoleColor.Green);
        _agentService.Initialize(endpoint, projectName);

        try
        {
            ConsoleUtility.WriteLine($"Agents in project '{projectName}':", ConsoleColor.Green);

            await foreach (var agentInfo in _agentService.ListAgentsAsync(cancellationToken))
            {
                ConsoleUtility.WriteLine($"\nId: {agentInfo.Id}, Name: {agentInfo.Name}", ConsoleColor.White);
            }
        }
        catch (Exception ex)
        {
            ConsoleUtility.WriteLine($"Error listing agents: {ex.Message}", ConsoleColor.Red);
        }
    }
}