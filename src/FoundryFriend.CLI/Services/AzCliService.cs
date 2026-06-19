using FoundryFriend.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FoundryFriend.CLI.Services;

/// <summary>
/// Invokes Azure CLI commands as external processes, forwarding stdin/stdout/stderr
/// directly to the terminal so interactive flows (browser launch, device code) work correctly.
/// </summary>
internal class AzCliService : IAzCliService
{
    /// <inheritdoc />
    public async Task<int> RunAsync(string arguments, CancellationToken cancellationToken = default)
    {
        // On Windows, 'az' is a .cmd batch file that cannot be launched directly
        // without the shell when UseShellExecute=false.
        ProcessStartInfo startInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c az {arguments}",
                UseShellExecute = false,
            }
            : new ProcessStartInfo
            {
                FileName = "az",
                Arguments = arguments,
                UseShellExecute = false,
            };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}
