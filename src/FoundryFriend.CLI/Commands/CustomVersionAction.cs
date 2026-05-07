using FoundryFriend.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace FoundryFriend.CLI.Commands;

/// <summary>
/// Custom version action that overrides the default version display behavior
/// with a custom message instead of showing actual version information.
/// </summary>
internal class CustomVersionAction : SynchronousCommandLineAction
{
    /// <summary>
    /// Invokes the version action, displaying a custom message instead of version details.
    /// </summary>
    /// <param name="parseResult">The parse result containing the command context.</param>
    /// <returns>Always returns 0 to indicate successful execution.</returns>
    override public int Invoke(ParseResult parseResult)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString();
        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        ConsoleUtility.WriteLine($"Version: {version}", ConsoleColor.Green);
        ConsoleUtility.WriteLine($"FileVersion: {fileVersion}", ConsoleColor.Green);
        return 0;
    }
}
