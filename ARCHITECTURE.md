# Code Architecture - Modular Structure

## Overview

The SSH Security Check application has been refactored into a clean, modular architecture following SOLID principles. The code is now organized into separate, focused components for better maintainability, testability, and extensibility.

## Project Structure

```
linux-ssh-security-check/
??? Program.cs                          # Main entry point (orchestrator)
??? Models/                             # Data models
?   ??? CommandLineOptions.cs          # CLI configuration
?   ??? SecurityCheck.cs                # Security check result model
?   ??? SecurityCheckResults.cs         # Aggregated results
??? Services/                           # Core services
?   ??? ArgumentParser.cs               # Command-line parsing
?   ??? ConfigurationParser.cs          # SSH config file parsing
?   ??? DisplayService.cs               # Console output formatting
?   ??? ReportGenerator.cs              # Report file generation
?   ??? SystemService.cs                # System service interactions
??? Checks/                             # Security check modules
?   ??? AuthenticationChecks.cs         # Authentication security
?   ??? ProtocolChecks.cs               # Protocol version checks
?   ??? AccessControlChecks.cs          # Access control checks
?   ??? NetworkChecks.cs                # Network/forwarding checks
?   ??? SessionChecks.cs                # Session management checks
?   ??? SystemChecks.cs                 # System-level checks
??? Core/                               # Core orchestration
    ??? SshSecurityChecker.cs           # Main security checker
```

## Architecture Layers

### 1. **Entry Point** (`Program.cs`)
- Minimal orchestration code
- Coordinates services
- Handles top-level error handling
- ~70 lines of code

### 2. **Models** (`Models/`)
Data transfer objects with no business logic:
- `CommandLineOptions` - CLI arguments
- `SecurityCheck` - Individual check result
- `CheckStatus` - Enum for check status
- `SecurityCheckResults` - Collection of results

### 3. **Services** (`Services/`)
Reusable, single-responsibility services:

**ArgumentParser**
- Parses command-line arguments
- Returns structured options object

**ConfigurationParser**
- Loads and parses SSH config files
- Provides methods to query configuration
- Uses regex for pattern matching

**DisplayService**
- All console output operations
- Color-coded formatting
- Consistent user interface

**ReportGenerator**
- Generates detailed text reports
- Async file operations
- Structured output format

**SystemService**
- Interacts with system services
- Checks service status via systemctl
- Cross-platform aware

### 4. **Checks** (`Checks/`)
Focused security check modules organized by category:

**AuthenticationChecks**
- Root login checks
- Password authentication
- Key-based authentication
- PAM configuration
- Max auth attempts

**ProtocolChecks**
- SSH protocol version validation

**AccessControlChecks**
- Login grace time
- Strict modes
- User access control
- Environment variables
- Rhosts handling

**NetworkChecks**
- X11 forwarding
- TCP forwarding
- Agent forwarding
- Tunnel permissions

**SessionChecks**
- Client alive interval
- Client alive count max

**SystemChecks**
- Service status
- File permissions
- Key permissions

### 5. **Core** (`Core/`)
**SshSecurityChecker**
- Orchestrates all security checks
- Coordinates check modules
- Aggregates results
- Main business logic coordinator

## Design Principles

### Single Responsibility Principle (SRP)
Each class has one reason to change:
- `ArgumentParser` only handles CLI parsing
- `DisplayService` only handles output
- Each check module focuses on one security aspect

### Open/Closed Principle (OCP)
Easy to extend without modifying existing code:
- Add new check modules without changing orchestrator
- Add new display formats without changing checks

### Dependency Inversion Principle (DIP)
High-level modules depend on abstractions:
- Check modules depend on `ConfigurationParser` interface
- Services are loosely coupled

### Separation of Concerns
Clear boundaries between:
- Data (Models)
- Business logic (Checks)
- Infrastructure (Services)
- Presentation (DisplayService)

## Benefits of Modular Design

### 1. **Maintainability**
- Easy to locate and fix bugs
- Clear code organization
- Self-documenting structure

### 2. **Testability**
- Each module can be unit tested independently
- Mock dependencies easily
- Test coverage by feature

### 3. **Extensibility**
- Add new checks without affecting existing ones
- Add new output formats (JSON, XML) easily
- Plugin architecture possible

### 4. **Reusability**
- Services can be reused in other projects
- Check modules are independent
- Clear APIs

### 5. **Collaboration**
- Multiple developers can work on different modules
- Reduced merge conflicts
- Clear ownership of components

## Adding New Features

### Adding a New Security Check

1. **Create check method in appropriate module:**

```csharp
// In Checks/AuthenticationChecks.cs
public SecurityCheck CheckNewFeature()
{
    var value = _parser.GetConfigValue("NewParameter");
    
    return new SecurityCheck
    {
        Name = "New Feature Check",
        Description = "What this checks",
        Status = value == "secure" ? CheckStatus.Pass : CheckStatus.Fail,
        Details = $"Current value: {value}",
        Recommendation = "Set 'NewParameter secure' in sshd_config"
    };
}
```

