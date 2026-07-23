using FoundryFriend.Core.Entities;

namespace FoundryFriend.Core.Interfaces;

/// <summary>
/// Defines the contract for chat completion operations against an Azure AI Foundry model deployment.
/// Implementations handle client construction, endpoint normalization, conversation state, and streaming.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Sends a user message to the model and streams the assistant response as an asynchronous sequence of text chunks.
    /// The service manages the conversation history internally, appending both user and assistant messages.
    /// </summary>
    /// <param name="userMessage">The user's input message.</param>
    /// <param name="cancellationToken">A token to cancel the streaming operation.</param>
    /// <returns>An asynchronous enumerable of text chunks forming the assistant's response.</returns>
    IAsyncEnumerable<string> SendMessageStreamingAsync(string userMessage, CancellationToken cancellationToken);

    /// <summary>
    /// Initializes the chat service with the specified configuration, creating the underlying client
    /// and optionally setting a system message for the conversation.
    /// Must be called before <see cref="SendMessageStreamingAsync"/>.
    /// </summary>
    /// <param name="endpoint">The Azure AI Foundry endpoint URL.</param>
    /// <param name="projectName">The name of the project in Foundry.</param>
    /// <param name="modelDeploymentName">The name of the model deployment to target.</param>
    /// <param name="systemMessage">An optional system message to set the assistant's behavior.</param>
    void Initialize(string endpoint, string projectName, string modelDeploymentName, string? systemMessage);

    /// <summary>
    /// Resets the conversation history, clearing all previous messages.
    /// </summary>
    void Reset();
}
