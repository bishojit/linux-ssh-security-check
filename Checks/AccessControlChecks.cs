using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Checks;

/// <summary>
/// Access control and session security checks.
/// </summary>
internal class AccessControlChecks
{
    private readonly ConfigurationParser _parser;

    public AccessControlChecks(ConfigurationParser parser)
    {
        _parser = parser;
    }

    /// <summary>
    /// Checks login grace time configuration.
    /// </summary>
    public SecurityCheck CheckLoginGraceTime()
    {
        var value = _parser.GetConfigValue("LoginGraceTime");
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
    public SecurityCheck CheckStrictModes()
    {
        var isEnabled = _parser.CheckConfigValue("StrictModes", "yes");

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
    /// Checks if AllowUsers or AllowGroups is configured.
    /// </summary>
    public SecurityCheck CheckAllowUsers()
    {
        var hasAllowUsers = _parser.CheckConfigExists("AllowUsers");
        var hasAllowGroups = _parser.CheckConfigExists("AllowGroups");
        var hasDenyUsers = _parser.CheckConfigExists("DenyUsers");
        var hasDenyGroups = _parser.CheckConfigExists("DenyGroups");

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
    /// Checks PermitUserEnvironment setting.
    /// </summary>
    public SecurityCheck CheckPermitUserEnvironment()
    {
        var isSecure = _parser.CheckConfigValue("PermitUserEnvironment", "no");

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
    /// Checks IgnoreRhosts setting.
    /// </summary>
    public SecurityCheck CheckIgnoreRhosts()
    {
        var isEnabled = _parser.CheckConfigValue("IgnoreRhosts", "yes");

        return new SecurityCheck
        {
            Name = "Ignore Rhosts",
            Description = ".rhosts and .shosts files should be ignored",
            Status = isEnabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isEnabled ? "Rhosts files are ignored" : "Rhosts files may be used",
            Recommendation = "Set 'IgnoreRhosts yes' in sshd_config"
        };
    }
}
