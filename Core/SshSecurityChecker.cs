using SshSecurityCheck.Checks;
using SshSecurityCheck.Models;
using SshSecurityCheck.Services;

namespace SshSecurityCheck.Core;

/// <summary>
/// Main security checker orchestrator that coordinates all security validations.
/// </summary>
internal class SshSecurityChecker
{
    private readonly string _configPath;
    private readonly ConfigurationParser _parser;
    private readonly SystemService _systemService;
    private readonly AuthenticationChecks _authChecks;
    private readonly ProtocolChecks _protocolChecks;
    private readonly AccessControlChecks _accessChecks;
    private readonly NetworkChecks _networkChecks;
    private readonly SessionChecks _sessionChecks;
    private readonly SystemChecks _systemChecks;

    /// <summary>
    /// Initializes a new instance of the <see cref="SshSecurityChecker"/> class.
    /// </summary>
    /// <param name="configPath">Path to the SSH configuration file</param>
    public SshSecurityChecker(string configPath)
    {
        _configPath = configPath;
        _parser = new ConfigurationParser();
        _systemService = new SystemService();
        
        // Initialize all check modules
        _authChecks = new AuthenticationChecks(_parser);
        _protocolChecks = new ProtocolChecks(_parser);
        _accessChecks = new AccessControlChecks(_parser);
        _networkChecks = new NetworkChecks(_parser);
        _sessionChecks = new SessionChecks(_parser);
        _systemChecks = new SystemChecks(_systemService, configPath);
    }

    /// <summary>
    /// Runs all security checks and returns the results.
    /// </summary>
    /// <param name="includeWarnings">Whether to include warning-level checks</param>
    /// <returns>Complete security check results</returns>
    public async Task<SecurityCheckResults> RunAllChecksAsync(bool includeWarnings = false)
    {
        var results = new SecurityCheckResults { ConfigPath = _configPath };

        // Validate prerequisites
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("This application must be run on a Linux system.");
        }

        // Load configuration file
        await _parser.LoadConfigurationAsync(_configPath);

        // Run authentication checks
        results.AddCheck(_authChecks.CheckRootLogin());
        results.AddCheck(_authChecks.CheckPasswordAuthentication());
        results.AddCheck(_authChecks.CheckEmptyPasswords());
        results.AddCheck(_authChecks.CheckPublicKeyAuthentication());
        results.AddCheck(_authChecks.CheckHostbasedAuthentication());
        results.AddCheck(_authChecks.CheckChallengeResponseAuth());
        results.AddCheck(_authChecks.CheckUsePAM());
        results.AddCheck(_authChecks.CheckMaxAuthTries());

        // Run protocol checks
        results.AddCheck(_protocolChecks.CheckProtocolVersion());

        // Run access control checks
        results.AddCheck(_accessChecks.CheckLoginGraceTime());
        results.AddCheck(_accessChecks.CheckStrictModes());
        results.AddCheck(_accessChecks.CheckAllowUsers());
        results.AddCheck(_accessChecks.CheckPermitUserEnvironment());
        results.AddCheck(_accessChecks.CheckIgnoreRhosts());

        // Run network checks
        results.AddCheck(_networkChecks.CheckX11Forwarding());
        results.AddCheck(_networkChecks.CheckTcpForwarding());
        results.AddCheck(_networkChecks.CheckAgentForwarding());
        results.AddCheck(_networkChecks.CheckPermitTunnel());

        // Run session checks
        results.AddCheck(_sessionChecks.CheckClientAliveInterval());
        results.AddCheck(_sessionChecks.CheckClientAliveCountMax());

        // Run system checks
        results.AddCheck(await _systemChecks.CheckSshServiceStatusAsync());
        results.AddCheck(_systemChecks.CheckConfigFilePermissions());
        results.AddCheck(_systemChecks.CheckPrivateKeyPermissions());

        return results;
    }
}
