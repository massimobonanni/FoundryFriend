using FoundryFriend.Core.Entities;
using System.Text.Json.Serialization;

namespace FoundryFriend.Core.Configuration;

/// <summary>
/// Represents configuration settings for a user session, including model selection, language preferences, and custom settings.
/// </summary>
/// <remarks>
/// This class holds all necessary configuration for authenticating and communicating with the service endpoint,
/// as well as storing custom and sensitive session-specific values.
/// </remarks>
public class SessionSettings
{
    /// <summary>
    /// Gets or sets the authentication mode used for the session.
    /// </summary>
    /// <value>An <see cref="Entities.AuthenticationMode"/> value indicating the authentication strategy to use.</value>
    public AuthenticationMode AuthenticationMode { get; set; }

    /// <summary>
    /// Gets or sets the service endpoint URL for the session.
    /// </summary>
    /// <value>A string containing the URL of the target service endpoint, or <c>null</c> if not configured.</value>
    public string? ServiceEndopoint { get; set; }

    /// <summary>
    /// Gets or sets the access key used for authentication with the service endpoint.
    /// </summary>
    /// <value>A string containing the access key, or <c>null</c> if not required or not yet provided.</value>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Gets or sets a collection of custom configuration settings specific to the session.
    /// </summary>
    /// <value>A dictionary containing key-value pairs of custom settings. Initialized as an empty dictionary.</value>
    public Dictionary<string, string> CustomSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets a collection of sensitive configuration values that should not be serialized.
    /// </summary>
    /// <value>A dictionary containing key-value pairs of secret values such as API keys or tokens. This property is excluded from JSON serialization.</value>
    [JsonIgnore]
    public Dictionary<string, string> Secrets { get; set; } = new();
}