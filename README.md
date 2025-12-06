# SSH Security Check for Linux OS

A comprehensive .NET application for analyzing and auditing SSH server security configurations on Linux systems.

## ?? Overview

This application performs automated security audits of SSH (Secure Shell) configurations to identify potential vulnerabilities and misconfigurations. It checks 23+ critical security parameters and provides actionable recommendations for hardening your SSH server.

## ?? Documentation

**Quick Links:**
- ?? **[Complete Documentation Index](Docs/INDEX.md)** - Navigate all documentation
- ?? **[Quick Start Guide](Docs/QUICK_START.md)** - Get started in 5 minutes
- ?? **[Technical Documentation](Docs/TECHNICAL_DOCUMENTATION.md)** - Architecture and API reference
- ????? **[Development Guide](Docs/DEVELOPMENT_GUIDE.md)** - Contributing and extending
- ?? **[Changelog](Docs/CHANGELOG.md)** - Version history and updates
- ?? **[Project Summary](Docs/PROJECT_SUMMARY.md)** - Project overview
- ?? **[Publishing Guide](PUBLISH.md)** - Build and distribution instructions

**Configuration:**
- ?? **[Secure SSH Config Example](sshd_config.secure.example)** - Reference configuration

## ? Features

- **Comprehensive Security Checks**: Validates 23+ critical SSH configuration parameters
- **Color-Coded Output**: Easy-to-read results with visual indicators
- **Detailed Recommendations**: Specific guidance on fixing security issues
- **Report Generation**: Export detailed security reports to file
- **Verbose Mode**: Get in-depth information about each check
- **Service Status Validation**: Ensures SSH service is running properly
- **File Permission Checks**: Validates security of configuration and key files
- **Security Score**: Overall security rating based on check results

## ??? Security Checks Performed

### Critical Authentication Checks
- ? Root login configuration
- ? Password authentication (recommends key-based auth)
- ? Empty password prevention
- ? Public key authentication status
- ? SSH protocol version (ensures Protocol 2)
- ? Host-based authentication

### Access Control
- ? Maximum authentication attempts
- ? Login grace time
- ? User/group access restrictions
- ? Strict modes enforcement

### Network Security
- ? X11 forwarding
- ? TCP forwarding
- ? Agent forwarding
- ? Tunnel permissions

### Session Management
- ? Client alive interval (idle timeout)
- ? Client alive count max
- ? Challenge-response authentication
- ? PAM integration

### System Configuration
- ? User environment variables
- ? Rhosts file handling
- ? SSH service status
- ? Configuration file permissions
- ? Private key file permissions

## ?? Requirements

- **.NET 10.0** or later
- **Linux Operating System** (Ubuntu, Debian, CentOS, RHEL, etc.)
- **Root/sudo privileges** (required to read SSH configuration)
- **SSH server installed** (openssh-server)

## ?? Installation

### 1. Clone or Download the Project

```bash
cd /path/to/project
```

### 2. Build the Application

```bash
dotnet build
```

### 3. Run the Application

```bash
sudo dotnet run
```

## ?? Usage

### Basic Usage

Run with default settings:

```bash
sudo dotnet run
```

### Verbose Mode

Get detailed information about each check:

```bash
sudo dotnet run --verbose
```

### Custom Configuration File

Specify a different SSH configuration file:

```bash
sudo dotnet run --config /etc/ssh/sshd_config.d/custom.conf
```

### Generate Report

Save security audit results to a file:

```bash
sudo dotnet run --output security-report.txt
```

### Combined Options

Use multiple options together:

```bash
sudo dotnet run --verbose --output report.txt
```

### Display Help

```bash
dotnet run --help
```

## ?? Command Line Options

| Option | Short | Description |
|--------|-------|-------------|
| `--help` | `-h` | Display help information |
| `--verbose` | `-v` | Enable detailed output with recommendations |
| `--config <path>` | `-c` | Specify custom SSH config file path |
| `--output <file>` | `-o` | Save report to specified file |

## ?? Example Output

```
=== SSH Security Check for Linux OS ===
Version: 1.0.0
Description: Comprehensive SSH security configuration analyzer

======================================================================
SECURITY CHECK RESULTS
======================================================================
[?] Root Login Configuration
    Description: Root should not be allowed to login directly via SSH
    Details: PermitRootLogin is set to 'no'

[?] Password Authentication
    Description: Password authentication should be disabled in favor of key-based auth
    ? Recommendation: Set 'PasswordAuthentication no' in sshd_config and use SSH keys instead

[?] Empty Passwords
    Description: Empty passwords should never be allowed
    Details: Empty passwords are prohibited

...

======================================================================

Summary:
  Total Checks: 23
  Passed: 18
  Failed: 3
  Warnings: 2

  Security Score: 78.3%

  ? Good, but there's room for improvement.
```

## ?? Configuration Best Practices

Based on the security checks, here are recommended SSH hardening settings:

### Edit SSH Configuration

