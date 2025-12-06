using System.Diagnostics;
using System.Text.RegularExpressions;

/// <summary>
/// SSH Security Check Application for Linux OS
/// This application performs comprehensive security checks on SSH configuration
/// and provides a detailed report of potential security vulnerabilities.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point for the SSH security check application.
    /// </summary>
    /// <returns>
    /// Exit code: 0 if all checks pass, 1 if any check fails or error occurs
    /// </returns>
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== SSH Security Check for Linux OS ===");
        Console.WriteLine("Version: 1.0.0");
        Console.WriteLine("Description: Comprehensive SSH security configuration analyzer\n");

        // Parse command line arguments
        var options = ParseArguments(args);
        
        if (options.ShowHelp)
        {
            DisplayHelp();
            return 0;
        }

        var sshConfigPath = options.ConfigPath ?? "/etc/ssh/sshd_config";
        var checker = new SshSecurityChecker(sshConfigPath);

        try
        {
            // Perform all security checks
            var results = await checker.RunAllChecksAsync(options.Verbose);
            
            // Display results
            DisplayResults(results, options.Verbose);
            
            // Generate report if requested
            if (!string.IsNullOrEmpty(options.OutputFile))
            {
                await GenerateReportAsync(results, options.OutputFile);
                Console.WriteLine($"\nReport saved to: {options.OutputFile}");
            }

            return results.AllChecksPassed ? 0 : 1;
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ Error: Permission denied. Please run with sudo privileges:");
            Console.WriteLine("   sudo dotnet run");
            Console.ResetColor();
            return 1;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.ResetColor();
            
            if (options.Verbose)
            {
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            }
            
            return 1;
        }
    }

    /// <summary>
    /// Parses command line arguments into an options object.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Parsed options</returns>
    private static CommandLineOptions ParseArguments(string[] args)
    {
        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;
                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;
                case "-c":
                case "--config":
                    if (i + 1 < args.Length)
                    {
                        options.ConfigPath = args[++i];
                    }
                    break;
                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFile = args[++i];
                    }
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Displays help information for the application.
    /// </summary>
    private static void DisplayHelp()
    {
        Console.WriteLine("SSH Security Check - Help");
        Console.WriteLine("\nUsage:");
        Console.WriteLine("  sudo dotnet run [options]");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  -h, --help              Show this help message");
        Console.WriteLine("  -v, --verbose           Enable verbose output with recommendations");
        Console.WriteLine("  -c, --config <path>     Specify custom SSH config file path");
        Console.WriteLine("                          (default: /etc/ssh/sshd_config)");
        Console.WriteLine("  -o, --output <file>     Save report to specified file");
        Console.WriteLine("\nExamples:");
        Console.WriteLine("  sudo dotnet run");
        Console.WriteLine("  sudo dotnet run --verbose");
        Console.WriteLine("  sudo dotnet run --config /etc/ssh/sshd_config.d/custom.conf");
        Console.WriteLine("  sudo dotnet run --verbose --output security-report.txt");
    }

    /// <summary>
    /// Displays the security check results in a formatted output.
    /// </summary>
    /// <param name="results">The security check results</param>
    /// <param name="verbose">Whether to display detailed information</param>
    private static void DisplayResults(SecurityCheckResults results, bool verbose)
    {
        Console.WriteLine("\n" + new string('=', 70));
        Console.WriteLine("SECURITY CHECK RESULTS");
        Console.WriteLine(new string('=', 70));

        foreach (var check in results.Checks)
        {
            DisplayCheckResult(check, verbose);
        }

        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"\nSummary:");
        Console.WriteLine($"  Total Checks: {results.TotalChecks}");
        Console.WriteLine($"  Passed: {results.PassedChecks}");
        Console.WriteLine($"  Failed: {results.FailedChecks}");
        Console.WriteLine($"  Warnings: {results.Warnings}");

        var percentage = results.SecurityScore;
        Console.Write($"\n  Security Score: ");
        Console.ForegroundColor = percentage >= 80 ? ConsoleColor.Green :
                                 percentage >= 60 ? ConsoleColor.Yellow : ConsoleColor.Red;
        Console.WriteLine($"{percentage:F1}%");
        Console.ResetColor();

        if (percentage >= 90)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  ✓ Excellent! Your SSH configuration is highly secure.");
        }
        else if (percentage >= 70)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  ⚠ Good, but there's room for improvement.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ❌ Warning! Your SSH configuration has security vulnerabilities.");
        }
        Console.ResetColor();
    }

    /// <summary>
    /// Displays a single security check result.
    /// </summary>
    /// <param name="check">The security check to display</param>
    /// <param name="verbose">Whether to show detailed recommendations</param>
    private static void DisplayCheckResult(SecurityCheck check, bool verbose)
    {
        var statusSymbol = check.Status switch
        {
            CheckStatus.Pass => "✓",
            CheckStatus.Fail => "✗",
            CheckStatus.Warning => "⚠",
            _ => "?"

        };

        var color = check.Status switch
        {
            CheckStatus.Pass => ConsoleColor.Green,
            CheckStatus.Fail => ConsoleColor.Red,
            CheckStatus.Warning => ConsoleColor.Yellow,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = color;
        Console.Write($"[{statusSymbol}] ");
        Console.ResetColor();
        Console.WriteLine($"{check.Name}");

        if (verbose && !string.IsNullOrEmpty(check.Description))
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    Description: {check.Description}");
            Console.ResetColor();
        }

        if (!check.Passed && !string.IsNullOrEmpty(check.Recommendation))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    → Recommendation: {check.Recommendation}");
            Console.ResetColor();
        }

        if (verbose && !string.IsNullOrEmpty(check.Details))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    Details: {check.Details}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Generates a detailed security report and saves it to a file.
    /// </summary>
    /// <param name="results">The security check results</param>
    /// <param name="outputPath">Path to save the report</param>
    private static async Task GenerateReportAsync(SecurityCheckResults results, string outputPath)
    {
        using var writer = new StreamWriter(outputPath);

        await writer.WriteLineAsync("SSH SECURITY CHECK REPORT");
        await writer.WriteLineAsync(new string('=', 70));
        await writer.WriteLineAsync($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        await writer.WriteLineAsync($"Config File: {results.ConfigPath}");
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("SECURITY CHECKS:");
        await writer.WriteLineAsync(new string('-', 70));

        foreach (var check in results.Checks)
        {
            var status = check.Status.ToString().ToUpper();
            await writer.WriteLineAsync($"[{status}] {check.Name}");
            
            if (!string.IsNullOrEmpty(check.Description))
            {
                await writer.WriteLineAsync($"  Description: {check.Description}");
            }
            
            if (!string.IsNullOrEmpty(check.Details))
            {
                await writer.WriteLineAsync($"  Details: {check.Details}");
            }
            
            if (!check.Passed && !string.IsNullOrEmpty(check.Recommendation))
            {
                await writer.WriteLineAsync($"  Recommendation: {check.Recommendation}");
            }
            
            await writer.WriteLineAsync();
        }

        await writer.WriteLineAsync(new string('=', 70));
        await writer.WriteLineAsync("SUMMARY:");
        await writer.WriteLineAsync($"  Total Checks: {results.TotalChecks}");
        await writer.WriteLineAsync($"  Passed: {results.PassedChecks}");
        await writer.WriteLineAsync($"  Failed: {results.FailedChecks}");
        await writer.WriteLineAsync($"  Warnings: {results.Warnings}");
        await writer.WriteLineAsync($"  Security Score: {results.SecurityScore:F1}%");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"  Overall Status: {(results.AllChecksPassed ? "SECURE" : "NEEDS ATTENTION")}");
    }
}

