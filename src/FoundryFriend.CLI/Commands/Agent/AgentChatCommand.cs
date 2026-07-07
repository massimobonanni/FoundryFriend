using Azure;
using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Agent;

/// <summary>
/// Starts an interactive multi-turn chat with an agent in Azure AI Foundry.
/// The command handler is thin — it parses input, validates configuration, and delegates
/// all chat logic to <see cref="IAgentChatService"/>.
/// </summary>
internal class AgentChatCommand : CommandBase
{
    private readonly IAgentChatService _agentChatService;
    private readonly Option<string> _agentIdOption;
    private readonly Option<string> _projectNameOption;
    private readonly Option<bool> _fullLogOption;

    public AgentChatCommand(ISessionManager sessionManager, IAgentChatService agentChatService)
        : base("chat", "Start a new chat with an agent", sessionManager)
    {
        _agentChatService = agentChatService;

        _agentIdOption = new Option<string>("--agent-id")
        {
            Description = "The id of the agent to use in the chat",
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

        _fullLogOption = new Option<bool>("--full-log")
        {
            Description = "Enable full logging for the chat session",
            DefaultValueFactory = _ => false
        };
        _fullLogOption.Aliases.Add("-fl");
        this.Options.Add(_fullLogOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var agentId = parseResult.GetValue(_agentIdOption);
        var projectName = parseResult.GetValue(_projectNameOption);
        var fullLog = parseResult.GetValue(_fullLogOption);

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
            ConsoleUtility.WriteLine("Error: Agent chat requires Identity authentication.", ConsoleColor.Red);
            return;
        }

        // 2. Initialize the agent chat service
        ConsoleUtility.WriteLine("Connecting to Microsoft Foundry", ConsoleColor.Green);

        var conversationId = await _agentChatService.InitializeAsync(endpoint, agentId!, projectName!, cancellationToken)
            .WithLoadingIndicator(
                message: "Creating conversation",
                style: LoadingIndicator.Style.Spinner,
                completionMessage: "Done",
                showTimeTaken: true);

        ConsoleUtility.WriteLine($"Conversation {conversationId} started with agent '{agentId}'. Type 'exit' or 'quit' to stop.", ConsoleColor.Green);
        ConsoleUtility.WriteLine(new string('-', 50), ConsoleColor.Green);
        ConsoleUtility.WriteLine();

        // 3. Multi-turn chat loop — only console I/O here
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
                var outputText=new List<string>();
            
                await foreach (var chunk in _agentChatService.SendMessageStreamingAsync(userInput, fullLog, cancellationToken))
                {
                    if (chunk.UpdateType.Equals("StreamingResponseOutputTextDeltaUpdate", StringComparison.OrdinalIgnoreCase))
                    {
                        outputText.Add(chunk.Text);
                    }
                    else if (fullLog && !string.IsNullOrWhiteSpace(chunk.Text))
                    {
                        ConsoleUtility.WriteLine($"[{chunk.UpdateType}] {chunk.Text}", ConsoleColor.Magenta);
                    }
                }

                ConsoleUtility.Write("Assistant: ", ConsoleColor.Yellow);
                outputText.ForEach(text => ConsoleUtility.Write(text, ConsoleColor.Yellow));
                ConsoleUtility.WriteLine();

            }
            catch (RequestFailedException ex)
            {
                ConsoleUtility.WriteLine($"Error: {ex.Message} (Status: {ex.Status})", ConsoleColor.Red);
            }
        }
    }
}