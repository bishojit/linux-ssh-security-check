using SshSecurityCheck.Models;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for interacting with users and getting their input.
/// </summary>
internal class InteractionService
{
    /// <summary>
    /// Prompts the user for confirmation with yes/no response.
    /// </summary>
    /// <param name="message">The question to ask</param>
    /// <returns>True if user confirms (yes), false otherwise</returns>
    public bool PromptYesNo(string message)
    {
        while (true)
        {
            Console.Write($"{message} (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();
            
            if (response == "y" || response == "yes")
                return true;
            if (response == "n" || response == "no")
                return false;
                
            Console.WriteLine("Please enter 'y' for yes or 'n' for no.");
        }
    }

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    /// <param name="message">Warning message to display</param>
    public void DisplayWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"??  {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays an info message.
    /// </summary>
    /// <param name="message">Info message to display</param>
    public void DisplayInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"??  {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays a success message.
    /// </summary>
    /// <param name="message">Success message to display</param>
    public void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"? {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays an error message.
    /// </summary>
    /// <param name="message">Error message to display</param>
    public void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"? {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prompts user to select which failed checks they want to fix.
    /// </summary>
    /// <param name="failedChecks">List of failed security checks</param>
    /// <returns>List of checks selected by user for remediation</returns>
    public List<SecurityCheck> PromptSelectChecksToFix(List<SecurityCheck> failedChecks)
    {
        Console.WriteLine("\n??????????????????????????????????????????????????????????????????");
        Console.WriteLine("?           INTERACTIVE SECURITY REMEDIATION                     ?");
        Console.WriteLine("??????????????????????????????????????????????????????????????????\n");

        Console.WriteLine($"Found {failedChecks.Count} security issue(s) that can be automatically fixed.\n");

        var selectedChecks = new List<SecurityCheck>();

        foreach (var check in failedChecks)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"?? {check.Name}");
            Console.ResetColor();
            Console.WriteLine($"?  Issue: {check.Details}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"?  Fix: {check.Recommendation}");
            Console.ResetColor();
            Console.WriteLine("??");

            if (PromptYesNo($"  Do you want to fix this issue?"))
            {
                selectedChecks.Add(check);
            }
            Console.WriteLine();
        }

        return selectedChecks;
    }
}
