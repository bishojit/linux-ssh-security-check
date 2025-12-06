using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Checks;

/// <summary>
/// System-level security checks including file permissions and service status.
/// </summary>
internal class SystemChecks
{
    private readonly SystemService _systemService;
    private readonly string _configPath;

    public SystemChecks(SystemService systemService, string configPath)
    {
        _systemService = systemService;
        _configPath = configPath;
    }

    /// <summary>
    /// Checks if SSH service is running.
    /// </summary>
    public async Task<SecurityCheck> CheckSshServiceStatusAsync()
    {
        var isRunning = await _systemService.IsServiceRunningAsync("ssh") || 
                       await _systemService.IsServiceRunningAsync("sshd");

        return new SecurityCheck
        {
            Name = "SSH Service Status",
            Description = "SSH service should be running",
            Status = isRunning ? CheckStatus.Pass : CheckStatus.Fail,
            Details = isRunning ? "SSH service is active" : "SSH service is not running",
            Recommendation = "Start SSH service: sudo systemctl start ssh"
        };
    }

    /// <summary>
    /// Checks SSH configuration file permissions.
    /// </summary>
    public SecurityCheck CheckConfigFilePermissions()
    {
        try
        {
            var fileInfo = new FileInfo(_configPath);
            
            // Only check Unix file mode on Linux
            if (OperatingSystem.IsLinux())
            {
                var unixFileMode = File.GetUnixFileMode(_configPath);
                
                // Config file should be readable by root only (0600 or 0644)
                var isSecure = !unixFileMode.HasFlag(UnixFileMode.OtherWrite);

                return new SecurityCheck
                {
                    Name = "Config File Permissions",
                    Description = "SSH config file should not be world-writable",
                    Status = isSecure ? CheckStatus.Pass : CheckStatus.Fail,
                    Details = $"File permissions: {Convert.ToString((int)unixFileMode, 8)}",
                    Recommendation = "Run: sudo chmod 644 /etc/ssh/sshd_config"
                };
            }
            
            return new SecurityCheck
            {
                Name = "Config File Permissions",
                Description = "File permission check skipped (not on Linux)",
                Status = CheckStatus.Warning,
                Details = "This check only runs on Linux systems",
                Recommendation = "Deploy to Linux to verify file permissions"
            };
        }
        catch
        {
            return new SecurityCheck
            {
                Name = "Config File Permissions",
                Description = "Unable to check file permissions",
                Status = CheckStatus.Warning,
                Details = "Could not verify file permissions",
                Recommendation = "Manually verify config file permissions"
            };
        }
    }

    /// <summary>
    /// Checks private key file permissions.
    /// </summary>
    public SecurityCheck CheckPrivateKeyPermissions()
    {
        try
        {
            // Only check on Linux
            if (!OperatingSystem.IsLinux())
            {
                return new SecurityCheck
                {
                    Name = "Private Key Permissions",
                    Description = "File permission check skipped (not on Linux)",
                    Status = CheckStatus.Warning,
                    Details = "This check only runs on Linux systems",
                    Recommendation = "Deploy to Linux to verify key file permissions"
                };
            }
            
            var keyFiles = new[] { "/etc/ssh/ssh_host_rsa_key", "/etc/ssh/ssh_host_ed25519_key" };
            var issues = new List<string>();

            foreach (var keyFile in keyFiles)
            {
                if (File.Exists(keyFile))
                {
                    var mode = File.GetUnixFileMode(keyFile);
                    
                    // Private keys should be 0600 (readable/writable by owner only)
                    if (mode.HasFlag(UnixFileMode.GroupRead) || mode.HasFlag(UnixFileMode.GroupWrite) ||
                        mode.HasFlag(UnixFileMode.OtherRead) || mode.HasFlag(UnixFileMode.OtherWrite))
                    {
                        issues.Add(Path.GetFileName(keyFile));
                    }
                }
            }

            return new SecurityCheck
            {
                Name = "Private Key Permissions",
                Description = "SSH private keys should be readable by root only",
                Status = issues.Count == 0 ? CheckStatus.Pass : CheckStatus.Fail,
                Details = issues.Count == 0 ? "Private key permissions are secure" : $"Insecure keys: {string.Join(", ", issues)}",
                Recommendation = "Run: sudo chmod 600 /etc/ssh/ssh_host_*_key"
            };
        }
        catch
        {
            return new SecurityCheck
            {
                Name = "Private Key Permissions",
                Description = "Unable to check private key permissions",
                Status = CheckStatus.Warning,
                Details = "Could not verify private key permissions",
                Recommendation = "Manually verify: ls -la /etc/ssh/ssh_host_*_key"
            };
        }
    }
}
