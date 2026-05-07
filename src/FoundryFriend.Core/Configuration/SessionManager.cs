using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using FoundryFriend.Core.Security;
using System.Text;
using System.Text.Json;

namespace FoundryFriend.Core.Configuration;

/// <summary>
/// Manages application session settings and encrypted secrets, providing persistence 
/// through JSON configuration files and encrypted data storage.
/// </summary>
/// <remarks>
/// The SessionManager class handles the persistence of application settings and secrets:
/// <list type="bullet">
/// <item><description>Regular settings are stored as JSON in the user's profile directory</description></item>
/// <item><description>Secrets are encrypted using the platform's data protection API and stored separately</description></item>
/// <item><description>Configuration files are located in ~/.foundryfriendly/</description></item>
/// <item><description>Provides thread-safe access to current session settings</description></item>
/// </list>
/// </remarks>
public class SessionManager : ISessionManager
{
    private const string ApplicationSettingsFolder = ".foundryfriendly";

    /// <summary>
    /// The directory path where configuration files are stored.
    /// </summary>
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ApplicationSettingsFolder
    );

    /// <summary>
    /// The file path for storing non-sensitive settings in JSON format.
    /// </summary>
    private static readonly string ConfigFile = Path.Combine(ConfigDirectory, "settings.json");

    /// <summary>
    /// The file path for storing encrypted secrets.
    /// </summary>
    private static readonly string SecretsFile = Path.Combine(ConfigDirectory, "secrets.dat");

    /// <summary>
    /// The protected data provider used for encrypting and decrypting sensitive data.
    /// </summary>
    private readonly IProtectedDataProvider _protectedDataProvider;

    /// <summary>
    /// The current session settings instance.
    /// </summary>
    private SessionSettings _currentSettings = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="SessionManager"/> class using the default protected data provider.
    /// </summary>
    public SessionManager() : this(ProtectedDataProviderFactory.Create())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionManager"/> class with the specified protected data provider.
    /// </summary>
    /// <param name="protectedDataProvider">The protected data provider for encrypting and decrypting secrets.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="protectedDataProvider"/> is null.</exception>
    public SessionManager(IProtectedDataProvider protectedDataProvider)
    {
        _protectedDataProvider = protectedDataProvider ?? throw new ArgumentNullException(nameof(protectedDataProvider));
    }

    /// <summary>
    /// Asynchronously loads settings from configuration files, including both regular settings and encrypted secrets.
    /// If loading fails, creates a new default settings instance and logs a warning.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadSettingsAsync()
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);

            // Load regular settings
            if (File.Exists(ConfigFile))
            {
                var json = await File.ReadAllTextAsync(ConfigFile);
                _currentSettings = JsonSerializer.Deserialize<SessionSettings>(json) ?? new();
            }

            // Load encrypted secrets
            if (File.Exists(SecretsFile))
            {
                var encryptedData = await File.ReadAllBytesAsync(SecretsFile);
                var decryptedData = await _protectedDataProvider.UnprotectAsync(encryptedData);
                var secretsJson = Encoding.UTF8.GetString(decryptedData);
                _currentSettings.Secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(secretsJson) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load settings: {ex.Message}");
            _currentSettings = new SessionSettings();
        }
    }

    /// <summary>
    /// Asynchronously saves the current settings to configuration files, including both regular settings and encrypted secrets.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveSettingsAsync()
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);

            // Save regular settings
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(ConfigFile, json);

            // Save encrypted secrets
            if (_currentSettings.Secrets.Any())
            {
                var secretsJson = JsonSerializer.Serialize(_currentSettings.Secrets);
                var secretsBytes = Encoding.UTF8.GetBytes(secretsJson);
                var encryptedData = await _protectedDataProvider.ProtectAsync(secretsBytes);
                await File.WriteAllBytesAsync(SecretsFile, encryptedData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears all settings and secrets from both memory and persistent storage.
    /// </summary>
    /// <param name="deleteFiles">If true, deletes the configuration files from disk. If false, only clears in-memory settings.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearAllSettingsAsync(bool deleteFiles = true)
    {
        try
        {
            // Clear in-memory settings
            _currentSettings = new SessionSettings();

            if (deleteFiles)
            {
                // Delete configuration files if they exist
                if (File.Exists(ConfigFile))
                {
                    File.Delete(ConfigFile);
                }

                if (File.Exists(SecretsFile))
                {
                    File.Delete(SecretsFile);
                }

                // Optionally remove the entire configuration directory if it's empty
                if (Directory.Exists(ConfigDirectory) && !Directory.EnumerateFileSystemEntries(ConfigDirectory).Any())
                {
                    Directory.Delete(ConfigDirectory);
                }
            }
            else
            {
                // If not deleting files, save the empty settings to overwrite existing files
                await SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets a custom setting value for the specified key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value.</param>
    public void SetSetting(string key, string value)
    {
        _currentSettings.CustomSettings[key] = value;
    }

    /// <summary>
    /// Sets a secret value for the specified key. Secrets are encrypted when persisted.
    /// </summary>
    /// <param name="key">The secret key.</param>
    /// <param name="value">The secret value.</param>
    public void SetSecret(string key, string value)
    {
        _currentSettings.Secrets[key] = value;
    }

    /// <summary>
    /// Gets the custom setting value for the specified key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value if found; otherwise, null.</returns>
    public string? GetSetting(string key)
    {
        return _currentSettings.CustomSettings.GetValueOrDefault(key);
    }

    /// <summary>
    /// Gets the secret value for the specified key.
    /// </summary>
    /// <param name="key">The secret key.</param>
    /// <returns>The secret value if found; otherwise, null.</returns>
    public string? GetSecret(string key)
    {
        return _currentSettings.Secrets.GetValueOrDefault(key);
    }


    /// <summary>
    /// Sets the authentication mode for the current session.
    /// </summary>
    /// <param name="mode">The <see cref="AuthenticationMode"/> value to persist as a custom setting.</param>
    public void SetAuthenticationMode(AuthenticationMode mode)
    {
        this._currentSettings.AuthenticationMode = mode;
    }

    /// <summary>
    /// Gets the authentication mode for the current session.
    /// </summary>
    /// <returns>
    /// The <see cref="AuthenticationMode"/> value stored in the custom settings if present and valid;
    /// otherwise, <see cref="AuthenticationMode.Identity"/> as the default.
    /// </returns>
    public AuthenticationMode GetAuthenticationMode()
    {
        return _currentSettings.AuthenticationMode;
    }

    public void SetEndpoint(string? endpoint)
    {
        this._currentSettings.ServiceEndopoint = endpoint;
    }

    public string? GetEndpoint()
    {
        return this._currentSettings.ServiceEndopoint;
    }


    public void SetAccessKey(string? accessKey)
    {
        SetSecret("accessKey", accessKey);
    }


    public string? GetAccessKey()
    {
        return GetSecret("accessKey");
    }
}