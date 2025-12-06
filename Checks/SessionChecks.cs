using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Checks;

/// <summary>
/// Session management security checks.
/// </summary>
internal class SessionChecks
{
    private readonly ConfigurationParser _parser;

    public SessionChecks(ConfigurationParser parser)
    {
        _parser = parser;
    }

    /// <summary>
    /// Checks ClientAliveInterval setting.
    /// </summary>
    public SecurityCheck CheckClientAliveInterval()
    {
        var value = _parser.GetConfigValue("ClientAliveInterval");
        
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
    public SecurityCheck CheckClientAliveCountMax()
    {
        var value = _parser.GetConfigValue("ClientAliveCountMax");
        
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
}