/// <summary>
/// Command line options for the SSH security checker.
/// </summary>
internal class CommandLineOptions
{
    /// <summary>
    /// Gets or sets whether to display help information.
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Gets or sets whether to enable verbose output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// Gets or sets the path to the SSH configuration file.
    /// </summary>
    public string? ConfigPath { get; set; }

    /// <summary>
    /// Gets or sets the output file path for the report.
    /// </summary>
    public string? OutputFile { get; set; }
}

/// <summary>
/// Main security checker class that performs all SSH security validations.
/// </summary>
internal class SshSecurityChecker
{
    private readonly string _configPath;
    private string? _configContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="SshSecurityChecker"/> class.
    /// </summary>
    /// <param name="configPath">Path to the SSH configuration file</param>
    public SshSecurityChecker(string configPath)
    {
        _configPath = configPath;
    }

    /// <summary>
    /// Runs all security checks and returns the results.
    /// </summary>
    /// <param name="includeWarnings">Whether to include warning-level checks</param>
    /// <returns>Complete security check results</returns>
    public async Task<SecurityCheckResults> RunAllChecksAsync(bool includeWarnings = false)
    {
        var results = new SecurityCheckResults { ConfigPath = _configPath };

        // Validate prerequisites
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("This application must be run on a Linux system.");
        }

