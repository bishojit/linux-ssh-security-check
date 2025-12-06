namespace SshSecurityCheck.Models;

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
