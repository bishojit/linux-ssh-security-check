# Development Guide - SSH Security Check

## ?? For Developers

This guide is for developers who want to understand, modify, or extend the SSH Security Check application.

---

## ??? Architecture Deep Dive

### Design Patterns Used

#### 1. Builder Pattern (Implicit)
```csharp
var results = new SecurityCheckResults();
results.AddCheck(CheckRootLogin());
results.AddCheck(CheckPasswordAuth());
// ... building up results
```

#### 2. Strategy Pattern
Each check method implements the same strategy:
```csharp
private SecurityCheck CheckXXX()
{
    var isSecure = CheckConfigValue("Parameter", "value");
    return new SecurityCheck { /* ... */ };
}
```

#### 3. Template Method
```csharp
public async Task<SecurityCheckResults> RunAllChecksAsync()
{
    // Template for all security checks
    ValidatePlatform();
    LoadConfiguration();
    ExecuteChecks();
    AggregateResults();
}
```

---

## ?? Code Organization

### File Structure
```
Program.cs
??? Program class (Entry point)
?   ??? Main()
?   ??? ParseArguments()
?   ??? DisplayHelp()
?   ??? DisplayResults()
?   ??? GenerateReportAsync()
??? CommandLineOptions (DTO)
??? SshSecurityChecker (Core logic)
?   ??? Constructor
?   ??? RunAllChecksAsync()
?   ??? 23+ Check methods
?   ??? Helper methods
??? SecurityCheckResults (Aggregation)
?   ??? Properties
?   ??? AddCheck()
??? SecurityCheck (Data model)
?   ??? Properties
??? CheckStatus (Enum)
```

---

## ?? Adding a New Security Check

### Step-by-Step Guide

#### Step 1: Create the Check Method

Add to `SshSecurityChecker` class:

```csharp
/// <summary>
/// Checks if [your security parameter] is properly configured.
/// </summary>
/// <returns>Security check result</returns>
private SecurityCheck CheckYourParameter()
{
    // Get the value from config
    var value = GetConfigValue("YourParameter");
    
    // Or check for specific value
    var isSecure = CheckConfigValue("YourParameter", "secure-value");
    
    // Create result
    return new SecurityCheck
    {
        Name = "Your Parameter Check",
        Description = "Explanation of what this checks",
        Status = isSecure ? CheckStatus.Pass : CheckStatus.Fail,
        Details = isSecure 
            ? "Parameter is configured securely" 
            : $"Current value: {value}",
        Recommendation = "Set 'YourParameter secure-value' in sshd_config"
    };
}
```

#### Step 2: Register the Check

Add to `RunAllChecksAsync()` method:

```csharp
public async Task<SecurityCheckResults> RunAllChecksAsync(bool includeWarnings = false)
{
    // ... existing code ...
    
    // Add your check
    results.AddCheck(CheckYourParameter());
    
    // ... rest of checks ...
    
    return results;
}
```

#### Step 3: Test the Check

```csharp
[Fact]
public void CheckYourParameter_WhenSecure_ShouldPass()
{
    // Arrange
    var checker = CreateCheckerWithConfig("YourParameter secure-value");
    
    // Act
    var result = checker.CheckYourParameter();
    
    // Assert
    Assert.Equal(CheckStatus.Pass, result.Status);
}
```

#### Step 4: Document the Check

Update documentation:
- Add to README.md security checks list
- Add to TECHNICAL_DOCUMENTATION.md
- Update test coverage

---

## ?? Testing Strategy

### Unit Testing Framework

```csharp
using Xunit;

public class SshSecurityCheckerTests
{
    [Theory]
    [InlineData("PermitRootLogin no", CheckStatus.Pass)]
    [InlineData("PermitRootLogin yes", CheckStatus.Fail)]
    [InlineData("PermitRootLogin prohibit-password", CheckStatus.Warning)]
    public void CheckRootLogin_WithVariousConfigs_ReturnsExpectedStatus(
        string configLine, CheckStatus expectedStatus)
    {
        // Arrange
        var checker = CreateCheckerWithConfig(configLine);
        
        // Act
        var result = checker.CheckRootLogin();
        
        // Assert
        Assert.Equal(expectedStatus, result.Status);
    }
    
    private SshSecurityChecker CreateCheckerWithConfig(string configContent)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configContent);
        return new SshSecurityChecker(tempFile);
    }
}
```

