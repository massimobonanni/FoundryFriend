using FoundryFriend.Core.Configuration;
using FoundryFriend.Core.Entities;

namespace FoundryFriend.Core.Interfaces;

/// <summary>
/// Defines the contract for managing user session settings, including configuration persistence and secret management.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Clears all session settings and optionally deletes the associated configuration files.
    /// </summary>
    /// <param name="deleteFiles">If <c>true</c>, deletes the configuration files from disk; otherwise, only clears in-memory settings. Default is <c>true</c>.</param>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    Task ClearAllSettingsAsync(bool deleteFiles = true);

    /// <summary>
    /// Retrieves a secret value by its key from the secure storage.
    /// </summary>
    /// <param name="key">The key identifying the secret value to retrieve.</param>
    /// <returns>The secret value if found; otherwise, <c>null</c>.</returns>
    string? GetSecret(string key);

    /// <summary>
    /// Retrieves a custom setting value by its key.
    /// </summary>
    /// <param name="key">The key identifying the setting value to retrieve.</param>
    /// <returns>The setting value if found; otherwise, <c>null</c>.</returns>
    string? GetSetting(string key);

    /// <summary>
    /// Loads session settings from persistent storage asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    Task LoadSettingsAsync();

    /// <summary>
    /// Saves the current session settings to persistent storage asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveSettingsAsync();

    /// <summary>
    /// Sets the authentication mode for the current session.
    /// </summary>
    /// <param name="mode">The <see cref="AuthenticationMode"/> to apply to the current session.</param>
    void SetAuthenticationMode(AuthenticationMode mode);

    /// <summary>
    /// Gets the current authentication mode for the session.
    /// </summary>
    /// <returns>The current <see cref="AuthenticationMode"/> applied to the session.</returns>
    AuthenticationMode GetAuthenticationMode();

    /// <summary>
    /// Sets the endpoint URL used to connect to the Azure AI Foundry service.
    /// </summary>
    /// <param name="endpoint">The endpoint URL to store, or <c>null</c> to clear the current value.</param>
    void SetEndpoint(string? endpoint);

    /// <summary>
    /// Gets the endpoint URL used to connect to the Azure AI Foundry service.
    /// </summary>
    /// <returns>The stored endpoint URL, or <c>null</c> if not set.</returns>
    string? GetEndpoint();

    /// <summary>
    /// Sets the access key used for authenticating with the Azure AI Foundry service.
    /// </summary>
    /// <param name="accessKey">The access key to store, or <c>null</c> to clear the current value.</param>
    void SetAccessKey(string? accessKey);

    /// <summary>
    /// Gets the access key used for authenticating with the Azure AI Foundry service.
    /// </summary>
    /// <returns>The stored access key, or <c>null</c> if not set.</returns>
    string? GetAccessKey();

    /// <summary>
    /// Sets a secret value in secure storage, identified by the specified key.
    /// </summary>
    /// <param name="key">The key to identify the secret value.</param>
    /// <param name="value">The secret value to store securely.</param>
    void SetSecret(string key, string value);

    /// <summary>
    /// Sets a custom setting value, identified by the specified key.
    /// </summary>
    /// <param name="key">The key to identify the setting.</param>
    /// <param name="value">The setting value to store.</param>
    void SetSetting(string key, string value);
}