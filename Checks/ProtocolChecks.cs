using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Checks;

/// <summary>
/// Protocol and version security checks.
/// </summary>
internal class ProtocolChecks
{
    private readonly ConfigurationParser _parser;

    public ProtocolChecks(ConfigurationParser parser)
    {
        _parser = parser;
    }

    /// <summary>
    /// Checks SSH protocol version.
    /// </summary>
    public SecurityCheck CheckProtocolVersion()
    {
        var hasProtocol2 = _parser.CheckConfigValue("Protocol", "2");
        var pattern = @"^\s*Protocol\s+.*1";
        var hasProtocol1 = _parser.ContainsPattern(pattern);

        return new SecurityCheck
        {
            Name = "SSH Protocol Version",
            Description = "Only SSH Protocol 2 should be used (Protocol 1 is deprecated)",
            Status = !hasProtocol1 ? CheckStatus.Pass : CheckStatus.Fail,
            Details = hasProtocol1 ? "Protocol 1 is enabled (insecure)" : "Protocol 2 is being used",
            Recommendation = "Ensure 'Protocol 2' is set or the line is commented out (default is 2)"
        };
    }
}
