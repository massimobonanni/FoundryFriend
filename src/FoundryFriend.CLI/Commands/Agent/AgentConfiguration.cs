// FoundryFriend.Core/Configuration/AgentConfiguration.cs

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundryFriend.Core.Configuration;

/// <summary>
/// Represents the configuration for an MCP (Model Context Protocol) server that an agent can use as a tool.
/// </summary>
internal class AgentMcpServerConfiguration
{
    /// <summary>
    /// Gets or sets the label used to identify the MCP server.
    /// </summary>
    [JsonPropertyName("serverLabel")]
    public string ServerLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the MCP server.
    /// </summary>
    [JsonPropertyName("serverUrl")]
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the approval requirement policy for tool calls made to this MCP server. Default is "always".
    /// </summary>
    [JsonPropertyName("requireApproval")]
    public string RequireApproval { get; set; } = "always";

    /// <summary>
    /// Gets or sets the list of tool names allowed from this MCP server, or <c>null</c> to allow all tools.
    /// </summary>
    [JsonPropertyName("allowedTools")]
    public List<string>? AllowedTools { get; set; }

    /// <summary>
    /// Gets or sets the connection identifier used to authenticate with the MCP server, if required.
    /// </summary>
    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// Represents the configuration of an agent as loaded from a JSON configuration file,
/// including its metadata, model deployment, instructions, and any MCP server tools.
/// </summary>
internal class AgentConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier for the agent.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the agent.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the agent.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the model deployment used by the agent.
    /// </summary>
    [JsonPropertyName("modelDeploymentName")]
    public string ModelDeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw instructions for the agent, which may be represented as either a single string
    /// or an array of strings in the source JSON.
    /// </summary>
    [JsonPropertyName("instructions")]
    public JsonElement Instructions { get; set; }

    /// <summary>
    /// Gets or sets the collection of MCP server tools available to the agent, or <c>null</c> if none are configured.
    /// </summary>
    [JsonPropertyName("mcpServers")]
    public List<AgentMcpServerConfiguration>? McpServers { get; set; }

    /// <summary>
    /// Converts the <see cref="Instructions"/> value into a single string, joining array elements with newlines.
    /// </summary>
    /// <returns>The instructions as a single string, or an empty string if not set or in an unsupported format.</returns>
    public string GetInstructionsAsString() => Instructions.ValueKind switch
    {
        JsonValueKind.Array => string.Join("\n", Instructions.EnumerateArray().Select(e => e.GetString() ?? "")),
        JsonValueKind.String => Instructions.GetString() ?? "",
        _ => string.Empty
    };

    /// <summary>
    /// Asynchronously loads and deserializes an <see cref="AgentConfiguration"/> from the specified JSON file.
    /// </summary>
    /// <param name="filePath">The path to the agent configuration JSON file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The deserialized <see cref="AgentConfiguration"/>, or <c>null</c> if deserialization fails.</returns>
    public static async Task<AgentConfiguration?> LoadFromFileAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<AgentConfiguration>(stream, cancellationToken: ct);
    }
}