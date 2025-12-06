# Changelog

All notable changes to the SSH Security Check project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2024-12-06

### Added - Interactive Remediation Feature ??
- **Interactive Remediation Mode** (`--fix` flag)
  - User-guided security issue fixing
  - Interactive prompts for each security check
  - Ability to select which issues to fix
- **Automatic Backup System**
  - Timestamped backups before any changes
  - Format: `sshd_config.backup_YYYYMMDD_HHMMSS`
  - Automatic rollback on errors
  - Manual restore capabilities
- **New Services** (3 files, ~450 lines)
  - `InteractionService.cs` - User interaction and prompts
  - `BackupService.cs` - Configuration backup management
  - `RemediationService.cs` - Fix orchestration and application
- **18 Auto-Fixable Security Checks**
  - Authentication: Root login, password auth, empty passwords, public key auth, host-based auth, challenge-response, PAM, max auth tries
  - Access Control: Login grace time, strict modes, user environment, ignore rhosts
  - Network: X11 forwarding, TCP forwarding, agent forwarding, permit tunnel
  - Session: Client alive interval, client alive count max
- **Enhanced User Experience**
  - Color-coded prompts and messages
  - Clear safety instructions
  - Step-by-step testing guide
  - Rollback procedures

### Added - Modular Architecture ???
- **Complete Code Refactoring**
  - Split monolithic 1,500-line `Program.cs` into 16 focused modules
  - Average file size reduced from 1,500 to 83 lines (94% reduction)
- **New Folder Structure**
  - `Models/` - Data models (CommandLineOptions, SecurityCheck, SecurityCheckResults)
  - `Services/` - Infrastructure services (ArgumentParser, ConfigurationParser, DisplayService, ReportGenerator, SystemService, InteractionService, BackupService, RemediationService)
  - `Checks/` - Security validation modules (AuthenticationChecks, ProtocolChecks, AccessControlChecks, NetworkChecks, SessionChecks, SystemChecks)
  - `Core/` - Main orchestrator (SshSecurityChecker)
- **Benefits**
  - Highly testable - each module can be unit tested independently
  - Maintainable - easy to find and modify functionality
  - Extensible - simple to add new checks or features
  - Professional - industry-standard architecture

### Added - Enhanced Documentation ??
- **New Documentation Files**
  - `ARCHITECTURE.md` - Complete architecture documentation
  - `MIGRATION_SUMMARY.md` - Refactoring details
  - `Docs/INTERACTIVE_REMEDIATION.md` - Interactive fix feature guide (~600 lines)
  - `FEATURE_INTERACTIVE_REMEDIATION.md` - Feature summary
  - `EMOJI_FIX_SUMMARY.md` - Emoji usage guide
  - `PUBLISHING_FROM_WINDOWS.md` - Windows publishing guide
- **Updated Documentation**
  - `README.md` - Added interactive remediation section
  - `PUBLISH.md` - Enhanced with Windows/WSL/Docker solutions
  - `Docs/INDEX.md` - Added new documentation links
  - All emojis properly rendered (no more [?] placeholders)
- **Total Documentation**: ~26,000+ words across 15+ files

### Improved - Report Generation
- **ASCII-Safe Report Symbols**
  - `[PASS]`, `[FAIL]`, `[WARN]` instead of emojis
  - `>>` for recommendations
  - Better compatibility with text editors
- **Enhanced Report Format**
  - Security rating descriptions
  - Clearer status indicators
  - Improved readability

### Improved - Display Service
- **Proper Emoji Support**
  - ? (U+2713) for pass
  - ? (U+2717) for fail
  - ? (U+26A0) for warning
  - ? (U+2192) for recommendations
  - ? (U+274C) for errors
- **Cross-Platform Compatibility**
  - Modern terminal support
  - Consistent display across platforms

### Improved - Publishing Process
- **Cross-Compilation Fixed**
  - Disabled `PublishAot` for Windows-to-Linux builds
  - Fixed platform-specific API warnings
  - Added comprehensive publishing documentation
- **Multiple Publishing Methods**
  - Self-contained deployment (~79 MB)
  - Framework-dependent deployment (~200 KB)
  - Single-file via WSL/Docker
  - Trimmed builds (~30 MB)
- **Platform Support**
  - Linux x64, ARM64, ARM
  - Alpine Linux (musl)
  - Raspberry Pi

### Fixed
- **Platform Compatibility Warnings** (CA1416)
  - Added `OperatingSystem.IsLinux()` checks for Unix-specific APIs
  - Proper handling of `File.GetUnixFileMode()`
- **Cross-Compilation Errors**
  - Resolved Native AOT cross-OS issues
  - Windows to Linux publishing now works
- **Emoji Display Issues**
  - All `[?]` placeholders replaced with proper Unicode
  - Consistent emoji usage across documentation
  - ASCII fallbacks for reports

### Changed
- **Command-Line Interface**
  - Added `--fix` / `-f` flag for interactive remediation
  - Enhanced help text with new feature descriptions
- **Application Flow**
  - Modular service-based architecture
  - Clear separation of concerns
  - Improved error handling
- **Configuration Updates**
  - Project file updated for cross-platform builds
  - `PublishAot=false` for compatibility
  - `InvariantGlobalization=false` for better support