### Integration Testing

```csharp
public class IntegrationTests
{
    [Fact]
    public async Task FullSecurityScan_WithSecureConfig_PassesAllChecks()
    {
        // Arrange
        var configPath = CreateSecureConfig();
        var checker = new SshSecurityChecker(configPath);
        
        // Act
        var results = await checker.RunAllChecksAsync();
        
        // Assert
        Assert.True(results.AllChecksPassed);
        Assert.True(results.SecurityScore >= 90);
        Assert.Equal(0, results.FailedChecks);
    }
    
    private string CreateSecureConfig()
    {
        var config = @"
            PermitRootLogin no
            PasswordAuthentication no
            PubkeyAuthentication yes
            PermitEmptyPasswords no
            # ... all secure settings
        ";
        var path = Path.GetTempFileName();
        File.WriteAllText(path, config);
        return path;
    }
}
```

---

## ?? Code Style Guidelines

### Naming Conventions

```csharp
// Classes: PascalCase
public class SecurityCheck { }

// Methods: PascalCase
private SecurityCheck CheckRootLogin() { }

// Private fields: _camelCase
private readonly string _configPath;

// Properties: PascalCase
public string Name { get; set; }

// Parameters: camelCase
public void AddCheck(SecurityCheck check) { }

// Local variables: camelCase
var isSecure = true;

// Constants: PascalCase
private const int MaxAuthTries = 4;
```

### XML Documentation

All public members must have XML documentation:

```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <param name="paramName">Description of parameter</param>
/// <returns>Description of return value</returns>
/// <exception cref="ExceptionType">When this exception is thrown</exception>
/// <remarks>
/// Additional information or usage notes.
/// </remarks>
/// <example>
/// <code>
/// var result = SomeMethod("example");
/// </code>
/// </example>
public ReturnType SomeMethod(ParamType paramName)
{
    // Implementation
}
```

### Code Organization

```csharp
public class YourClass
{
    // 1. Constants
    private const int DefaultValue = 100;
    
    // 2. Fields
    private readonly string _field;
    
    // 3. Constructors
    public YourClass(string field)
    {
        _field = field;
    }
    
    // 4. Properties
    public string Property { get; set; }
    
    // 5. Public methods
    public void PublicMethod() { }
    
    // 6. Private methods
    private void PrivateMethod() { }
    
    // 7. Nested types
    private class NestedClass { }
}
```

---

## ?? Debugging Tips

### Enable Verbose Logging

Add to check methods:

```csharp
private SecurityCheck CheckYourParameter()
{
    if (_verbose)
    {
        Console.WriteLine($"Checking YourParameter...");
        Console.WriteLine($"Config content length: {_configContent?.Length}");
    }
    
    var isSecure = CheckConfigValue("YourParameter", "value");
    
    if (_verbose)
    {
        Console.WriteLine($"Result: {(isSecure ? "PASS" : "FAIL")}");
    }
    
    return new SecurityCheck { /* ... */ };
}
```

### Debug Configuration Parsing

```csharp
private void DebugConfigParsing()
{
    Console.WriteLine("=== Configuration Debug ===");
    Console.WriteLine($"File: {_configPath}");
    Console.WriteLine($"Exists: {File.Exists(_configPath)}");
    Console.WriteLine($"Content length: {_configContent?.Length ?? 0}");
    Console.WriteLine($"Line count: {_configContent?.Split('\n').Length ?? 0}");
    Console.WriteLine("\nFirst 10 lines:");
    var lines = _configContent?.Split('\n').Take(10);
    foreach (var line in lines ?? Enumerable.Empty<string>())
    {
        Console.WriteLine($"  {line}");
    }
}
```

### Test Regex Patterns

