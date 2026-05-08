// FoundryFriend.Core/Configuration/AgentConfiguration.cs

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundryFriend.Core.Configuration;

internal class AgentMcpServerConfiguration
{
    [JsonPropertyName("serverLabel")]
    public string ServerLabel { get; set; } = string.Empty;

    [JsonPropertyName("serverUrl")]
    public string ServerUrl { get; set; } = string.Empty;

    [JsonPropertyName("requireApproval")]
    public string RequireApproval { get; set; } = "always";

    [JsonPropertyName("allowedTools")]
    public List<string>? AllowedTools { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

internal class AgentConfiguration
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("modelDeploymentName")]
    public string ModelDeploymentName { get; set; } = string.Empty;

    [JsonPropertyName("instructions")]
    public JsonElement Instructions { get; set; }

    [JsonPropertyName("mcpServers")]
    public List<AgentMcpServerConfiguration>? McpServers { get; set; }

    public string GetInstructionsAsString() => Instructions.ValueKind switch
    {
        JsonValueKind.Array => string.Join("\n", Instructions.EnumerateArray().Select(e => e.GetString() ?? "")),
        JsonValueKind.String => Instructions.GetString() ?? "",
        _ => string.Empty
    };

    public static async Task<AgentConfiguration?> LoadFromFileAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<AgentConfiguration>(stream, cancellationToken: ct);
    }
}