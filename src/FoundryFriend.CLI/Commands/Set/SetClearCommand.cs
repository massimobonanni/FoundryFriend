using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Set;

/// <summary>
/// Command implementation for clearing all session settings and credentials with user confirmation.
/// </summary>
internal class SetClearCommand : CommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetClearCommand"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager used to clear settings and credentials.</param>
    public SetClearCommand(ISessionManager sessionManager) :
        base("clear", "Clear settings", sessionManager)
    {
        this.SetAction(CommandHandler);
    }

    /// <summary>
    /// Handles the execution of the clear command by prompting for user confirmation
    /// and clearing all settings if confirmed.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        System.Console.WriteLine("Are you sure you want to clear all settings? This action cannot be undone. (yes/no)");
        var confirmation = System.Console.ReadLine();
        if (confirmation?.ToLower() != "yes" || confirmation?.ToLower() != "y")
        {
            ConsoleUtility.WriteLine("Operation cancelled.", ConsoleColor.Red);
            return;
        }

        await this._sessionManager.ClearAllSettingsAsync();

        ConsoleUtility.WriteLine("All settings have been cleared successfully.", ConsoleColor.Green);
    }
}