```csharp
[Fact]
public void RegexPattern_MatchesExpectedFormats()
{
    var testCases = new[]
    {
        ("PermitRootLogin no", true),
        ("  PermitRootLogin no  ", true),
        ("PermitRootLogin no # comment", true),
        ("#PermitRootLogin no", false),
        ("PermitRootLogin yes", false)
    };
    
    foreach (var (input, shouldMatch) in testCases)
    {
        var pattern = @"^\s*PermitRootLogin\s+no\s*(?:#.*)?$";
        var matches = Regex.IsMatch(input, pattern, RegexOptions.Multiline);
        Assert.Equal(shouldMatch, matches);
    }
}
```

---

## ?? Performance Optimization

### Async Operations

Always use async for I/O operations:

```csharp
// Good
var content = await File.ReadAllTextAsync(path);
var result = await CheckServiceStatusAsync("ssh");

// Bad
var content = File.ReadAllText(path); // Blocks thread
```

### Regex Compilation

For frequently used patterns:

```csharp
private static readonly Regex ParameterRegex = new(
    @"^\s*(?<param>\w+)\s+(?<value>\S+)",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase
);

private string? GetConfigValue(string parameter)
{
    var matches = ParameterRegex.Matches(_configContent ?? "");
    return matches
        .FirstOrDefault(m => m.Groups["param"].Value.Equals(parameter, 
            StringComparison.OrdinalIgnoreCase))
        ?.Groups["value"].Value;
}
```

### Parallel Checks

For independent checks:

```csharp
public async Task<SecurityCheckResults> RunAllChecksAsync()
{
    var results = new SecurityCheckResults();
    
    // Independent checks can run in parallel
    var checkTasks = new[]
    {
        Task.Run(() => CheckRootLogin()),
        Task.Run(() => CheckPasswordAuth()),
        Task.Run(() => CheckProtocolVersion())
    };
    
    var checkResults = await Task.WhenAll(checkTasks);
    
    foreach (var check in checkResults)
    {
        results.AddCheck(check);
    }
    
    return results;
}
```

---

## ?? Extension Points

### Custom Check Interface

```csharp
public interface ISecurityCheck
{
    string Name { get; }
    Task<SecurityCheck> ExecuteAsync();
}

public class CustomCheck : ISecurityCheck
{
    public string Name => "Custom Security Check";
    
    public async Task<SecurityCheck> ExecuteAsync()
    {
        // Your custom logic
        return new SecurityCheck { /* ... */ };
    }
}
```

### Plugin System

```csharp
public class PluginManager
{
    private readonly List<ISecurityCheck> _plugins = new();
    
    public void LoadPlugins(string pluginDirectory)
    {
        var assemblies = Directory.GetFiles(pluginDirectory, "*.dll")
            .Select(Assembly.LoadFrom);
        
        foreach (var assembly in assemblies)
        {
            var plugins = assembly.GetTypes()
                .Where(t => typeof(ISecurityCheck).IsAssignableFrom(t))
                .Select(t => (ISecurityCheck)Activator.CreateInstance(t)!);
            
            _plugins.AddRange(plugins);
        }
    }
    
    public async Task<List<SecurityCheck>> ExecutePluginsAsync()
    {
        var tasks = _plugins.Select(p => p.ExecuteAsync());
        return (await Task.WhenAll(tasks)).ToList();
    }
}
```

### Custom Output Formatters

```csharp
public interface IOutputFormatter
{
    Task FormatAsync(SecurityCheckResults results, Stream output);
}

public class JsonFormatter : IOutputFormatter
{
    public async Task FormatAsync(SecurityCheckResults results, Stream output)
    {
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await using var writer = new StreamWriter(output);
        await writer.WriteAsync(json);
    }
}

public class XmlFormatter : IOutputFormatter
{
    public async Task FormatAsync(SecurityCheckResults results, Stream output)
    {
        var serializer = new XmlSerializer(typeof(SecurityCheckResults));
        serializer.Serialize(output, results);
        await Task.CompletedTask;
    }
}
```

---

## ?? Metrics and Monitoring

### Performance Metrics

