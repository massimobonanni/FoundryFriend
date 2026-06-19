namespace FoundryFriend.Core.Interfaces;

/// <summary>
/// Defines the contract for invoking Azure CLI commands as external processes.
/// </summary>
public interface IAzCliService
{
    /// <summary>
    /// Runs an Azure CLI command with the specified arguments and returns the process exit code.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the <c>az</c> command (e.g. "login --tenant mytenant").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The exit code of the <c>az</c> process. <c>0</c> indicates success.</returns>
    Task<int> RunAsync(string arguments, CancellationToken cancellationToken = default);
}
