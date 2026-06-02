using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using FoundryFriend.Core.Interfaces;
using OpenAI.Responses;
using System.Runtime.CompilerServices;

#pragma warning disable OPENAI001

namespace FoundryFriend.CLI.Services;

/// <summary>
/// Provides agent-based chat operations against an Azure AI Foundry agent.
/// Manages the underlying <see cref="AIProjectClient"/>, conversation lifecycle, and streaming.
/// </summary>
internal class AgentChatService : IAgentChatService
{
    private ProjectResponsesClient? _responsesClient;
    private string? _conversationId;

    /// <inheritdoc />
    public async Task<string> InitializeAsync(string endpoint, string agentId, string projectName,
        CancellationToken cancellationToken)
    {
        // Normalize endpoint
        if (!endpoint.EndsWith("/"))
            endpoint += "/";
        endpoint += $"api/projects/{projectName}";

        var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

        // Create a conversation for multi-turn chat
        ProjectConversation conversation = await projectClient.ProjectOpenAIClient
            .GetProjectConversationsClient()
            .CreateProjectConversationAsync();

        _conversationId = conversation.Id;

        // Get the responses client wired to this agent and conversation
        _responsesClient = projectClient.ProjectOpenAIClient
            .GetProjectResponsesClientForAgent(
                defaultAgent: agentId,
                defaultConversationId: conversation.Id);

        return conversation.Id;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_responsesClient is null)
            throw new InvalidOperationException(
                "AgentChatService has not been initialized. Call InitializeAsync before sending messages.");

        var response = _responsesClient.CreateResponseStreamingAsync(userMessage);

        await foreach (StreamingResponseUpdate? update in response.WithCancellation(cancellationToken))
        {
            if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                yield return textDelta.Delta;
            }
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        _responsesClient = null;
        _conversationId = null;
    }
}
