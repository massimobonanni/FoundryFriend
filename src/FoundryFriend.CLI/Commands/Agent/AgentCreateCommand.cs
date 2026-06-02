using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Configuration;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;

/// <summary>
/// Creates an agent in Azure AI Foundry from a JSON configuration file.
/// The command handler is thin — it parses input, validates configuration, and delegates
/// all agent creation logic to <see cref="IAgentService"/>.
/// </summary>
internal class AgentCreateCommand : CommandBase
{
    private readonly IAgentService _agentService;
    private readonly Argument<string> _fileArgument;
    private readonly Option<string> _projectNameOption;

    public AgentCreateCommand(ISessionManager sessionManager, IAgentService agentService)
        : base("create", "Create an agent from a configuration file", sessionManager)
    {
        _agentService = agentService;

        _fileArgument = new Argument<string>("file")
        {
            Description = "Path to the agent configuration JSON file"
        };
        this.Arguments.Add(_fileArgument);

        _projectNameOption = new Option<string>("--project-name")
        {
            Description = "The name of the project in Foundry to create the agent in",
            Required = true
        };
        _projectNameOption.Aliases.Add("-p");
        this.Options.Add(_projectNameOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var filePath = parseResult.GetValue(_fileArgument)!;

        if (!File.Exists(filePath))
        {
            ConsoleUtility.WriteLine($"Error: file '{filePath}' not found.", ConsoleColor.Red);
            return;
        }

        var config = await AgentConfiguration.LoadFromFileAsync(filePath, cancellationToken);
        if (config is null)
        {
            ConsoleUtility.WriteLine("Error: failed to parse agent configuration file.", ConsoleColor.Red);
            return;
        }

        var projectName = parseResult.GetValue(_projectNameOption)!;

        // 1. Load and validate session configuration
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
            ConsoleUtility.WriteLine("Error: Agent creation requires Identity authentication.", ConsoleColor.Red);
            return;
        }

        // 2. Initialize the service and create the agent
        _agentService.Initialize(endpoint, projectName);

        try
        {
            ConsoleUtility.WriteLine($"Creating agent '{config.Name}'...", ConsoleColor.Cyan);

            var agentVersion = await _agentService.CreateAgentAsync(
                config.Id, config.ModelDeploymentName, config.GetInstructionsAsString(), cancellationToken);

            ConsoleUtility.WriteLine($"Agent created successfully.", ConsoleColor.Green);
            ConsoleUtility.WriteLine($"  Id      : {agentVersion.Id}");
            ConsoleUtility.WriteLine($"  Name    : {agentVersion.Name}");
            ConsoleUtility.WriteLine($"  Version : {agentVersion.Version}");
        }
        catch (Exception ex)
        {
            ConsoleUtility.WriteLine($"Error creating agent: {ex.Message}", ConsoleColor.Red);
        }
    }
}