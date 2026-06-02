using FoundryFriend.Core.Entities;

namespace FoundryFriend.Core.Interfaces;

/// <summary>
/// Defines the contract for agent administration operations against an Azure AI Foundry project.
/// Implementations handle client construction, endpoint normalization, and CRUD operations on agents.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Initializes the service by creating the underlying project client for the specified project.
    /// Must be called before any agent operations.
    /// </summary>
    /// <param name="endpoint">The Azure AI Foundry endpoint URL.</param>
    /// <param name="projectName">The name of the project in Foundry.</param>
    void Initialize(string endpoint, string projectName);

    /// <summary>
    /// Creates a new agent version in the configured project.
    /// </summary>
    /// <param name="agentName">The identifier for the agent.</param>
    /// <param name="modelDeploymentName">The model deployment to use for the agent.</param>
    /// <param name="instructions">The instructions that define the agent's behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Information about the created agent version.</returns>
    Task<AgentVersionInfo> CreateAgentAsync(string agentName, string modelDeploymentName,
        string instructions, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an agent from the configured project.
    /// </summary>
    /// <param name="agentName">The identifier of the agent to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DeleteAgentAsync(string agentName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all agents in the configured project.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of agent information.</returns>
    IAsyncEnumerable<AgentInfo> ListAgentsAsync(CancellationToken cancellationToken);
}