2. **Add to orchestrator:**

```csharp
// In Core/SshSecurityChecker.cs
results.AddCheck(_authChecks.CheckNewFeature());
```

### Adding a New Check Category

1. **Create new check class:**

```csharp
// Checks/ComplianceChecks.cs
namespace SshSecurityCheck.Checks;

internal class ComplianceChecks
{
    private readonly ConfigurationParser _parser;
    
    public ComplianceChecks(ConfigurationParser parser)
    {
        _parser = parser;
    }
    
    public SecurityCheck CheckCompliance()
    {
        // Implementation
    }
}
```

2. **Register in orchestrator:**

```csharp
// Core/SshSecurityChecker.cs
private readonly ComplianceChecks _complianceChecks;

// In constructor
_complianceChecks = new ComplianceChecks(_parser);

// In RunAllChecksAsync
results.AddCheck(_complianceChecks.CheckCompliance());
```

### Adding a New Output Format

1. **Create new generator:**

```csharp
// Services/JsonReportGenerator.cs
namespace SshSecurityCheck.Services;

internal class JsonReportGenerator
{
    public async Task GenerateAsync(SecurityCheckResults results, string outputPath)
    {
        // JSON generation logic
    }
}
```

2. **Use in Program.cs:**

```csharp
if (options.OutputFormat == "json")
{
    var jsonGenerator = new JsonReportGenerator();
    await jsonGenerator.GenerateAsync(results, options.OutputFile);
}
```

## Testing Strategy

### Unit Tests
Each module can be tested independently:

```csharp
[Fact]
public void ArgumentParser_ParsesVerboseFlag()
{
    var parser = new ArgumentParser();
    var result = parser.Parse(new[] { "--verbose" });
    Assert.True(result.Verbose);
}

[Fact]
public void AuthenticationChecks_DetectsInsecureRootLogin()
{
    var mockParser = new Mock<ConfigurationParser>();
    mockParser.Setup(p => p.CheckConfigValue("PermitRootLogin", "no"))
              .Returns(false);
    
    var checks = new AuthenticationChecks(mockParser.Object);
    var result = checks.CheckRootLogin();
    
    Assert.Equal(CheckStatus.Fail, result.Status);
}
```

### Integration Tests
Test complete workflows:

```csharp
[Fact]
public async Task FullSecurityScan_WithSecureConfig_PassesAllChecks()
{
    var configPath = CreateTestSecureConfig();
    var checker = new SshSecurityChecker(configPath);
    
    var results = await checker.RunAllChecksAsync();
    
    Assert.True(results.AllChecksPassed);
}
```

## Performance Considerations

- **Lazy Loading**: Configuration parsed once
- **Async Operations**: File I/O is asynchronous
- **Minimal Allocations**: Reuse services
- **Parallel Checks**: Can be parallelized if needed

## Migration Notes

### From Monolithic to Modular

**Before** (1,500 lines in one file):
- Hard to navigate
- Difficult to test
- Tight coupling
- Hard to extend

**After** (multiple focused files):
- Easy navigation
- Unit testable
- Loose coupling
- Easy to extend

### Breaking Changes
None - external API remains the same:
```bash
sudo dotnet run --verbose --output report.txt
```

## Future Enhancements

### Possible Extensions
1. **Plugin System**: Load checks from external assemblies
2. **Configuration Profiles**: CIS, STIG, custom profiles
3. **Remote Checking**: Check SSH on remote servers
4. **Continuous Monitoring**: Run as service
5. **Web Dashboard**: Real-time security monitoring
6. **Database Storage**: Store check history
7. **Alerting**: Email/Slack notifications
8. **Remediation**: Auto-fix security issues

### Dependency Injection
Can be enhanced with DI container:

```csharp
// Program.cs
var services = new ServiceCollection()
    .AddSingleton<ConfigurationParser>()
    .AddSingleton<DisplayService>()
    .AddSingleton<ReportGenerator>()
    .AddTransient<SshSecurityChecker>()
    .BuildServiceProvider();

var checker = services.GetRequiredService<SshSecurityChecker>();
```

## Documentation Standards

All code includes:
- XML documentation comments
- Purpose and usage descriptions
- Parameter descriptions
- Return value descriptions
- Example usage where appropriate

## Code Quality Metrics

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Lines per file | 1,500 | ~200 avg | 7.5x better |
| Cyclomatic complexity | High | Low | Much simpler |
| Test coverage | Difficult | Easy | Highly testable |
| Maintainability | Low | High | Easy to modify |
| Reusability | None | High | Services reusable |

## Summary

The modular architecture provides:
- ? Clear separation of concerns
- ? High testability
- ? Easy maintenance
- ? Simple extensibility
- ? Better code organization
- ? Professional structure
- ? Team-friendly codebase

The refactoring maintains 100% backward compatibility while providing a solid foundation for future enhancements.

---

**Version**: 2.0.0 (Modular)  
**Lines of Code**: ~1,500 (distributed across multiple files)  
**Files**: 16 focused modules  
**Last Updated**: 2024
