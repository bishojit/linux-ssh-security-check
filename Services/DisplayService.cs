using SshSecurityCheck.Models;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for displaying application information and results.
/// </summary>
internal class DisplayService
{
    /// <summary>
    /// Displays the application header.
    /// </summary>
    public void DisplayHeader()
    {
        Console.WriteLine("=== SSH Security Check for Linux OS ===");
        Console.WriteLine("Version: 1.0.0");
        Console.WriteLine("Description: Comprehensive SSH security configuration analyzer\n");
    }

    /// <summary>
    /// Displays help information for the application.
    /// </summary>
    public void DisplayHelp()
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
        Console.WriteLine("  -f, --fix               Enable interactive remediation mode");
        Console.WriteLine("\nExamples:");
        Console.WriteLine("  sudo dotnet run");
        Console.WriteLine("  sudo dotnet run --verbose");
        Console.WriteLine("  sudo dotnet run --config /etc/ssh/sshd_config.d/custom.conf");
        Console.WriteLine("  sudo dotnet run --verbose --output security-report.txt");
        Console.WriteLine("  sudo dotnet run --fix");
        Console.WriteLine("  sudo dotnet run --fix --verbose");
        Console.WriteLine("\nInteractive Remediation (--fix):");
        Console.WriteLine("  This mode will:");
        Console.WriteLine("  1. Identify security issues that can be automatically fixed");
        Console.WriteLine("  2. Prompt you to select which issues to fix");
        Console.WriteLine("  3. Create a backup of your current SSH configuration");
        Console.WriteLine("  4. Apply the selected fixes to your configuration");
        Console.WriteLine("\n  ??  IMPORTANT: Always keep an active SSH session open when");
        Console.WriteLine("                  using --fix mode to prevent lockout.");
    }

    /// <summary>
    /// Displays the security check results in a formatted output.
    /// </summary>
    /// <param name="results">The security check results</param>
    /// <param name="verbose">Whether to display detailed information</param>
    public void DisplayResults(SecurityCheckResults results, bool verbose)
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
            Console.WriteLine("\n  ? Excellent! Your SSH configuration is highly secure.");
        }
        else if (percentage >= 70)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  ? Good, but there's room for improvement.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ? Warning! Your SSH configuration has security vulnerabilities.");
        }
        Console.ResetColor();
    }

    /// <summary>
    /// Displays a single security check result.
    /// </summary>
    /// <param name="check">The security check to display</param>
    /// <param name="verbose">Whether to show detailed recommendations</param>
    private void DisplayCheckResult(SecurityCheck check, bool verbose)
    {
        var statusSymbol = check.Status switch
        {
            CheckStatus.Pass => "?",
            CheckStatus.Fail => "?",
            CheckStatus.Warning => "?",
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
            Console.WriteLine($"    ? Recommendation: {check.Recommendation}");
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
    /// Displays an error message.
    /// </summary>
    /// <param name="message">Error message to display</param>
    public void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n? Error: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays a success message.
    /// </summary>
    /// <param name="message">Success message to display</param>
    public void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n{message}");
        Console.ResetColor();
    }
}