### Security
- **Safe Remediation**
  - Automatic backups before changes
  - Validation of all parameter updates
  - Rollback on errors
  - Clear safety warnings
  - No partial updates

### Performance
- **Maintained Performance**
  - Modular architecture with no performance penalty
  - Async operations throughout
  - Efficient file I/O
  - Low memory footprint

## [2.0.0] - 2024-12-06

### Added - Modular Architecture
- Complete code refactoring to modular design
- 16 focused modules replacing monolithic code
- Clear separation of concerns
- SOLID principles implementation

### Documentation
- Comprehensive architecture documentation
- Migration guide from monolithic to modular
- Enhanced code examples

## [1.0.0] - 2024

### Added
- Initial release of SSH Security Check application
- 23+ comprehensive security checks for SSH configuration
- Command-line interface with multiple options
  - Verbose mode for detailed output
  - Custom configuration file path support
  - Report generation to file
  - Help documentation
- Color-coded console output
  - Green for passed checks
  - Red for failed checks
  - Yellow for warnings
- Security scoring system (0-100%)
- Configuration parameter validation:
  - Root login configuration
  - Password authentication status
  - Public key authentication
  - Protocol version verification
  - Maximum authentication attempts
  - Login grace time
  - Strict modes
  - User environment variables
  - X11 forwarding
  - TCP forwarding
  - Agent forwarding
  - Host-based authentication
  - Rhosts handling
  - Challenge-response authentication
  - PAM integration
  - Client alive interval
  - Client alive count max
  - User/group access control
  - Tunnel permissions
- Service status checking
  - SSH/SSHD service validation via systemctl
- File permission checks
  - SSH configuration file permissions
  - Private key file permissions
- Report generation
  - Text-based detailed reports
  - Summary statistics
  - Individual check details with recommendations
- Comprehensive documentation
  - README.md with usage instructions
  - QUICK_START.md for rapid deployment
  - TECHNICAL_DOCUMENTATION.md for developers
  - Inline XML documentation for all classes and methods
- Error handling
  - Permission denied detection
  - Platform validation (Linux only)
  - Missing configuration file handling
  - Graceful exception management
- Built for .NET 10.0 and C# 14.0

### Security Features
- Validates critical SSH hardening parameters
- Provides actionable remediation recommendations
- Follows industry best practices (CIS, NIST guidelines)
- Checks for common misconfigurations
- Identifies deprecated or insecure settings

### Documentation
- User guide with examples
- Technical architecture documentation
- Quick start guide for common scenarios
- Troubleshooting section
- Configuration best practices
- SSH hardening template

### Performance
- Fast execution (< 500ms typical)
- Low memory footprint
- Async I/O operations
- Efficient regex-based parsing

## [Unreleased]

### Planned Features
- [ ] JSON output format option
- [ ] XML output format option
- [ ] Remote SSH server checking via SSH connection
- [ ] Batch processing for multiple servers
- [ ] Historical security score tracking
- [ ] Web-based dashboard
- [ ] Docker container support
- [ ] Integration with security frameworks (CIS Benchmark, STIG)
- [ ] SSH client configuration checking (~/.ssh/config)
- [ ] Dry-run mode for --fix flag
- [ ] Auto-confirm mode for scripting
- [ ] Custom fix profiles
- [ ] Integration with Ansible/Puppet
- [ ] Rollback history management
- [ ] Scheduled automatic fixes

### Possible Enhancements
- Multi-server audit support
- Real-time monitoring service
- Email/Slack notifications
- Database storage for check history
- API endpoints for automation
- Plugin system for custom checks
- Configuration templates library
- Compliance reporting (CIS, STIG, PCI-DSS)

---

## Version History Summary

| Version | Date | Major Changes |
|---------|------|---------------|
| 2.1.0 | 2024-12-06 | Interactive remediation, enhanced docs, emoji fixes |
| 2.0.0 | 2024-12-06 | Modular architecture refactoring |
| 1.0.0 | 2024 | Initial release with 23+ security checks |

---

## Breaking Changes

### Version 2.1.0
- None - Fully backward compatible
- All existing commands work unchanged

### Version 2.0.0
- None - Fully backward compatible
- Internal restructuring only
- No API changes

---

## Migration Guide

### Upgrading to 2.1.0
1. Pull latest changes: `git pull origin master`
2. Rebuild: `dotnet build`
3. New feature available: `sudo dotnet run --fix`
4. Review documentation: `Docs/INTERACTIVE_REMEDIATION.md`

### Upgrading to 2.0.0
1. No changes required
2. Same commands work as before
3. Optional: Review new architecture in `ARCHITECTURE.md`

---

## Contributing

See [DEVELOPMENT_GUIDE.md](Docs/DEVELOPMENT_GUIDE.md) for contribution guidelines.

---

## Support

- ?? Full documentation: [Docs/INDEX.md](Docs/INDEX.md)
- ?? Report issues: GitHub Issues
- ?? Discussions: GitHub Discussions
- ?? Contact: See README.md

---

**Last Updated**: 2024-12-06  
**Current Version**: 2.1.0  
**Repository**: https://github.com/bishojit/linux-ssh-security-check
