using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Entities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Set;

/// <summary>
/// Represents the main 'set' command that provides subcommands for configuring session settings.
/// This command serves as a container for various configuration options including credentials, model selection, and language preferences.
/// </summary>
internal class SetCommand : CommandBase
{
    private readonly Option<string> _foundryEndpointOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCommand"/> class with the specified session manager.
    /// </summary>
    /// <param name="sessionManager">The session manager instance used to manage configuration settings across all subcommands.</param>
    public SetCommand(ISessionManager sessionManager) :
        base("set", "Configure session settings", sessionManager)
    {
        this.Subcommands.Add(new SetShowCommand(sessionManager));
        this.Subcommands.Add(new SetClearCommand(sessionManager));

        _foundryEndpointOption = new Option<string>(name: "--endpoint")
        {
            Description = "The Foundry service endpoint",
            Required = false,
        };
        _foundryEndpointOption.Aliases.Add("-e");
        this.Options.Add(_foundryEndpointOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        await this._sessionManager.LoadSettingsAsync();

        if (parseResult.TryGetValue(_foundryEndpointOption, out var endpointValue))
        {
            if (!string.IsNullOrWhiteSpace(endpointValue) &&
            !Uri.TryCreate(endpointValue, UriKind.Absolute, out _))
            {
                ConsoleUtility.WriteLine($"The endpoint '{endpointValue}' is not a valid URL.", ConsoleColor.Red);
                return;
            }
            this._sessionManager.SetEndpoint(endpointValue);
        }

        await this._sessionManager.SaveSettingsAsync();

        ConsoleUtility.WriteLine("Settings updated successfully.", ConsoleColor.Green);
    }
}
