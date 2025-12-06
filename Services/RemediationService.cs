using SshSecurityCheck.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for automatically remediating security issues in SSH configuration.
/// </summary>
internal class RemediationService
{
    private readonly ConfigurationParser _parser;
    private readonly BackupService _backupService;
    private readonly InteractionService _interactionService;

    public RemediationService(
        ConfigurationParser parser,
        BackupService backupService,
        InteractionService interactionService)
    {
        _parser = parser;
        _backupService = backupService;
        _interactionService = interactionService;
    }

    /// <summary>
    /// Performs interactive remediation of security issues.
    /// </summary>
    /// <param name="configPath">Path to SSH configuration file</param>
    /// <param name="failedChecks">List of failed security checks</param>
    /// <returns>True if remediation was successful</returns>
    public async Task<bool> PerformInteractiveRemediationAsync(string configPath, List<SecurityCheck> failedChecks)
    {
        // Filter checks that can be automatically fixed
        var fixableChecks = failedChecks.Where(c => CanAutoFix(c.Name)).ToList();

        if (fixableChecks.Count == 0)
        {
            _interactionService.DisplayInfo("No automatically fixable issues found.");
            return false;
        }

        // Prompt user to select which issues to fix
        var selectedChecks = _interactionService.PromptSelectChecksToFix(fixableChecks);

        if (selectedChecks.Count == 0)
        {
            _interactionService.DisplayInfo("No issues selected for remediation.");
            return false;
        }

        // Create backup
        _interactionService.DisplayInfo($"Creating backup of {configPath}...");
        string backupPath;
        try
        {
            backupPath = _backupService.CreateBackup(configPath);
            _interactionService.DisplaySuccess($"Backup created: {backupPath}");
        }
        catch (Exception ex)
        {
            _interactionService.DisplayError($"Failed to create backup: {ex.Message}");
            return false;
        }

        // Apply fixes
        try
        {
            _interactionService.DisplayInfo("Applying security fixes...");
            await ApplyFixesAsync(configPath, selectedChecks);
            _interactionService.DisplaySuccess("Security fixes applied successfully!");

            // Display instructions
            Console.WriteLine("\n??????????????????????????????????????????????????????????????????");
            Console.WriteLine("?                  IMPORTANT NEXT STEPS                          ?");
            Console.WriteLine("??????????????????????????????????????????????????????????????????");
            Console.WriteLine("\n1. Test the SSH configuration:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("   sudo sshd -t");
            Console.ResetColor();
            Console.WriteLine("\n2. If test passes, restart SSH service:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("   sudo systemctl restart sshd");
            Console.ResetColor();
            Console.WriteLine("\n3. Keep your current SSH session open!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("   ??  Test login in a NEW terminal before closing this one.");
            Console.ResetColor();
            Console.WriteLine("\n4. If something goes wrong, restore from backup:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"   sudo cp {backupPath} {configPath}");
            Console.WriteLine("   sudo systemctl restart sshd");
            Console.ResetColor();

            return true;
        }
        catch (Exception ex)
        {
            _interactionService.DisplayError($"Failed to apply fixes: {ex.Message}");
            
            // Attempt to restore backup
            try
            {
                _interactionService.DisplayInfo("Attempting to restore from backup...");
                _backupService.RestoreFromBackup(backupPath, configPath);
                _interactionService.DisplaySuccess("Configuration restored from backup.");
            }
            catch (Exception restoreEx)
            {
                _interactionService.DisplayError($"Failed to restore backup: {restoreEx.Message}");
                _interactionService.DisplayError($"Manual restore required: sudo cp {backupPath} {configPath}");
            }

            return false;
        }
    }

    /// <summary>
    /// Applies security fixes to the configuration file.
    /// </summary>
    private async Task ApplyFixesAsync(string configPath, List<SecurityCheck> checks)
    {
        var content = await File.ReadAllTextAsync(configPath);
        var originalContent = content;

        foreach (var check in checks)
        {
            content = ApplyFix(content, check.Name);
        }

        // Only write if changes were made
        if (content != originalContent)
        {
            await File.WriteAllTextAsync(configPath, content);
        }
    }

    /// <summary>
    /// Applies a specific fix to the configuration content.
    /// </summary>
    private string ApplyFix(string content, string checkName)
    {
        return checkName switch
        {
            "Root Login Configuration" => SetConfigParameter(content, "PermitRootLogin", "no"),
            "Password Authentication" => SetConfigParameter(content, "PasswordAuthentication", "no"),
            "Empty Passwords" => SetConfigParameter(content, "PermitEmptyPasswords", "no"),
            "Public Key Authentication" => SetConfigParameter(content, "PubkeyAuthentication", "yes"),
            "Host-Based Authentication" => SetConfigParameter(content, "HostbasedAuthentication", "no"),
            "Challenge-Response Authentication" => SetConfigParameter(content, "ChallengeResponseAuthentication", "no"),
            "PAM Authentication" => SetConfigParameter(content, "UsePAM", "yes"),
            "Maximum Authentication Attempts" => SetConfigParameter(content, "MaxAuthTries", "4"),
            "Login Grace Time" => SetConfigParameter(content, "LoginGraceTime", "60"),
            "Strict Modes" => SetConfigParameter(content, "StrictModes", "yes"),
            "User Environment" => SetConfigParameter(content, "PermitUserEnvironment", "no"),
            "X11 Forwarding" => SetConfigParameter(content, "X11Forwarding", "no"),
            "TCP Forwarding" => SetConfigParameter(content, "AllowTcpForwarding", "no"),
            "Agent Forwarding" => SetConfigParameter(content, "AllowAgentForwarding", "no"),
            "Permit Tunnel" => SetConfigParameter(content, "PermitTunnel", "no"),
            "Ignore Rhosts" => SetConfigParameter(content, "IgnoreRhosts", "yes"),
            "Client Alive Interval" => SetConfigParameter(content, "ClientAliveInterval", "300"),
            "Client Alive Count Max" => SetConfigParameter(content, "ClientAliveCountMax", "2"),
            _ => content
        };
    }

    /// <summary>
    /// Sets or updates a configuration parameter in the SSH config.
    /// </summary>
    private string SetConfigParameter(string content, string parameter, string value)
    {
        // Pattern to match the parameter (commented or uncommented)
        var pattern = $@"^(\s*#?\s*{Regex.Escape(parameter)}\s+).*$";
        var match = Regex.Match(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

        if (match.Success)
        {
            // Replace existing parameter
            var replacement = $"{parameter} {value}";
            content = Regex.Replace(content, pattern, replacement, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }
        else
        {
            // Add new parameter at the end of the file
            // Add a comment explaining the change
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var addition = $"\n# Added by SSH Security Check on {timestamp}\n{parameter} {value}\n";
            content += addition;
        }

        return content;
    }

    /// <summary>
    /// Determines if a security check can be automatically fixed.
    /// </summary>
    private bool CanAutoFix(string checkName)
    {
        var fixableChecks = new[]
        {
            "Root Login Configuration",
            "Password Authentication",
            "Empty Passwords",
            "Public Key Authentication",
            "Host-Based Authentication",
            "Challenge-Response Authentication",
            "PAM Authentication",
            "Maximum Authentication Attempts",
            "Login Grace Time",
            "Strict Modes",
            "User Environment",
            "X11 Forwarding",
            "TCP Forwarding",
            "Agent Forwarding",
            "Permit Tunnel",
            "Ignore Rhosts",
            "Client Alive Interval",
            "Client Alive Count Max"
        };

        return fixableChecks.Contains(checkName);
    }
}
