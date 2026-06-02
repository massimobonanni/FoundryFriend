using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace FoundryFriend.CLI.Services;

/// <summary>
/// Provides agent administration operations against an Azure AI Foundry project.
/// Manages the underlying <see cref="AIProjectClient"/> and wraps CRUD calls on agents.
/// </summary>
internal class AgentService : IAgentService
{
    private AIProjectClient? _projectClient;

    /// <inheritdoc />
    public void Initialize(string endpoint, string projectName)
    {
        if (!endpoint.EndsWith("/"))
            endpoint += "/";
        endpoint += $"api/projects/{projectName}";

        _projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
    }

    /// <inheritdoc />
    public async Task<AgentVersionInfo> CreateAgentAsync(string agentName, string modelDeploymentName,
        string instructions, CancellationToken cancellationToken)
    {
        EnsureInitialized();

        var agentDefinition = new DeclarativeAgentDefinition(modelDeploymentName)
        {
            Instructions = instructions,
        };

        var agentVersion = await _projectClient!.AgentAdministrationClient.CreateAgentVersionAsync(
            agentName: agentName,
            options: new(agentDefinition),
            cancellationToken: cancellationToken);

        return new AgentVersionInfo(
            agentVersion.Value.Id,
            agentVersion.Value.Name,
            agentVersion.Value.Version);
    }

    /// <inheritdoc />
    public async Task DeleteAgentAsync(string agentName, CancellationToken cancellationToken)
    {
        EnsureInitialized();

        await _projectClient!.AgentAdministrationClient.DeleteAgentAsync(
            agentName: agentName,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<AgentInfo> ListAgentsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        EnsureInitialized();

        var agentList = _projectClient!.AgentAdministrationClient.GetAgentsAsync(
            cancellationToken: cancellationToken);

        await foreach (var agent in agentList.WithCancellation(cancellationToken))
        {
            yield return new AgentInfo(agent.Id, agent.Name);
        }
    }

    private void EnsureInitialized()
    {
        if (_projectClient is null)
            throw new InvalidOperationException(
                "AgentService has not been initialized. Call Initialize before performing agent operations.");
    }
}