```bash
sudo nano /etc/ssh/sshd_config
```

### Recommended Settings

```bash
# Authentication
PermitRootLogin no
PasswordAuthentication no
PubkeyAuthentication yes
PermitEmptyPasswords no
ChallengeResponseAuthentication no
UsePAM yes

# Protocol and Encryption
Protocol 2

# Access Control
MaxAuthTries 4
LoginGraceTime 60
StrictModes yes
AllowUsers your-username  # Specify allowed users

# Forwarding and Tunneling
X11Forwarding no
AllowTcpForwarding no
AllowAgentForwarding no
PermitTunnel no

# Session Management
ClientAliveInterval 300
ClientAliveCountMax 2

# Additional Security
HostbasedAuthentication no
IgnoreRhosts yes
PermitUserEnvironment no
```

**For complete configuration reference, see [sshd_config.secure.example](sshd_config.secure.example)**

### Apply Changes

After modifying the configuration:

```bash
# Test configuration
sudo sshd -t

# Restart SSH service
sudo systemctl restart ssh
# or
sudo systemctl restart sshd
```

## ?? Setting Up SSH Key Authentication

If password authentication is currently enabled, follow these steps to switch to key-based authentication:

### 1. Generate SSH Key (on client machine)

```bash
ssh-keygen -t ed25519 -C "your_email@example.com"
```

### 2. Copy Public Key to Server

```bash
ssh-copy-id username@server-ip
```

### 3. Test Key-Based Login

```bash
ssh username@server-ip
```

### 4. Disable Password Authentication

Once key-based auth is working, update `/etc/ssh/sshd_config`:

```bash
PasswordAuthentication no
```

Then restart SSH:

```bash
sudo systemctl restart ssh
```

## ?? Understanding Security Scores

| Score | Rating | Description |
|-------|--------|-------------|
| 90-100% | Excellent | Highly secure configuration |
| 70-89% | Good | Secure with minor improvements needed |
| 50-69% | Fair | Several security issues to address |
| Below 50% | Poor | Critical security vulnerabilities present |

## ?? Troubleshooting

### Permission Denied Error

**Problem**: `Error: Permission denied`

**Solution**: Run with sudo privileges:
```bash
sudo dotnet run
```

### SSH Config Not Found

**Problem**: `SSH config file not found`

**Solution**: Verify SSH is installed:
```bash
sudo apt-get install openssh-server  # Ubuntu/Debian
sudo yum install openssh-server      # CentOS/RHEL
```

### Platform Not Supported

**Problem**: `This application must be run on a Linux system`

**Solution**: This tool only works on Linux. For Windows SSH server security, use different tools.

**For more troubleshooting help, see [Quick Start Guide](Docs/QUICK_START.md#-troubleshooting-guide)**

## ?? Technical Details

### Architecture

The application is structured into several key components:

- **Program.cs**: Main entry point and CLI interface
- **SshSecurityChecker**: Core security validation logic
- **SecurityCheckResults**: Results aggregation and scoring
- **SecurityCheck**: Individual check representation

### Technology Stack

- **.NET 10.0**: Modern .NET platform
- **C# 14.0**: Latest C# language features
- **System.Diagnostics**: Process execution for service checks
- **System.Text.RegularExpressions**: Configuration parsing
- **System.IO**: File and permission operations

### Code Quality

- Comprehensive XML documentation
- SOLID principles
- Asynchronous operations where applicable
- Proper error handling and user feedback
- Cross-platform Linux compatibility

**For detailed technical information, see [Technical Documentation](Docs/TECHNICAL_DOCUMENTATION.md)**

## ?? License

This project is provided under the MIT License. See [LICENSE](LICENSE) for details.

## ?? Contributing

Contributions are welcome! Areas for enhancement:

- Additional security checks
- Support for SSH client configuration (~/.ssh/config)
- Integration with security frameworks (CIS, STIG)
- Automated remediation options
- Web-based dashboard
- Multi-server audit support

**For contribution guidelines, see [Development Guide](Docs/DEVELOPMENT_GUIDE.md#-contributing-guidelines)**

## ?? Disclaimer

This tool is designed to help identify security issues but should not be considered a complete security solution. Always:

- Review recommendations for your specific use case
- Test configuration changes in a safe environment
- Keep SSH and system packages updated
- Follow your organization's security policies
- Consider additional security layers (fail2ban, firewall rules, etc.)

## ?? Additional Resources

- [OpenSSH Security Best Practices](https://www.openssh.com/security.html)
- [CIS Benchmark for SSH](https://www.cisecurity.org/)
- [SSH Hardening Guide](https://www.ssh.com/academy/ssh/hardening)
- [NIST SSH Guidelines](https://csrc.nist.gov/)

## ?? Support

For issues, questions, or suggestions, please open an issue in the project repository.

---

**Version**: 1.0.0  
**Last Updated**: 2024  
**Requires**: .NET 10.0, Linux OS  
**Documentation**: [Docs/INDEX.md](Docs/INDEX.md)
