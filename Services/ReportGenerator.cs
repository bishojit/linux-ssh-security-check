using SshSecurityCheck.Models;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for generating security reports.
/// </summary>
internal class ReportGenerator
{
    /// <summary>
    /// Generates a detailed security report and saves it to a file.
    /// </summary>
    /// <param name="results">The security check results</param>
    /// <param name="outputPath">Path to save the report</param>
    public async Task GenerateAsync(SecurityCheckResults results, string outputPath)
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
            var statusSymbol = check.Status switch
            {
                CheckStatus.Pass => "[PASS]",
                CheckStatus.Fail => "[FAIL]",
                CheckStatus.Warning => "[WARN]",
                _ => "[????]"
            };
            
            await writer.WriteLineAsync($"{statusSymbol} {check.Name}");
            
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
                await writer.WriteLineAsync($"  >> Recommendation: {check.Recommendation}");
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
        
        var scoreRating = results.SecurityScore switch
        {
            >= 90 => "EXCELLENT - Highly secure configuration",
            >= 70 => "GOOD - Secure with minor improvements needed",
            >= 50 => "FAIR - Several security issues to address",
            _ => "POOR - Critical security vulnerabilities present"
        };
        
        await writer.WriteLineAsync($"  Security Rating: {scoreRating}");
        await writer.WriteLineAsync($"  Overall Status: {(results.AllChecksPassed ? "SECURE" : "NEEDS ATTENTION")}");
    }
}
