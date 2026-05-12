using Azure;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Inference;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Configuration;   // AgentConfiguration
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using OpenAI.Responses;
using System.CommandLine;
using System.Net;
using System.Security.Cryptography;

#pragma warning disable OPENAI001

namespace FoundryFriend.CLI.Commands.Agent;

internal class AgentChatCommand : CommandBase
{
    private readonly Option<string> _agentIdOption;
    private readonly Option<string> _projectNameOption;

    public AgentChatCommand(ISessionManager sessionManager)
        : base("chat", "Start a new chat with an agent", sessionManager)
    {
        _agentIdOption = new Option<string>("--agent-id")
        {
            Description = "The id of the agent to use in the chat",
            Required = true
        };
        _agentIdOption.Aliases.Add("-id");
        this.Options.Add(_agentIdOption);

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
        var agentId = parseResult.GetValue(_agentIdOption);
        var projectName = parseResult.GetValue(_projectNameOption);

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

        // Create a conversation for multi-turn chat
        ProjectConversation conversation = projectClient.ProjectOpenAIClient.GetProjectConversationsClient().CreateProjectConversation();

        // Chat with the agent to answer questions
        ProjectResponsesClient responsesClient = projectClient.ProjectOpenAIClient.GetProjectResponsesClientForAgent(
            defaultAgent: agentId,
            defaultConversationId: conversation.Id);

        ConsoleUtility.WriteLine($"Chat started with agent '{agentId}'. Type 'exit' or 'quit' to stop.", ConsoleColor.Green);
        ConsoleUtility.WriteLine(new string('-', 50), ConsoleColor.Green);
        ConsoleUtility.WriteLine();

        // Multi-turn chat loop
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsoleUtility.Write("You: ", ConsoleColor.White);

            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleUtility.WriteLine("Chat ended.", ConsoleColor.Cyan);
                break;
            }

            try
            {
                var response = responsesClient.CreateResponseStreamingAsync(userInput);

                ConsoleUtility.Write("Assistant: ", ConsoleColor.Yellow);

                await foreach (StreamingResponseUpdate? update in response.WithCancellation(cancellationToken))
                {
                    if (update != null)
                    {
                        switch (update)
                        {
                            case StreamingResponseOutputTextDeltaUpdate textDelta:
                                ConsoleUtility.Write(textDelta.Delta, ConsoleColor.Yellow);
                                break;
                            case StreamingResponseOutputTextDoneUpdate textDone:
                                // Final assembled text for this output item
                                break;
                            case StreamingResponseOutputItemDoneUpdate itemDone:
                                // An output item completed
                                break;
                            case StreamingResponseFunctionCallArgumentsDeltaUpdate funcArgsDelta:
                                break;
                            case StreamingResponseCreatedUpdate created:
                                // Response object created
                                break;
                            case StreamingResponseCompletedUpdate completed:
                                // Response fully completed
                                break;
                            default:
                                break;
                        }
                    }
                }

                ConsoleUtility.WriteLine();

            }
            catch (RequestFailedException ex)
            {
                ConsoleUtility.WriteLine($"Error: {ex.Message} (Status: {ex.Status})", ConsoleColor.Red);
            }
        }
    }
}