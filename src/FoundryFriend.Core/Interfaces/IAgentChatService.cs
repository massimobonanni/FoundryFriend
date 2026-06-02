namespace FoundryFriend.Core.Interfaces;

/// <summary>
/// Defines the contract for agent-based chat operations against an Azure AI Foundry agent.
/// Implementations handle client construction, conversation creation, and streaming.
/// </summary>
public interface IAgentChatService
{
    /// <summary>
    /// Initializes the agent chat service by creating the underlying project client
    /// and starting a new conversation with the specified agent.
    /// Must be called before <see cref="SendMessageStreamingAsync"/>.
    /// </summary>
    /// <param name="endpoint">The Azure AI Foundry endpoint URL.</param>
    /// <param name="agentId">The identifier of the agent to chat with.</param>
    /// <param name="projectName">The name of the project in Foundry that contains the agent.</param>
    /// <param name="cancellationToken">A token to cancel the initialization.</param>
    /// <returns>The conversation identifier created for this session.</returns>
    Task<string> InitializeAsync(string endpoint, string agentId, string projectName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sends a user message to the agent and streams the response as an asynchronous sequence of text chunks.
    /// </summary>
    /// <param name="userMessage">The user's input message.</param>
    /// <param name="cancellationToken">A token to cancel the streaming operation.</param>
    /// <returns>An asynchronous enumerable of text chunks forming the agent's response.</returns>
    IAsyncEnumerable<string> SendMessageStreamingAsync(string userMessage, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the service state, clearing the conversation and client references.
    /// </summary>
    void Reset();
}
