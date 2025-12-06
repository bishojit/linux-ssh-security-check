# SSH Security Check - Technical Documentation

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Class Documentation](#class-documentation)
3. [Security Check Details](#security-check-details)
4. [Configuration File Parsing](#configuration-file-parsing)
5. [Extension and Customization](#extension-and-customization)
6. [Testing Guidelines](#testing-guidelines)
7. [Performance Considerations](#performance-considerations)

---

## Architecture Overview

### Component Diagram

```
???????????????????????????????????????????????????????
?                   Program (Main)                     ?
?  - Argument parsing                                  ?
?  - Help display                                      ?
?  - Result formatting                                 ?
?  - Report generation                                 ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?              SshSecurityChecker                      ?
?  - Configuration loading                             ?
?  - Security check execution                          ?
?  - Regex-based parsing                               ?
?  - Service status validation                         ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?           SecurityCheckResults                       ?
?  - Check aggregation                                 ?
?  - Score calculation                                 ?
?  - Statistics tracking                               ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?              SecurityCheck                           ?
?  - Individual check result                           ?
?  - Status (Pass/Fail/Warning)                        ?
?  - Recommendations                                   ?
???????????????????????????????????????????????????????
```

---

## Class Documentation

### Program Class

**Purpose**: Main application entry point and user interface handler.

#### Key Methods

##### `Main(string[] args)`
```csharp
private static async Task<int> Main(string[] args)
```
- **Returns**: Exit code (0 = success, 1 = failure)
- **Responsibilities**:
  - Parse command-line arguments
  - Coordinate security checks
  - Display results
  - Generate reports
  - Handle errors gracefully

##### `ParseArguments(string[] args)`
```csharp
private static CommandLineOptions ParseArguments(string[] args)
```
- **Parameters**: Command-line arguments array
- **Returns**: `CommandLineOptions` object
- **Supported Arguments**:
  - `-h, --help`: Display help
  - `-v, --verbose`: Enable verbose output
  - `-c, --config <path>`: Custom config file
  - `-o, --output <file>`: Report output path

##### `DisplayResults(SecurityCheckResults results, bool verbose)`
```csharp
private static void DisplayResults(SecurityCheckResults results, bool verbose)
```
- **Parameters**:
  - `results`: Aggregated check results
  - `verbose`: Whether to show detailed information
- **Output**: Color-coded console display with:
  - Individual check results
  - Summary statistics
  - Security score
  - Overall status

##### `GenerateReportAsync(SecurityCheckResults results, string outputPath)`
```csharp
private static async Task GenerateReportAsync(SecurityCheckResults results, string outputPath)
```
- **Parameters**:
  - `results`: Check results to export
  - `outputPath`: File path for report
- **Output**: Text-based detailed report file

---

### CommandLineOptions Class

**Purpose**: Data transfer object for command-line configuration.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ShowHelp` | `bool` | Whether to display help |
| `Verbose` | `bool` | Enable detailed output |
| `ConfigPath` | `string?` | Custom SSH config path |
| `OutputFile` | `string?` | Report output file path |

---

### SshSecurityChecker Class

**Purpose**: Core security validation engine.

#### Constructor

```csharp
public SshSecurityChecker(string configPath)
```
- **Parameters**: Path to SSH configuration file
- **Initializes**: Configuration path for subsequent checks

#### Key Methods

##### `RunAllChecksAsync(bool includeWarnings)`
```csharp
public async Task<SecurityCheckResults> RunAllChecksAsync(bool includeWarnings = false)
```
- **Parameters**: Whether to include warning-level checks
- **Returns**: `SecurityCheckResults` with all check outcomes
- **Process**:
  1. Validate platform (Linux only)
  2. Verify config file exists
  3. Load configuration content
  4. Execute all security checks
  5. Aggregate results

##### Configuration Checking Methods

All check methods follow this pattern:

```csharp
private SecurityCheck CheckXXX()
{
    return new SecurityCheck
    {
        Name = "Check Name",
        Description = "What this checks",
        Status = /* Pass/Fail/Warning */,
        Details = "Specific findings",
        Recommendation = "How to fix if failed"
    };
}
```

**Available Check Methods**:
- `CheckRootLogin()` - Validates root login disabled
- `CheckPasswordAuthentication()` - Password auth status
- `CheckEmptyPasswords()` - Empty password prevention
- `CheckPublicKeyAuthentication()` - Key-based auth
- `CheckProtocolVersion()` - SSH protocol version
- `CheckMaxAuthTries()` - Auth attempt limits
- `CheckLoginGraceTime()` - Login timeout
- `CheckStrictModes()` - File permission checking
- `CheckPermitUserEnvironment()` - Environment variables
- `CheckX11Forwarding()` - X11 forwarding status
- `CheckTcpForwarding()` - TCP forwarding
- `CheckAgentForwarding()` - SSH agent forwarding
- `CheckHostbasedAuthentication()` - Host-based auth
- `CheckIgnoreRhosts()` - .rhosts handling
- `CheckChallengeResponseAuth()` - Challenge-response
- `CheckUsePAM()` - PAM integration
- `CheckClientAliveInterval()` - Idle timeout
- `CheckClientAliveCountMax()` - Alive message limit
- `CheckAllowUsers()` - User access control
- `CheckPermitTunnel()` - Tunnel configuration
- `CheckSshServiceStatusAsync()` - Service running
- `CheckConfigFilePermissions()` - Config file perms
- `CheckPrivateKeyPermissions()` - Key file perms

##### Configuration Parsing Methods

```csharp
private bool CheckConfigValue(string parameter, string expectedValue)
```
- **Purpose**: Check if parameter equals expected value
- **Returns**: `true` if match found
- **Algorithm**: Regex pattern matching (case-insensitive)

```csharp
private bool CheckConfigExists(string parameter)
```
- **Purpose**: Check if parameter is defined
- **Returns**: `true` if parameter exists with any value

```csharp
private string? GetConfigValue(string parameter)
```
- **Purpose**: Extract parameter value
- **Returns**: Value string or `null` if not found

##### Service Status Method

```csharp
private static async Task<bool> CheckServiceStatusAsync(string serviceName)
```
- **Purpose**: Check if systemd service is active
- **Uses**: `systemctl is-active <service>`
- **Returns**: `true` if service is active

---

### SecurityCheckResults Class

**Purpose**: Aggregates and analyzes security check outcomes.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ConfigPath` | `string` | SSH config file path |
| `Checks` | `IReadOnlyList<SecurityCheck>` | All check results |
| `TotalChecks` | `int` | Total number of checks |
| `PassedChecks` | `int` | Number of passed checks |
| `FailedChecks` | `int` | Number of failed checks |
| `Warnings` | `int` | Number of warnings |
| `AllChecksPassed` | `bool` | Whether all checks passed |
| `SecurityScore` | `double` | Percentage score (0-100) |

#### Methods

```csharp
public void AddCheck(SecurityCheck check)
```
- **Purpose**: Add a check result to the collection
- **Parameters**: `SecurityCheck` instance

---

### SecurityCheck Class

**Purpose**: Represents a single security check result.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Check name/title |
| `Description` | `string` | What the check validates |
| `Status` | `CheckStatus` | Pass/Fail/Warning |
| `Details` | `string` | Specific findings |
| `Recommendation` | `string` | Fix instructions |
| `Passed` | `bool` | Whether check passed |

---

### CheckStatus Enum

**Purpose**: Represents the outcome of a security check.

#### Values

| Value | Description |
|-------|-------------|
| `Pass` | Check passed successfully |
| `Fail` | Check failed (security issue) |
| `Warning` | Potential issue (not critical) |

---

## Security Check Details

### Authentication Checks

#### Root Login
- **Parameter**: `PermitRootLogin`
- **Secure Value**: `no`
- **Warning Value**: `prohibit-password`
- **Rationale**: Direct root login increases attack surface

#### Password Authentication
- **Parameter**: `PasswordAuthentication`
- **Secure Value**: `no`
- **Rationale**: Key-based auth is more secure than passwords

#### Empty Passwords
- **Parameter**: `PermitEmptyPasswords`
- **Secure Value**: `no`
- **Rationale**: Empty passwords are never acceptable

#### Public Key Authentication
- **Parameter**: `PubkeyAuthentication`
- **Secure Value**: `yes`
- **Rationale**: Enables key-based authentication

### Protocol and Version

#### SSH Protocol
- **Parameter**: `Protocol`
- **Secure Value**: `2` or not specified (defaults to 2)
- **Insecure**: Presence of `Protocol 1`
- **Rationale**: Protocol 1 has known vulnerabilities

### Access Control

#### Max Authentication Attempts
- **Parameter**: `MaxAuthTries`
- **Secure Range**: 1-4
- **Default**: 6
- **Rationale**: Limits brute force attempts

#### Login Grace Time
- **Parameter**: `LoginGraceTime`
- **Recommended**: 60 seconds
- **Rationale**: Reduces exposure to attacks

#### User Access Control
- **Parameters**: `AllowUsers`, `AllowGroups`, `DenyUsers`, `DenyGroups`
- **Rationale**: Explicit access control lists

### Session Security

#### Client Alive Interval
- **Parameter**: `ClientAliveInterval`
- **Recommended**: 300 (5 minutes)
- **Rationale**: Disconnect idle sessions

#### Client Alive Count Max
- **Parameter**: `ClientAliveCountMax`
- **Recommended**: 2-3
- **Rationale**: Number of checks before disconnect

### Forwarding and Tunneling

#### X11 Forwarding
- **Parameter**: `X11Forwarding`
- **Secure Value**: `no`
- **Rationale**: Disable unless required for GUI apps

#### TCP Forwarding
- **Parameter**: `AllowTcpForwarding`
- **Secure Value**: `no`
- **Rationale**: Prevents port forwarding attacks

#### Agent Forwarding
- **Parameter**: `AllowAgentForwarding`
- **Secure Value**: `no`
- **Rationale**: Prevents SSH agent hijacking

#### Tunnel Permissions
- **Parameter**: `PermitTunnel`
- **Secure Value**: `no`
- **Rationale**: Disables tunneling unless needed

---

## Configuration File Parsing

### Regex Patterns

#### Exact Value Match
```regex
^\s*{parameter}\s+{value}\s*(?:#.*)?$
```
- Matches parameter with specific value
- Ignores leading/trailing whitespace
- Supports inline comments

#### Parameter Exists
```regex
^\s*{parameter}\s+\S+
```
- Checks if parameter is defined
- Doesn't validate value

#### Value Extraction
```regex
^\s*{parameter}\s+(\S+)
```
- Captures parameter value
- Returns first match

### Parsing Rules

1. **Case Insensitive**: All parameter names are case-insensitive
2. **Comments**: Lines starting with `#` are ignored
3. **Whitespace**: Leading/trailing spaces ignored
4. **First Match**: If parameter appears multiple times, first uncommented instance wins
5. **Default Values**: If not specified, SSH uses built-in defaults

---

## Extension and Customization

### Adding New Security Checks

1. **Create Check Method** in `SshSecurityChecker`:

```csharp
private SecurityCheck CheckNewParameter()
{
    var isSecure = CheckConfigValue("NewParameter", "secure-value");
    
    return new SecurityCheck
    {
        Name = "New Parameter Check",
        Description = "What this parameter controls",
        Status = isSecure ? CheckStatus.Pass : CheckStatus.Fail,
        Details = isSecure ? "Secure setting detected" : "Insecure configuration",
        Recommendation = "Set 'NewParameter secure-value' in sshd_config"
    };
}
```

2. **Add to RunAllChecksAsync**:

```csharp
results.AddCheck(CheckNewParameter());
```

### Custom Output Formatters

Implement custom display logic:

```csharp
private static void DisplayResultsJson(SecurityCheckResults results)
{
    var json = JsonSerializer.Serialize(results, new JsonSerializerOptions 
    { 
        WriteIndented = true 
    });
    Console.WriteLine(json);
}
```

### Integration with CI/CD

Return appropriate exit codes:

```bash
#!/bin/bash
sudo dotnet run --output /var/reports/ssh-audit.txt

if [ $? -eq 0 ]; then
    echo "SSH security check passed"
    exit 0
else
    echo "SSH security check failed"
    exit 1
fi
```

---

## Testing Guidelines

### Unit Testing

Test individual check methods:

```csharp
[Fact]
public void CheckRootLogin_WhenDisabled_ShouldPass()
{
    var configContent = "PermitRootLogin no\n";
    var checker = new SshSecurityChecker("/mock/path");
    
    // Set private field via reflection or constructor parameter
    var result = checker.CheckRootLogin();
    
    Assert.Equal(CheckStatus.Pass, result.Status);
}
```

### Integration Testing

Test full workflow:

```csharp
[Fact]
public async Task RunAllChecks_WithSecureConfig_ShouldPassAllChecks()
{
    var tempConfig = CreateTemporarySecureConfig();
    var checker = new SshSecurityChecker(tempConfig);
    
    var results = await checker.RunAllChecksAsync();
    
    Assert.True(results.AllChecksPassed);
    Assert.True(results.SecurityScore >= 90);
}
```

### Test Configuration Files

Create sample configs for testing:

```bash
# test-configs/secure.conf
PermitRootLogin no
PasswordAuthentication no
PubkeyAuthentication yes
# ... all secure settings

# test-configs/insecure.conf
PermitRootLogin yes
PasswordAuthentication yes
PermitEmptyPasswords yes
# ... insecure settings
```

---

## Performance Considerations

### Optimization Strategies

1. **Async I/O**: Use async file operations
2. **Lazy Loading**: Load config only when needed
3. **Regex Caching**: Compile regex patterns once
4. **Parallel Checks**: Run independent checks concurrently

### Memory Usage

- **Config File**: Loaded once, stored in memory (~10-50 KB)
- **Results**: Minimal overhead (~1 KB per check)
- **Total**: < 1 MB for typical execution

### Execution Time

Typical performance metrics:
- Config file reading: < 10ms
- Security checks: 50-100ms
- Service status checks: 100-200ms
- **Total**: < 500ms

### Scalability

For multiple servers:

```csharp
public class MultiServerChecker
{
    public async Task<Dictionary<string, SecurityCheckResults>> 
        CheckMultipleServersAsync(List<string> serverHosts)
    {
        var tasks = serverHosts.Select(host => 
            CheckRemoteServerAsync(host));
        
        var results = await Task.WhenAll(tasks);
        
        return serverHosts.Zip(results)
            .ToDictionary(x => x.First, x => x.Second);
    }
}
```

---

## Appendix: SSH Configuration Reference

### Critical Parameters

| Parameter | Default | Secure Setting | Impact |
|-----------|---------|----------------|---------|
| PermitRootLogin | yes | no | High |
| PasswordAuthentication | yes | no | High |
| PubkeyAuthentication | yes | yes | High |
| PermitEmptyPasswords | no | no | Critical |
| Protocol | 2 | 2 | High |
| X11Forwarding | no | no | Medium |
| MaxAuthTries | 6 | 4 | Medium |
| LoginGraceTime | 120 | 60 | Low |

### Best Practice Checklist

- [ ] Disable root login
- [ ] Use key-based authentication only
- [ ] Set MaxAuthTries to 4 or less
- [ ] Configure ClientAliveInterval
- [ ] Disable unnecessary forwarding
- [ ] Use AllowUsers or AllowGroups
- [ ] Enable StrictModes
- [ ] Disable PermitUserEnvironment
- [ ] Configure firewall (fail2ban recommended)
- [ ] Regular security audits

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**For Application Version**: 1.0.0
