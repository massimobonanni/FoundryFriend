using Azure;
using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Chat;

/// <summary>
/// Starts an interactive multi-turn chat with a specific model deployment in Azure AI Foundry.
/// The command handler is thin — it parses input, validates configuration, and delegates
/// all chat logic to <see cref="IChatService"/>.
/// </summary>
internal class ChatCommand : CommandBase
{
    private readonly IChatService _chatService;
    private readonly Option<string> _systemMessageOption;
    private readonly Option<string> _modelDeployNameOption;
    private readonly Option<string> _projectNameOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatCommand"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager for configuration access.</param>
    /// <param name="chatService">The chat service that handles model communication.</param>
    public ChatCommand(ISessionManager sessionManager, IChatService chatService) :
        base("chat", "Start a chat with a specific model deployment in Foundry", sessionManager)
    {
        _chatService = chatService;

        _projectNameOption = new Option<string>("--project-name")
        {
            Description = "The name of the project in Foundry contains the model deployment",
            Required = true
        };
        _projectNameOption.Aliases.Add("-p");
        this.Options.Add(_projectNameOption);

        _modelDeployNameOption = new Option<string>(name: "--model-deployment")
        {
            Description = "The model deployment name in Azure Foundry",
            Required = true,
        };
        _modelDeployNameOption.Aliases.Add("-md");
        this.Options.Add(_modelDeployNameOption);

        _systemMessageOption = new Option<string>(name: "--system")
        {
            Description = "The system message of the chat",
            Required = false,
        };
        _systemMessageOption.Aliases.Add("-sm");
        this.Options.Add(_systemMessageOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var modelName = parseResult.GetValue(_modelDeployNameOption);
        var systemMessage = parseResult.GetValue(_systemMessageOption);
        var projectName = parseResult.GetValue(_projectNameOption);

        // 1. Load and validate session configuration
        await _sessionManager.LoadSettingsAsync();

        var endpoint = _sessionManager.GetEndpoint();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            ConsoleUtility.WriteLine("Error: endpoint not configured. Use 'set' command to configure it.", ConsoleColor.Red);
            return;
        }

        // 2. Initialize the chat service
        _chatService.Initialize(endpoint, projectName!, modelName!, systemMessage);

        ConsoleUtility.WriteLine($"Chat started with model '{modelName} in {projectName} project'. Type 'exit' or 'quit' to stop.", ConsoleColor.Green);
        ConsoleUtility.WriteLine(new string('-', 50), ConsoleColor.Green);

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
                ConsoleUtility.Write("Assistant: ", ConsoleColor.Yellow);

                await foreach (var chunk in _chatService.SendMessageStreamingAsync(userInput, cancellationToken))
                {
                    ConsoleUtility.Write(chunk, ConsoleColor.Yellow);
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