```csharp
public class PerformanceMetrics
{
    public TimeSpan ConfigLoadTime { get; set; }
    public TimeSpan CheckExecutionTime { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int ChecksPerSecond { get; set; }
}

public async Task<(SecurityCheckResults Results, PerformanceMetrics Metrics)> 
    RunWithMetricsAsync()
{
    var stopwatch = Stopwatch.StartNew();
    var metrics = new PerformanceMetrics();
    
    // Load config
    var loadStart = stopwatch.Elapsed;
    await LoadConfigurationAsync();
    metrics.ConfigLoadTime = stopwatch.Elapsed - loadStart;
    
    // Execute checks
    var checkStart = stopwatch.Elapsed;
    var results = await RunAllChecksAsync();
    metrics.CheckExecutionTime = stopwatch.Elapsed - checkStart;
    
    stopwatch.Stop();
    metrics.TotalTime = stopwatch.Elapsed;
    metrics.ChecksPerSecond = (int)(results.TotalChecks / stopwatch.Elapsed.TotalSeconds);
    
    return (results, metrics);
}
```

---

## ?? Common Development Issues

### Issue: Regex Not Matching

**Problem**: Check always fails even with correct config.

**Solution**: Test regex in isolation:

```csharp
var pattern = @"^\s*PermitRootLogin\s+no\s*(?:#.*)?$";
var testString = "PermitRootLogin no";
var matches = Regex.IsMatch(testString, pattern, 
    RegexOptions.Multiline | RegexOptions.IgnoreCase);
Console.WriteLine($"Matches: {matches}");
```

### Issue: File Not Found

**Problem**: Config file path is incorrect.

**Solution**: Add better error handling:

```csharp
if (!File.Exists(_configPath))
{
    throw new FileNotFoundException(
        $"SSH config not found. Checked: {_configPath}\n" +
        $"Common locations:\n" +
        $"  - /etc/ssh/sshd_config\n" +
        $"  - /etc/ssh/sshd_config.d/*.conf");
}
```

### Issue: Permission Denied

**Problem**: Not running with sudo.

**Solution**: Check permissions before reading:

```csharp
try
{
    _ = File.GetUnixFileMode(_configPath);
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Error: This application requires sudo privileges.");
    Console.WriteLine("Run: sudo dotnet run");
    return 1;
}
```

---

## ?? Contributing Guidelines

### Pull Request Process

1. **Fork** the repository
2. **Create** a feature branch
3. **Write** tests for new functionality
4. **Update** documentation
5. **Run** all tests
6. **Submit** pull request

### Code Review Checklist

- [ ] Code follows style guidelines
- [ ] XML documentation added
- [ ] Unit tests included
- [ ] Integration tests pass
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
- [ ] Performance impact assessed

### Commit Message Format

```
type(scope): subject

body

footer
```

**Types**: feat, fix, docs, style, refactor, test, chore

**Example**:
```
feat(checks): add SSH cipher suite validation

- Added CheckCipherSuites() method
- Updated documentation
- Added unit tests

Closes #123
```

---

## ?? Security Considerations

### When Adding Checks

1. **Validate input**: Never trust user input
2. **Escape regex**: Use `Regex.Escape()` for parameters
3. **Handle errors**: Graceful degradation
4. **Document risks**: Explain security implications

### Code Security

```csharp
// Good: Parameterized
private bool CheckConfigValue(string parameter, string value)
{
    var escapedParam = Regex.Escape(parameter);
    var escapedValue = Regex.Escape(value);
    var pattern = $@"^\s*{escapedParam}\s+{escapedValue}\s*(?:#.*)?$";
    return Regex.IsMatch(_configContent ?? "", pattern, 
        RegexOptions.Multiline | RegexOptions.IgnoreCase);
}

// Bad: Direct string interpolation (potential injection)
var pattern = $"^{parameter} {value}$";  // DON'T DO THIS
```

---

## ?? Additional Resources

### .NET Documentation
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/async)
- [Regex Guide](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions)
- [File I/O](https://docs.microsoft.com/en-us/dotnet/standard/io/)

### SSH Security
- [OpenSSH Security](https://www.openssh.com/security.html)
- [CIS Benchmark](https://www.cisecurity.org/benchmark/distribution_independent_linux)
- [NIST Guidelines](https://csrc.nist.gov/)

### Testing
- [xUnit Documentation](https://xunit.net/)
- [Moq Framework](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)

---

**Happy Coding! ??**

For questions or issues, please open a GitHub issue or discussion.
