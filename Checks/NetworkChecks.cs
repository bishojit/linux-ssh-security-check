using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Checks;

/// <summary>
/// Network and forwarding security checks.
/// </summary>
internal class NetworkChecks
{
    private readonly ConfigurationParser _parser;

    public NetworkChecks(ConfigurationParser parser)
    {
        _parser = parser;
    }

    /// <summary>
    /// Checks X11 forwarding configuration.
    /// </summary>
    public SecurityCheck CheckX11Forwarding()
    {
        var isDisabled = _parser.CheckConfigValue("X11Forwarding", "no");

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
    public SecurityCheck CheckTcpForwarding()
    {
        var isDisabled = _parser.CheckConfigValue("AllowTcpForwarding", "no");

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
    public SecurityCheck CheckAgentForwarding()
    {
        var isDisabled = _parser.CheckConfigValue("AllowAgentForwarding", "no");

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
    /// Checks PermitTunnel setting.
    /// </summary>
    public SecurityCheck CheckPermitTunnel()
    {
        var isDisabled = _parser.CheckConfigValue("PermitTunnel", "no");

        return new SecurityCheck
        {
            Name = "Permit Tunnel",
            Description = "Tunneling should be disabled if not needed",
            Status = isDisabled ? CheckStatus.Pass : CheckStatus.Warning,
            Details = isDisabled ? "Tunneling is disabled" : "Tunneling may be enabled",
            Recommendation = "Set 'PermitTunnel no' in sshd_config if not required"
        };
    }
}
