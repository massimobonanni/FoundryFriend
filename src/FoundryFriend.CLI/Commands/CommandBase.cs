using FoundryFriend.Core.Interfaces;
using System.CommandLine;

namespace FoundryFriend.CLI.Commands;

/// <summary>
/// Provides a base implementation for command classes in the PromptusMaximus console application.
/// This abstract class extends the System.CommandLine.Command class and provides common functionality
/// for managing session state through dependency injection.
/// </summary>
internal abstract class CommandBase : Command
{
    /// <summary>
    /// Gets the session manager instance used to handle session settings, configuration persistence,
    /// and secret management across command operations.
    /// </summary>
    /// <value>An <see cref="ISessionManager"/> implementation that provides access to current session settings and related operations.</value>
    protected readonly ISessionManager _sessionManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandBase"/> class with the specified command metadata and session manager.
    /// </summary>
    /// <param name="name">The name of the command as it will appear in the command-line interface.</param>
    /// <param name="description">A description of what the command does, used for help text and documentation.</param>
    /// <param name="sessionManager">The session manager instance that will be used to handle session-related operations throughout the command's lifecycle.</param>
    public CommandBase(string name, string description, ISessionManager sessionManager) : base(name, description)
    {
        _sessionManager = sessionManager;
    }
}
