using Azure;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Configuration;   // AgentConfiguration
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using OpenAI.Responses;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;

internal class AgentCreateCommand : CommandBase
{
    private readonly Argument<string> _fileArgument;
    private readonly Option<string> _projectName;

    public AgentCreateCommand(ISessionManager sessionManager)
        : base("create", "Create an agent from a configuration file", sessionManager)
    {
        _fileArgument = new Argument<string>("file")
        {
            Description = "Path to the agent configuration JSON file"
        };
        this.Arguments.Add(_fileArgument);

        _projectName = new Option<string>("--project-name")
        {
            Description = "The name of the project in Foundry to create the agent in",
            Required = true
        };

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

        var projectName = parseResult.GetValue(_projectName)!;

        await _sessionManager.LoadSettingsAsync();

        var projectEndpoint = _sessionManager.GetEndpoint();
        if (string.IsNullOrWhiteSpace(projectEndpoint))
        {
            ConsoleUtility.WriteLine("Error: endpoint not configured. Use 'set' command to configure it.", ConsoleColor.Red);
            return;
        }

        // add string to the endpoint
        if (!projectEndpoint.EndsWith("/"))
        {
            projectEndpoint += "/";
        }
        projectEndpoint += $"api/projects/{projectName}";

        AIProjectClient projectClient = null!;
        var authMode = _sessionManager.GetAuthenticationMode();

        if (authMode == AuthenticationMode.Key)
        {
                ConsoleUtility.WriteLine("Error: The agent creation cannot be run with access key", ConsoleColor.Red);
                return;
        }
        else
        {
            projectClient = new AIProjectClient(new Uri(projectEndpoint), new DefaultAzureCredential());
        }

        try
        {
            
            ProjectsAgentDefinition agentDefinition = new DeclarativeAgentDefinition(config.ModelDeploymentName)
            { 
                Instructions = config.GetInstructionsAsString(),
            };

            ConsoleUtility.WriteLine($"Creating agent '{config.Name}'...", ConsoleColor.Cyan);

            var agentVersion = await projectClient.AgentAdministrationClient.CreateAgentVersionAsync(
                agentName: config.Id,
                options: new (agentDefinition),
                cancellationToken: cancellationToken);

            ConsoleUtility.WriteLine($"Agent created successfully.", ConsoleColor.Green);
            ConsoleUtility.WriteLine($"  Id      : {agentVersion.Value.Id}");
            ConsoleUtility.WriteLine($"  Name    : {agentVersion.Value.Name}");
            ConsoleUtility.WriteLine($"  Version : {agentVersion.Value.Version}");

            //if (config.McpServers is { Count: > 0 })
            //    ConsoleUtility.WriteLine($"  MCP servers: {string.Join(", ", config.McpServers.Select(m => m.ServerLabel))}");
        }
        catch (Exception ex)
        {
            ConsoleUtility.WriteLine($"Error creating agent: {ex.Message}", ConsoleColor.Red);
        }
    }
}