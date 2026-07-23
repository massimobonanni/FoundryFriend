using Azure;
using Azure.AI.Inference;
using Azure.AI.Projects;
using Azure.Identity;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using OpenAI.Chat;
using System.Runtime.CompilerServices;

namespace FoundryFriend.CLI.Services;

/// <summary>
/// Provides chat completion operations against an Azure AI Foundry model deployment.
/// Manages the underlying <see cref="ChatCompletionsClient"/>, conversation history, and streaming.
/// </summary>
internal class ChatService : IChatService
{
    private ChatClient? _client;
    private readonly List<ChatMessage> _messages = new();

    /// <inheritdoc />
    public void Initialize(string endpoint, string projectName, string modelDeploymentName, string? systemMessage)
    {
        _messages.Clear();

        // Normalize endpoint
        if (!endpoint.EndsWith("/"))
            endpoint += "/";
        endpoint += $"api/projects/{projectName}";

        var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

        _client = projectClient.ProjectOpenAIClient.GetChatClient(modelDeploymentName);

        // Set optional system message
        if (!string.IsNullOrWhiteSpace(systemMessage))
            _messages.Add(new SystemChatMessage(systemMessage));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_client is null)
            throw new InvalidOperationException("ChatService has not been initialized. Call Initialize before sending messages.");

        _messages.Add(new UserChatMessage(userMessage));


        var assistantReply = new System.Text.StringBuilder();
        var streamingResponse = _client.CompleteChatStreamingAsync(_messages, null, cancellationToken);

        await foreach (var update in streamingResponse.WithCancellation(cancellationToken))
        {
            assistantReply.Append(update.ContentUpdate);
            yield return update.ContentUpdate.ToString();
        }

        // Add the complete assistant reply to conversation history
        _messages.Add(new AssistantChatMessage(assistantReply.ToString()));
    }

    /// <inheritdoc />
    public void Reset()
    {
        _messages.Clear();
        _client = null;
    }
}
