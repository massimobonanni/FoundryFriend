namespace FoundryFriend.Core.Entities;

/// <summary>
/// Represents summary information about an agent returned by list operations.
/// </summary>
/// <param name="Id">The unique identifier of the agent.</param>
/// <param name="Name">The display name of the agent.</param>
public record AgentInfo(string Id, string Name);

/// <summary>
/// Represents the result of creating a new agent version.
/// </summary>
/// <param name="Id">The unique identifier of the created agent version.</param>
/// <param name="Name">The display name of the agent.</param>
/// <param name="Version">The version string assigned to this agent version.</param>
public record AgentVersionInfo(string Id, string Name, string Version);
