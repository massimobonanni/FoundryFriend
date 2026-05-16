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

internal class AgentListCommand : CommandBase
{
    private readonly Option<string> _projectNameOption;

    public AgentListCommand(ISessionManager sessionManager)
        : base("list", "Display the list of the agent configured", sessionManager)
    {
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

        ConsoleUtility.WriteLine("Connecting to Microsoft Foundry", ConsoleColor.Green);

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
            var agentList = projectClient.AgentAdministrationClient.GetAgentsAsync(
                cancellationToken: cancellationToken);

            ConsoleUtility.WriteLine($"Agents in project '{projectName}':", ConsoleColor.Green);
            await foreach (var agentInfo in agentList.WithCancellation(cancellationToken))
            {
                Console.WriteLine($"\nId: {agentInfo.Id}, Name: {agentInfo.Name}");
            }
        }
        catch (Exception ex)
        {
            ConsoleUtility.WriteLine($"Error creating agent: {ex.Message}", ConsoleColor.Red);
        }
    }
}