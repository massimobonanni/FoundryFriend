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

internal class AgentDeleteCommand : CommandBase
{
    private readonly Option<string> _projectNameOption;
    private readonly Option<string> _agentIdOption;

    public AgentDeleteCommand(ISessionManager sessionManager)
        : base("delete", "Delete an agent from Foundry project", sessionManager)
    {
        _agentIdOption = new Option<string>("--agent-id")
        {
            Description = "The id of the agent to delete",
            Required = true
        };
        _agentIdOption.Aliases.Add("-id");
        this.Options.Add(_agentIdOption);

        _projectNameOption = new Option<string>("--project-name")
        {
            Description = "The name of the project in Foundry contaims the agent",
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

        System.Console.WriteLine($"Are you sure you want to delete the agent {agentId}? This action cannot be undone. (yes/no)");
        var confirmation = System.Console.ReadLine();
        if (confirmation?.ToLower() != "yes" || confirmation?.ToLower() != "y")
        {
            ConsoleUtility.WriteLine("Operation cancelled.", ConsoleColor.Red);
            return;
        }

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
            ConsoleUtility.WriteLine($"Deleting agent '{agentId}'...", ConsoleColor.Cyan);

            var agentVersion = await projectClient.AgentAdministrationClient.DeleteAgentAsync(
                agentName: agentId,
                cancellationToken: cancellationToken);

            ConsoleUtility.WriteLine($"Agent deleted successfully.", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            ConsoleUtility.WriteLine($"Error deleting agent: {ex.Message}", ConsoleColor.Red);
        }
    }
}