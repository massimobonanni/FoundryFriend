using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace FoundryFriend.CLI.Commands.Set;

/// <summary>
/// Command that displays the current configuration settings for the session.
/// Shows the default model, language preference, and GitHub token status.
/// </summary>
internal class SetShowCommand : CommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetShowCommand"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager used to access current settings and tokens.</param>
    public SetShowCommand(ISessionManager sessionManager) :
        base("show", "Show current settings", sessionManager)
    {
        this.SetAction(CommandHandler);
    }

    /// <summary>
    /// Handles the execution of the show command by displaying current session settings.
    /// Loads the latest settings and outputs the model, language, and GitHub token status to the console.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments (not used in this implementation).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        await this._sessionManager.LoadSettingsAsync();

        var authMode = this._sessionManager.GetAuthenticationMode();
        ConsoleUtility.WriteLine($"Authentication Mode : {authMode}");
        var foundryEndpoint=this._sessionManager.GetEndpoint();
        if (string.IsNullOrEmpty(foundryEndpoint))
        {
            ConsoleUtility.WriteLine("Foundry endpoint : Not Set");
        }
        else
        {
            ConsoleUtility.WriteLine($"Foundry endpoint: {foundryEndpoint}");
        }

        if (authMode == Core.Entities.AuthenticationMode.Key)
        {
            var accessKey= this._sessionManager.GetAccessKey();
            if (string.IsNullOrEmpty(accessKey))
            {
                ConsoleUtility.WriteLine("Access Key : Not Set");
            }
            else
            {
                ConsoleUtility.WriteLine($"Access Key : {accessKey.Mask()}");
            }
        }
        ConsoleUtility.WriteLine();
    }
}
