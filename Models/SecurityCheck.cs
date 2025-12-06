namespace SshSecurityCheck.Models;

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
