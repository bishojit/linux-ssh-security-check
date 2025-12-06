using System.Diagnostics;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for checking system services status.
/// </summary>
internal class SystemService
{
    /// <summary>
    /// Checks if a system service is running.
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <returns>True if the service is active</returns>
    public async Task<bool> IsServiceRunningAsync(string serviceName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "systemctl",
                    Arguments = $"is-active {serviceName}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Trim().Equals("active", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
