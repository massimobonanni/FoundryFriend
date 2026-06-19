using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands.Login;

/// <summary>
/// Represents the 'login' command that delegates to <c>az login</c> for Azure authentication.
/// </summary>
internal class LoginCommand : CommandBase
{
    private readonly IAzCliService _azCliService;
    private readonly Option<string> _tenantOption;
    private readonly Option<bool> _deviceCodeOption;

    public LoginCommand(ISessionManager sessionManager, IAzCliService azCliService)
        : base("login", "Log in to Azure using 'az login'", sessionManager)
    {
        _azCliService = azCliService;

        _tenantOption = new Option<string>("--tenant")
        {
            Description = "The Azure AD tenant ID or domain name to log in to",
            Required = false,
        };
        _tenantOption.Aliases.Add("-t");
        this.Options.Add(_tenantOption);

        _deviceCodeOption = new Option<bool>("--device-code")
        {
            Description = "Use device code authentication flow (useful in headless or remote environments)",
            Required = false,
        };
        _deviceCodeOption.Aliases.Add("-dc");
        this.Options.Add(_deviceCodeOption);

        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var args = new List<string>();

        if (parseResult.TryGetValue(_tenantOption, out var tenant) && !string.IsNullOrWhiteSpace(tenant))
            args.AddRange(["--tenant", tenant]);

        if (parseResult.TryGetValue(_deviceCodeOption, out var useDeviceCode) && useDeviceCode)
            args.Add("--use-device-code");

        var azArguments = args.Count > 0
            ? $"login {string.Join(' ', args)}"
            : "login";

        ConsoleUtility.WriteLine($"Running: az {azArguments}", ConsoleColor.Cyan);

        var exitCode = await _azCliService.RunAsync(azArguments, cancellationToken);

        if (exitCode == 0)
            ConsoleUtility.WriteLine("Login successful.", ConsoleColor.Green);
        else
            ConsoleUtility.WriteLine($"Login failed (az exited with code {exitCode}).", ConsoleColor.Red);
    }
}