        if (!File.Exists(_configPath))
        {
            throw new FileNotFoundException($"SSH config file not found at {_configPath}");
        }

        // Load configuration file
        _configContent = await File.ReadAllTextAsync(_configPath);

        // Critical security checks
        results.AddCheck(CheckRootLogin());
        results.AddCheck(CheckPasswordAuthentication());
        results.AddCheck(CheckEmptyPasswords());
        results.AddCheck(CheckPublicKeyAuthentication());
        results.AddCheck(CheckProtocolVersion());
        results.AddCheck(CheckMaxAuthTries());
        results.AddCheck(CheckLoginGraceTime());
        results.AddCheck(CheckStrictModes());
        results.AddCheck(CheckPermitUserEnvironment());
        results.AddCheck(CheckX11Forwarding());
        results.AddCheck(CheckTcpForwarding());
        results.AddCheck(CheckAgentForwarding());
        results.AddCheck(CheckHostbasedAuthentication());
        results.AddCheck(CheckIgnoreRhosts());
        results.AddCheck(CheckChallengeResponseAuth());
        results.AddCheck(CheckUsePAM());
        results.AddCheck(CheckClientAliveInterval());
        results.AddCheck(CheckClientAliveCountMax());
        results.AddCheck(CheckAllowUsers());
        results.AddCheck(CheckPermitTunnel());
        
        // Service status check
        results.AddCheck(await CheckSshServiceStatusAsync());
        
        // File permission checks
        results.AddCheck(CheckConfigFilePermissions());
        results.AddCheck(CheckPrivateKeyPermissions());

