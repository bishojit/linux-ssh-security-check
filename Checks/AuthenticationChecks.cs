using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Checks;

/// <summary>
/// Authentication-related security checks.
/// </summary>
internal class AuthenticationChecks
{
    private readonly ConfigurationParser _parser;

    public AuthenticationChecks(ConfigurationParser parser)
    {
        _parser = parser;
    }

    /// <summary>
    /// Checks if root login is disabled.
    /// </summary>
    public SecurityCheck CheckRootLogin()
    {
        var isSecure = _parser.CheckConfigValue("PermitRootLogin", "no");
        var allowsProhibitPassword = _parser.CheckConfigValue("PermitRootLogin", "prohibit-password");

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
    public SecurityCheck CheckPasswordAuthentication()
    {
        var isSecure = _parser.CheckConfigValue("PasswordAuthentication", "no");

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
    public SecurityCheck CheckEmptyPasswords()
    {
        var isSecure = _parser.CheckConfigValue("PermitEmptyPasswords", "no");

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
    public SecurityCheck CheckPublicKeyAuthentication()
    {
        var isEnabled = _parser.CheckConfigValue("PubkeyAuthentication", "yes");

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
    /// Checks host-based authentication.
    /// </summary>
    public SecurityCheck CheckHostbasedAuthentication()
    {
        var isDisabled = _parser.CheckConfigValue("HostbasedAuthentication", "no");

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
    /// Checks challenge-response authentication.
    /// </summary>
    public SecurityCheck CheckChallengeResponseAuth()
    {
        var isDisabled = _parser.CheckConfigValue("ChallengeResponseAuthentication", "no") ||
                        _parser.CheckConfigValue("KbdInteractiveAuthentication", "no");

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
    public SecurityCheck CheckUsePAM()
    {
        var isEnabled = _parser.CheckConfigValue("UsePAM", "yes");

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
    /// Checks maximum authentication attempts.
    /// </summary>
    public SecurityCheck CheckMaxAuthTries()
    {
        var value = _parser.GetConfigValue("MaxAuthTries");
        
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
}
