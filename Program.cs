using SshSecurityCheck.Core;
using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

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
        var displayService = new DisplayService();
        var argumentParser = new ArgumentParser();
        var reportGenerator = new ReportGenerator();

        // Display application header
        displayService.DisplayHeader();

        // Parse command line arguments
        var options = argumentParser.Parse(args);
        
        if (options.ShowHelp)
        {
            displayService.DisplayHelp();
            return 0;
        }

        var sshConfigPath = options.ConfigPath ?? "/etc/ssh/sshd_config";
        var checker = new SshSecurityChecker(sshConfigPath);

        try
        {
            // Perform all security checks
            var results = await checker.RunAllChecksAsync(options.Verbose);
            
            // Display results
            displayService.DisplayResults(results, options.Verbose);
            
            // Generate report if requested
            if (!string.IsNullOrEmpty(options.OutputFile))
            {
                await reportGenerator.GenerateAsync(results, options.OutputFile);
                displayService.DisplaySuccess($"Report saved to: {options.OutputFile}");
            }

            // Interactive remediation mode
            if (options.FixMode)
            {
                var failedChecks = results.Checks
                    .Where(c => c.Status == CheckStatus.Fail)
                    .ToList();

                if (failedChecks.Any())
                {
                    var interactionService = new InteractionService();
                    var backupService = new BackupService();
                    var parser = new ConfigurationParser();
                    await parser.LoadConfigurationAsync(sshConfigPath);
                    
                    var remediationService = new RemediationService(
                        parser,
                        backupService,
                        interactionService);

                    var remediationSuccess = await remediationService
                        .PerformInteractiveRemediationAsync(sshConfigPath, failedChecks);

                    if (remediationSuccess)
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                        Console.WriteLine("║  Run the security check again to verify all issues are fixed   ║");
                        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                    }
                }
                else
                {
                    displayService.DisplaySuccess("No failed checks to remediate. All security checks passed!");
                }
            }

            return results.AllChecksPassed ? 0 : 1;
        }
        catch (UnauthorizedAccessException)
        {
            displayService.DisplayError("Permission denied. Please run with sudo privileges:");
            Console.WriteLine("   sudo dotnet run");
            return 1;
        }
        catch (Exception ex)
        {
            displayService.DisplayError(ex.Message);
            
            if (options.Verbose)
            {
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            }
            
            return 1;
        }
    }
}
