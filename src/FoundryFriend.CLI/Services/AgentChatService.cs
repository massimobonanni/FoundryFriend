using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using FoundryFriend.Core.Entities;
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
    public async IAsyncEnumerable<StreamingResponseChunk> SendMessageStreamingAsync(
        string userMessage,
        bool extendedLog,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_responsesClient is null)
            throw new InvalidOperationException(
                "AgentChatService has not been initialized. Call InitializeAsync before sending messages.");

        var response = _responsesClient.CreateResponseStreamingAsync(userMessage);

        await foreach (StreamingResponseUpdate? update in response.WithCancellation(cancellationToken))
        {
            // Incremental text token streamed from the model
            if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                yield return new StreamingResponseChunk(textDelta.Delta, nameof(StreamingResponseOutputTextDeltaUpdate));
            }
            // Full text output block has been completed
            else if (update is StreamingResponseOutputTextDoneUpdate textDone)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseOutputTextDoneUpdate));
            }
            // Incremental refusal text token (model declined to answer)
            else if (update is StreamingResponseRefusalDeltaUpdate refusalDelta)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseRefusalDeltaUpdate));
            }
            // Refusal text has been fully emitted
            else if (update is StreamingResponseRefusalDoneUpdate refusalDone)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseRefusalDoneUpdate));
            }
            else if (!extendedLog)
            {
                continue;
            }
            // Incremental function/tool call arguments JSON chunk
            else if (update is StreamingResponseFunctionCallArgumentsDeltaUpdate funcArgsDelta)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseFunctionCallArgumentsDeltaUpdate));
            }
            // Function/tool call arguments have been fully emitted
            else if (update is StreamingResponseFunctionCallArgumentsDoneUpdate funcArgsDone)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseFunctionCallArgumentsDoneUpdate));
            }
            // A new content part has started within an output item
            else if (update is StreamingResponseContentPartAddedUpdate contentPartAdded)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseContentPartAddedUpdate));
            }
            // A content part has been fully completed
            else if (update is StreamingResponseContentPartDoneUpdate contentPartDone)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseContentPartDoneUpdate));
            }
            // A new output item (text block, function call, etc.) was added to the response
            else if (update is StreamingResponseOutputItemAddedUpdate outputItemAdded)
            {
                yield return new StreamingResponseChunk($"Output Item Added : {outputItemAdded.Item.Id}", nameof(StreamingResponseOutputItemAddedUpdate));
            }
            // An output item has been fully completed
            else if (update is StreamingResponseOutputItemDoneUpdate outputItemDone)
            {
                yield return new StreamingResponseChunk($"Output Item Done : {outputItemDone.Item.Id}", nameof(StreamingResponseOutputItemDoneUpdate));
            }
            // The response object was created on the server
            else if (update is StreamingResponseCreatedUpdate created)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseCreatedUpdate));
            }
            // The model is actively processing the request
            else if (update is StreamingResponseInProgressUpdate inProgress)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseInProgressUpdate));
            }
            // The response has been fully completed successfully
            else if (update is StreamingResponseCompletedUpdate completed)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseCompletedUpdate));
            }
            // The response encountered an error and failed
            else if (update is StreamingResponseFailedUpdate failed)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseFailedUpdate));
            }
            // The response stopped early (e.g. max tokens reached)
            else if (update is StreamingResponseIncompleteUpdate incomplete)
            {
                yield return new StreamingResponseChunk(string.Empty, nameof(StreamingResponseIncompleteUpdate));
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