        return results;
    }

    /// <summary>
    /// Checks if root login is disabled.
    /// </summary>
    private SecurityCheck CheckRootLogin()
    {
        var isSecure = CheckConfigValue("PermitRootLogin", "no");
        var allowsProhibitPassword = CheckConfigValue("PermitRootLogin", "prohibit-password");

        return new SecurityCheck
        {
            Name = "Root Login Configuration",
            Description = "Root should not be allowed to login directly via SSH",
            Status = isSecure ? CheckStatus.Pass : (allowsProhibitPassword ? CheckStatus.Warning : CheckStatus.Fail),
            Details = isSecure ? "PermitRootLogin is set to 'no'" : 
                     allowsProhibitPassword ? "PermitRootLogin is set to 'prohibit-password'" :
                     "PermitRootLogin is not properly configured",
            Recommendation = "Set 'PermitRootLogin no' in sshd_config"
        };
    }

    /// <summary>
    /// Checks if password authentication is disabled.
    /// </summary>
    private SecurityCheck CheckPasswordAuthentication()
    {
        var isSecure = CheckConfigValue("PasswordAuthentication", "no");

        return new SecurityCheck
        {
            Name = "Password Authentication",
            Description = "Password authentication should be disabled in favor of key-based auth",
            Status = isSecure ? CheckStatus.Pass : CheckStatus.Fail,
            Details = isSecure ? "PasswordAuthentication is disabled" : "Password authentication is enabled",
            Recommendation = "Set 'PasswordAuthentication no' in sshd_config and use SSH keys instead"
        };
    }

    /// <summary>
    /// Checks if empty passwords are prohibited.
    /// </summary>
    private SecurityCheck CheckEmptyPasswords()
    {
        var isSecure = CheckConfigValue("PermitEmptyPasswords", "no");

        return new SecurityCheck
        {
            Name = "Empty Passwords",
            Description = "Empty passwords should never be allowed",
            Status = isSecure ? CheckStatus.Pass : CheckStatus.Fail,
            Details = isSecure ? "Empty passwords are prohibited" : "Empty passwords may be allowed",
            Recommendation = "Set 'PermitEmptyPasswords no' in sshd_config"
        };
    }

    /// <summary>
    /// Checks if public key authentication is enabled.
    /// </summary>
    private SecurityCheck CheckPublicKeyAuthentication()
    {
        var isEnabled = CheckConfigValue("PubkeyAuthentication", "yes");

        return new SecurityCheck
        {
            Name = "Public Key Authentication",
            Description = "SSH key-based authentication should be enabled",
            Status = isEnabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isEnabled ? "Public key authentication is enabled" : "Public key authentication may not be enabled",
            Recommendation = "Set 'PubkeyAuthentication yes' in sshd_config"
        };
    }

    /// <summary>
    /// Checks SSH protocol version.
    /// </summary>
    private SecurityCheck CheckProtocolVersion()
    {
        var hasProtocol2 = CheckConfigValue("Protocol", "2");
        var pattern = @"^\s*Protocol\s+.*1";
        var hasProtocol1 = Regex.IsMatch(_configContent ?? "", pattern, RegexOptions.Multiline);

        return new SecurityCheck
        {
            Name = "SSH Protocol Version",
            Description = "Only SSH Protocol 2 should be used (Protocol 1 is deprecated)",
            Status = !hasProtocol1 ? CheckStatus.Pass : CheckStatus.Fail,
            Details = hasProtocol1 ? "Protocol 1 is enabled (insecure)" : "Protocol 2 is being used",
            Recommendation = "Ensure 'Protocol 2' is set or the line is commented out (default is 2)"
        };
    }

    /// <summary>
    /// Checks maximum authentication attempts.
    /// </summary>
    private SecurityCheck CheckMaxAuthTries()
    {
        var value = GetConfigValue("MaxAuthTries");
        
        if (int.TryParse(value, out int maxTries) && maxTries <= 4 && maxTries > 0)
        {
            return new SecurityCheck
            {
                Name = "Maximum Authentication Attempts",
                Description = "Limit authentication attempts to prevent brute force attacks",
                Status = CheckStatus.Pass,
                Details = $"MaxAuthTries is set to {maxTries}"
            };
        }

        return new SecurityCheck
        {
            Name = "Maximum Authentication Attempts",
            Description = "Limit authentication attempts to prevent brute force attacks",
            Status = CheckStatus.Warning,
            Details = value != null ? $"MaxAuthTries is set to {value}" : "MaxAuthTries is not configured",
            Recommendation = "Set 'MaxAuthTries 4' in sshd_config"
        };
    }

    /// <summary>
    /// Checks login grace time configuration.
    /// </summary>
    private SecurityCheck CheckLoginGraceTime()
    {
        var value = GetConfigValue("LoginGraceTime");
        var isConfigured = !string.IsNullOrEmpty(value);

        return new SecurityCheck
        {
            Name = "Login Grace Time",
            Description = "Time limit for authentication should be set",
            Status = isConfigured ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isConfigured ? $"LoginGraceTime is set to {value}" : "LoginGraceTime uses default value",
            Recommendation = "Set 'LoginGraceTime 60' in sshd_config (60 seconds)"
        };
    }

    /// <summary>
    /// Checks if strict modes are enabled.
    /// </summary>
    private SecurityCheck CheckStrictModes()
    {
        var isEnabled = CheckConfigValue("StrictModes", "yes");

        return new SecurityCheck
        {
            Name = "Strict Modes",
            Description = "SSH should check file permissions before accepting login",
            Status = isEnabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isEnabled ? "StrictModes is enabled" : "StrictModes may not be enabled",
            Recommendation = "Set 'StrictModes yes' in sshd_config"
        };
    }

    /// <summary>
    /// Checks PermitUserEnvironment setting.
    /// </summary>
    private SecurityCheck CheckPermitUserEnvironment()
    {
        var isSecure = CheckConfigValue("PermitUserEnvironment", "no");

        return new SecurityCheck
        {
            Name = "User Environment",
            Description = "User environment variables should not be accepted",
            Status = isSecure ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isSecure ? "PermitUserEnvironment is disabled" : "User environment may be permitted",
            Recommendation = "Set 'PermitUserEnvironment no' in sshd_config"
        };
    }

    /// <summary>
    /// Checks X11 forwarding configuration.
    /// </summary>
    private SecurityCheck CheckX11Forwarding()
    {
        var isDisabled = CheckConfigValue("X11Forwarding", "no");

        return new SecurityCheck
        {
            Name = "X11 Forwarding",
            Description = "X11 forwarding should be disabled unless explicitly needed",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isDisabled ? "X11Forwarding is disabled" : "X11Forwarding may be enabled",
            Recommendation = "Set 'X11Forwarding no' in sshd_config unless X11 is required"
        };
    }

    /// <summary>
    /// Checks TCP forwarding configuration.
    /// </summary>
    private SecurityCheck CheckTcpForwarding()
    {
        var isDisabled = CheckConfigValue("AllowTcpForwarding", "no");

        return new SecurityCheck
        {
            Name = "TCP Forwarding",
            Description = "TCP forwarding should be disabled if not needed",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isDisabled ? "TCP forwarding is disabled" : "TCP forwarding may be enabled",
            Recommendation = "Set 'AllowTcpForwarding no' in sshd_config if not required"
        };
    }

    /// <summary>
    /// Checks agent forwarding configuration.
    /// </summary>
    private SecurityCheck CheckAgentForwarding()
    {
        var isDisabled = CheckConfigValue("AllowAgentForwarding", "no");

        return new SecurityCheck
        {
            Name = "Agent Forwarding",
            Description = "SSH agent forwarding should be disabled if not needed",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isDisabled ? "Agent forwarding is disabled" : "Agent forwarding may be enabled",
            Recommendation = "Set 'AllowAgentForwarding no' in sshd_config if not required"
        };
    }

    /// <summary>
    /// Checks host-based authentication.
    /// </summary>
    private SecurityCheck CheckHostbasedAuthentication()
    {
        var isDisabled = CheckConfigValue("HostbasedAuthentication", "no");

        return new SecurityCheck
        {
            Name = "Host-Based Authentication",
            Description = "Host-based authentication should be disabled",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Fail,
            Details = isDisabled ? "Host-based authentication is disabled" : "Host-based authentication may be enabled",
            Recommendation = "Set 'HostbasedAuthentication no' in sshd_config"
        };
    }

    /// <summary>
    /// Checks IgnoreRhosts setting.
    /// </summary>
    private SecurityCheck CheckIgnoreRhosts()
    {
        var isEnabled = CheckConfigValue("IgnoreRhosts", "yes");

        return new SecurityCheck
        {
            Name = "Ignore Rhosts",
            Description = ".rhosts and .shosts files should be ignored",
            Status = isEnabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isEnabled ? "Rhosts files are ignored" : "Rhosts files may be used",
            Recommendation = "Set 'IgnoreRhosts yes' in sshd_config"
        };
    }

    /// <summary>
    /// Checks challenge-response authentication.
    /// </summary>
    private SecurityCheck CheckChallengeResponseAuth()
    {
        var isDisabled = CheckConfigValue("ChallengeResponseAuthentication", "no") ||
                        CheckConfigValue("KbdInteractiveAuthentication", "no");

        return new SecurityCheck
        {
            Name = "Challenge-Response Authentication",
            Description = "Challenge-response authentication should be disabled",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isDisabled ? "Challenge-response auth is disabled" : "Challenge-response auth may be enabled",
            Recommendation = "Set 'ChallengeResponseAuthentication no' in sshd_config"
        };
    }

    /// <summary>
    /// Checks UsePAM configuration.
    /// </summary>
    private SecurityCheck CheckUsePAM()
    {
        var isEnabled = CheckConfigValue("UsePAM", "yes");

        return new SecurityCheck
        {
            Name = "PAM Authentication",
            Description = "PAM should be enabled for additional security layers",
            Status = isEnabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isEnabled ? "PAM is enabled" : "PAM may not be enabled",
            Recommendation = "Set 'UsePAM yes' in sshd_config for additional security"
        };
    }

    /// <summary>
    /// Checks ClientAliveInterval setting.
    /// </summary>
    private SecurityCheck CheckClientAliveInterval()
    {
        var value = GetConfigValue("ClientAliveInterval");
        
        if (int.TryParse(value, out int interval) && interval > 0 && interval <= 300)
        {
            return new SecurityCheck
            {
                Name = "Client Alive Interval",
                Description = "Idle timeout should be configured",
                Status = CheckStatus.Pass,
                Details = $"ClientAliveInterval is set to {interval} seconds"
            };
        }

        return new SecurityCheck
        {
            Name = "Client Alive Interval",
            Description = "Idle timeout should be configured",
            Status = CheckStatus.Warning,
            Details = value != null ? $"ClientAliveInterval is {value}" : "ClientAliveInterval not configured",
            Recommendation = "Set 'ClientAliveInterval 300' in sshd_config (5 minutes)"
        };
    }

    /// <summary>
    /// Checks ClientAliveCountMax setting.
    /// </summary>
    private SecurityCheck CheckClientAliveCountMax()
    {
        var value = GetConfigValue("ClientAliveCountMax");
        
        if (int.TryParse(value, out int count) && count >= 0 && count <= 3)
        {
            return new SecurityCheck
            {
                Name = "Client Alive Count Max",
                Description = "Maximum client alive messages before disconnect",
                Status = CheckStatus.Pass,
                Details = $"ClientAliveCountMax is set to {count}"
            };
        }

        return new SecurityCheck
        {
            Name = "Client Alive Count Max",
            Description = "Maximum client alive messages before disconnect",
            Status = CheckStatus.Warning,
            Details = value != null ? $"ClientAliveCountMax is {value}" : "ClientAliveCountMax not configured",
            Recommendation = "Set 'ClientAliveCountMax 2' in sshd_config"
        };
    }

    /// <summary>
    /// Checks if AllowUsers or AllowGroups is configured.
    /// </summary>
    private SecurityCheck CheckAllowUsers()
    {
        var hasAllowUsers = CheckConfigExists("AllowUsers");
        var hasAllowGroups = CheckConfigExists("AllowGroups");
        var hasDenyUsers = CheckConfigExists("DenyUsers");
        var hasDenyGroups = CheckConfigExists("DenyGroups");

        var isConfigured = hasAllowUsers || hasAllowGroups || hasDenyUsers || hasDenyGroups;

        return new SecurityCheck
        {
            Name = "User Access Control",
            Description = "Explicitly define which users/groups can access SSH",
            Status = isConfigured ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isConfigured ? "User access restrictions are configured" : "No user access restrictions configured",
            Recommendation = "Consider setting 'AllowUsers' or 'AllowGroups' in sshd_config"
        };
    }

    /// <summary>
    /// Checks PermitTunnel setting.
    /// </summary>
    private SecurityCheck CheckPermitTunnel()
    {
        var isDisabled = CheckConfigValue("PermitTunnel", "no");

        return new SecurityCheck
        {
            Name = "Permit Tunnel",
            Description = "Tunneling should be disabled if not needed",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isDisabled ? "Tunneling is disabled" : "Tunneling may be enabled",
            Recommendation = "Set 'PermitTunnel no' in sshd_config if not required"
        };
    }

    /// <summary>
    /// Checks if SSH service is running.
    /// </summary>
    private async Task<SecurityCheck> CheckSshServiceStatusAsync()
    {
        var isRunning = await CheckServiceStatusAsync("ssh") || await CheckServiceStatusAsync("sshd");

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
    private SecurityCheck CheckConfigFilePermissions()
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
    private SecurityCheck CheckPrivateKeyPermissions()
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

    /// <summary>
    /// Checks if a configuration parameter has a specific value.
    /// </summary>
    /// <param name="parameter">Configuration parameter name</param>
    /// <param name="expectedValue">Expected value</param>
    /// <returns>True if the parameter has the expected value</returns>
    private bool CheckConfigValue(string parameter, string expectedValue)
    {
        if (string.IsNullOrEmpty(_configContent)) return false;

        var pattern = $@"^\s*{Regex.Escape(parameter)}\s+{Regex.Escape(expectedValue)}\s*(?:#.*)?$";
        return Regex.IsMatch(_configContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Checks if a configuration parameter exists.
    /// </summary>
    /// <param name="parameter">Configuration parameter name</param>
    /// <returns>True if the parameter is configured</returns>
    private bool CheckConfigExists(string parameter)
    {
        if (string.IsNullOrEmpty(_configContent)) return false;

        var pattern = $@"^\s*{Regex.Escape(parameter)}\s+\S+";
        return Regex.IsMatch(_configContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Gets the value of a configuration parameter.
    /// </summary>
    /// <param name="parameter">Configuration parameter name</param>
    /// <returns>The parameter value, or null if not found</returns>
    private string? GetConfigValue(string parameter)
    {
        if (string.IsNullOrEmpty(_configContent)) return null;

        var pattern = $@"^\s*{Regex.Escape(parameter)}\s+(\S+)";

        var match = Regex.Match(_configContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Checks if a system service is running.
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <returns>True if the service is active</returns>
    private static async Task<bool> CheckServiceStatusAsync(string serviceName)
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

/// <summary>
/// Represents the results of all security checks.
/// </summary>
internal class SecurityCheckResults
{
    private readonly List<SecurityCheck> _checks = new();

    /// <summary>
    /// Gets or sets the path to the SSH configuration file.
    /// </summary>
    public string ConfigPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of all security checks performed.
    /// </summary>
    public IReadOnlyList<SecurityCheck> Checks => _checks.AsReadOnly();

    /// <summary>
    /// Gets the total number of checks performed.
    /// </summary>
    public int TotalChecks => _checks.Count;

    /// <summary>
    /// Gets the number of passed checks.
    /// </summary>
    public int PassedChecks => _checks.Count(c => c.Status == CheckStatus.Pass);

    /// <summary>
    /// Gets the number of failed checks.
    /// </summary>
    public int FailedChecks => _checks.Count(c => c.Status == CheckStatus.Fail);

    /// <summary>
    /// Gets the number of warnings.
    /// </summary>
    public int Warnings => _checks.Count(c => c.Status == CheckStatus.Warning);

    /// <summary>
    /// Gets whether all critical checks passed.
    /// </summary>
    public bool AllChecksPassed => FailedChecks == 0;

    /// <summary>
    /// Gets the security score as a percentage.
    /// </summary>
    public double SecurityScore => TotalChecks > 0 ? (PassedChecks * 100.0 / TotalChecks) : 0;

    /// <summary>
    /// Adds a security check to the results.
    /// </summary>
    /// <param name="check">The security check to add</param>
    public void AddCheck(SecurityCheck check)
    {
        _checks.Add(check);
    }
}

/// <summary>
/// Represents a single security check and its result.
/// </summary>
internal class SecurityCheck
{
    /// <summary>
    /// Gets or sets the name of the security check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of what this check validates.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the check.
    /// </summary>
    public CheckStatus Status { get; set; }

    /// <summary>
    /// Gets or sets additional details about the check result.
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommendation for fixing failed checks.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Gets whether the check passed.
    /// </summary>
    public bool Passed => Status == CheckStatus.Pass;
}

/// <summary>
/// Represents the status of a security check.
/// </summary>
internal enum CheckStatus
{
    /// <summary>
    /// The check passed successfully.
    /// </summary>
    Pass,

    /// <summary>
    /// The check failed and indicates a security issue.
    /// </summary>
    Fail,

    /// <summary>
    /// The check found a potential issue but not critical.
    /// </summary>
    Warning
}
