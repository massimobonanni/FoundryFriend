using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace FoundryFriend.CLI.Services;

/// <summary>
/// Provides chat completion operations against an Azure AI Foundry model deployment.
/// Manages the underlying <see cref="ChatCompletionsClient"/>, conversation history, and streaming.
/// </summary>
internal class ChatService : IChatService
{
    private ChatCompletionsClient? _client;
    private string? _modelDeploymentName;
    private readonly List<ChatRequestMessage> _messages = new();

    /// <inheritdoc />
    public void Initialize(string endpoint, AuthenticationMode authMode, string? accessKey,
        string modelDeploymentName, string? systemMessage)
    {
        _modelDeploymentName = modelDeploymentName;
        _messages.Clear();

        // Normalize endpoint
        if (!endpoint.EndsWith("/"))
            endpoint += "/";
        endpoint += "models";

        // Build client based on authentication mode
        if (authMode == AuthenticationMode.Key)
        {
            _client = new ChatCompletionsClient(
                new Uri(endpoint),
                new AzureKeyCredential(accessKey!));
        }
        else
        {
            _client = new ChatCompletionsClient(
                new Uri(endpoint),
                new DefaultAzureCredential());
        }

        // Set optional system message
        if (!string.IsNullOrWhiteSpace(systemMessage))
            _messages.Add(new ChatRequestSystemMessage(systemMessage));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_client is null || _modelDeploymentName is null)
            throw new InvalidOperationException("ChatService has not been initialized. Call Initialize before sending messages.");

        _messages.Add(new ChatRequestUserMessage(userMessage));

        var options = new ChatCompletionsOptions(_messages)
        {
            Model = _modelDeploymentName
        };

        var assistantReply = new System.Text.StringBuilder();
        var streamingResponse = await _client.CompleteStreamingAsync(options, cancellationToken);

        await foreach (var update in streamingResponse.WithCancellation(cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.ContentUpdate))
            {
                assistantReply.Append(update.ContentUpdate);
                yield return update.ContentUpdate;
            }
        }

        // Add the complete assistant reply to conversation history
        _messages.Add(new ChatRequestAssistantMessage(assistantReply.ToString()));
    }

    /// <inheritdoc />
    public void Reset()
    {
        _messages.Clear();
        _client = null;
        _modelDeploymentName = null;
    }
}
