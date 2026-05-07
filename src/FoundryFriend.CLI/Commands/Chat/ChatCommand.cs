using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Chat;

/// <summary>
/// Represents the main 'set' command that provides subcommands for configuring session settings.
/// This command serves as a container for various configuration options including credentials, model selection, and language preferences.
/// </summary>
internal class ChatCommand : CommandBase
{
    private readonly Option<string> _systemMessageOption;
    private readonly Option<string> _modelDeployNameOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCommand"/> class with the specified session manager.
    /// </summary>
    /// <param name="sessionManager">The session manager instance used to manage configuration settings across all subcommands.</param>
    public ChatCommand(ISessionManager sessionManager) :
        base("chat", "Start a chat with a specific model deployment in Foundry", sessionManager)
    {
        _modelDeployNameOption = new Option<string>(name: "--modelDeployment")
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

        await _sessionManager.LoadSettingsAsync();

        // Validate session configuration
        var endpoint = _sessionManager.GetEndpoint();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            ConsoleUtility.WriteLine("Error: endpoint not configured. Use 'set' command to configure it.", ConsoleColor.Red);
            return;
        }

        // Build the inference client based on authentication mode
        ChatCompletionsClient client;
        var authMode = _sessionManager.GetAuthenticationMode();

        if (authMode == AuthenticationMode.Key)
        {
            var accessKey = _sessionManager.GetAccessKey();
            if (string.IsNullOrWhiteSpace(accessKey))
            {
                ConsoleUtility.WriteLine("Error: access key not configured. Use 'set' command to configure it.", ConsoleColor.Red);
                return;
            }
            client = new ChatCompletionsClient(
                new Uri(endpoint),
                new AzureKeyCredential(accessKey));
        }
        else
        {
            client = new ChatCompletionsClient(
                new Uri(endpoint),
                new DefaultAzureCredential());
        }

        // Build the conversation history
        var messages = new List<ChatRequestMessage>();

        if (!string.IsNullOrWhiteSpace(systemMessage))
            messages.Add(new ChatRequestSystemMessage(systemMessage));

        ConsoleUtility.WriteLine($"Chat started with model '{modelName}'. Type 'exit' or 'quit' to stop.", ConsoleColor.Green);
        ConsoleUtility.WriteLine(new string('-', 50), ConsoleColor.Green);

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

            messages.Add(new ChatRequestUserMessage(userInput));

            try
            {
                var options = new ChatCompletionsOptions(messages)
                {
                    Model = modelName
                };

                ConsoleUtility.Write("Assistant: ", ConsoleColor.Yellow);

                // Streaming response
                var streamingResponse = await client.CompleteStreamingAsync(options, cancellationToken);

                var assistantReply = new System.Text.StringBuilder();
                await foreach (var update in streamingResponse.WithCancellation(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(update.ContentUpdate))
                    {
                        ConsoleUtility.Write(update.ContentUpdate, ConsoleColor.Yellow);
                        assistantReply.Append(update.ContentUpdate);
                    }
                }

                ConsoleUtility.WriteLine();

                // Add assistant reply to history for multi-turn context
                messages.Add(new ChatRequestAssistantMessage(assistantReply.ToString()));
            }
            catch (RequestFailedException ex)
            {
                ConsoleUtility.WriteLine($"Error: {ex.Message} (Status: {ex.Status})", ConsoleColor.Red);
            }
        }
    }
}
