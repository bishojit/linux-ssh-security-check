# SSH Security Check - Quick Start Guide

## ?? Quick Start (5 Minutes)

### Step 1: Prerequisites Check

Verify you have the required components:

```bash
# Check .NET version
dotnet --version
# Should show 10.0 or higher

# Check if running on Linux
uname -s
# Should show "Linux"

# Check if SSH is installed
which sshd
# Should show path like /usr/sbin/sshd

# Verify SSH config exists
ls -la /etc/ssh/sshd_config
```

### Step 2: Build and Run

```bash
# Navigate to project directory
cd linux-ssh-security-check

# Build the project
dotnet build

# Run with sudo (required to read SSH config)
sudo dotnet run
```

### Step 3: Interpret Results

**Example Output:**
```
=== SSH Security Check for Linux OS ===

[?] Root Login Configuration
[?] Password Authentication
[?] Empty Passwords
...

Summary:
  Total Checks: 23
  Passed: 18
  Failed: 3
  Warnings: 2

  Security Score: 78.3%
```

**Status Symbols:**
- ? (Green) = Passed - Configuration is secure
- ? (Red) = Failed - Security issue found
- ? (Yellow) = Warning - Potential issue

---

## ?? Understanding Your Security Score

| Score | Status | Action Required |
|-------|--------|-----------------|
| 90-100% | ?? Excellent | Maintain current security |
| 70-89% | ?? Good | Address minor issues |
| 50-69% | ?? Fair | Fix multiple issues soon |
| <50% | ?? Poor | Immediate attention needed |

---

## ?? Common Issues and Fixes

### Issue #1: Password Authentication Enabled

**Check Failed:**
```
[?] Password Authentication
    ? Recommendation: Set 'PasswordAuthentication no' in sshd_config
```

**Solution:**

1. **First, set up SSH keys** (if not already done):
```bash
# On your local machine (client)
ssh-keygen -t ed25519

# Copy key to server
ssh-copy-id username@your-server-ip

# Test key-based login
ssh username@your-server-ip
```

2. **Then disable password auth**:
```bash
sudo nano /etc/ssh/sshd_config
```

Find and modify:
```
PasswordAuthentication no
```

3. **Restart SSH**:
```bash
sudo systemctl restart ssh
```

---

### Issue #2: Root Login Enabled

**Check Failed:**
```
[?] Root Login Configuration
    ? Recommendation: Set 'PermitRootLogin no' in sshd_config
```

**Solution:**

1. **Create a regular user with sudo access** (if needed):
```bash
sudo adduser yourusername
sudo usermod -aG sudo yourusername
```

2. **Disable root login**:
```bash
sudo nano /etc/ssh/sshd_config
```

Change:
```
PermitRootLogin no
```

3. **Restart SSH**:
```bash
sudo systemctl restart ssh
```

---

### Issue #3: X11 Forwarding Enabled

**Check Warning:**
```
[?] X11 Forwarding
    ? Recommendation: Set 'X11Forwarding no' in sshd_config
```

**Solution:**

```bash
sudo nano /etc/ssh/sshd_config
```

Set:
```
X11Forwarding no
```

Restart:
```bash
sudo systemctl restart ssh
```

**Note:** Only disable if you don't need GUI applications over SSH.

---

## ?? Quick Security Hardening Template

Copy this configuration block to harden your SSH:

```bash
# Edit SSH config
sudo nano /etc/ssh/sshd_config

# Add or modify these lines:
# ================================

# Disable root login
PermitRootLogin no

# Use key-based authentication only
PasswordAuthentication no
PubkeyAuthentication yes
PermitEmptyPasswords no

# Limit authentication attempts
MaxAuthTries 4
LoginGraceTime 60

# Disable risky features
X11Forwarding no
AllowTcpForwarding no
AllowAgentForwarding no
PermitTunnel no

# Session management
ClientAliveInterval 300
ClientAliveCountMax 2

# Additional security
StrictModes yes
HostbasedAuthentication no
IgnoreRhosts yes
PermitUserEnvironment no

# Use PAM
UsePAM yes

# Restrict users (optional - replace with your username)
AllowUsers yourusername

# ================================

# Save and exit (Ctrl+X, Y, Enter)

# Test configuration
sudo sshd -t

# If test passes, restart SSH
sudo systemctl restart ssh
```

---

## ?? Advanced Usage Examples

### Generate Detailed Report

```bash
sudo dotnet run --verbose --output /var/reports/ssh-security-$(date +%Y%m%d).txt
```

### Check Custom Configuration

```bash
sudo dotnet run --config /etc/ssh/sshd_config.d/custom.conf
```

### Automated Daily Check

Create a cron job:

```bash
sudo crontab -e
```

Add:
```
0 2 * * * cd /path/to/linux-ssh-security-check && /usr/bin/dotnet run --output /var/log/ssh-security-$(date +\%Y\%m\%d).log 2>&1
```

### Integration with Monitoring

```bash
#!/bin/bash
RESULT=$(sudo dotnet run)
SCORE=$(echo "$RESULT" | grep "Security Score" | awk '{print $3}' | tr -d '%')

if (( $(echo "$SCORE < 80" | bc -l) )); then
    # Send alert
    echo "SSH Security Score below 80%: $SCORE%" | mail -s "SSH Security Alert" admin@example.com
fi
```

---

## ??? Troubleshooting Guide

### Problem: "Permission denied"

**Error:**
```
Error: Permission denied. Please run with sudo privileges.
```

**Solution:**
```bash
sudo dotnet run
```

---

### Problem: "This application must be run on a Linux system"

**Error:**
```
Error: This application must be run on a Linux system.
```

**Solution:**
This tool only works on Linux. If you're on:
- **Windows**: Use WSL (Windows Subsystem for Linux) or a Linux VM
- **macOS**: Use a Linux VM or container

---

### Problem: "SSH config file not found"

**Error:**
```
Error: SSH config file not found at /etc/ssh/sshd_config
```

**Solution:**
Install OpenSSH server:

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install openssh-server

# CentOS/RHEL
sudo yum install openssh-server

# Start SSH service
sudo systemctl start ssh
sudo systemctl enable ssh
```

---

### Problem: Locked out after config changes

**Prevention:**
1. Always keep an existing SSH session open when testing
2. Test configuration before restarting:
```bash
sudo sshd -t
```

**Recovery:**
If locked out:
1. Access server via console (cloud provider panel, physical access, etc.)
2. Revert changes:
```bash
sudo nano /etc/ssh/sshd_config
# Restore previous settings
sudo systemctl restart ssh
```

---

## ?? Next Steps

After running the security check:

1. **Review Failed Checks** - Focus on red items first
2. **Plan Changes** - Don't apply all changes at once
3. **Test Configuration** - Always use `sudo sshd -t`
4. **Keep Backup Session** - Don't close your SSH session until verified
5. **Document Changes** - Keep a log of modifications
6. **Schedule Regular Checks** - Run this tool monthly
7. **Stay Updated** - Keep SSH and system packages current

---

## ?? Command Reference

```bash
# Basic run
sudo dotnet run

# Show help
dotnet run --help

# Verbose output
sudo dotnet run --verbose

# Custom config
sudo dotnet run --config /path/to/config

# Generate report
sudo dotnet run --output report.txt

# All options combined
sudo dotnet run --verbose --config /etc/ssh/sshd_config --output report.txt
```

---

## ? Pro Tips

1. **Backup First**: Always backup your SSH config before changes
   ```bash
   sudo cp /etc/ssh/sshd_config /etc/ssh/sshd_config.backup
   ```

2. **Test Before Restart**: Validate configuration syntax
   ```bash
   sudo sshd -t
   ```

3. **Keep a Session Open**: Don't close all SSH sessions until verified

4. **Use Fail2ban**: Complement SSH hardening with intrusion prevention
   ```bash
   sudo apt-get install fail2ban
   ```

5. **Monitor Logs**: Check SSH auth logs regularly
   ```bash
   sudo tail -f /var/log/auth.log
   ```

6. **Change Default Port**: (Optional) Use non-standard SSH port
   ```
   Port 2222
   ```

7. **Enable Firewall**: Restrict SSH access by IP
   ```bash
   sudo ufw allow from 192.168.1.0/24 to any port 22
   ```

---

## ?? Getting Help

- **Configuration Issues**: Check `/var/log/auth.log` for SSH errors
- **Connection Problems**: Verify firewall rules with `sudo ufw status`
- **Service Status**: Check with `sudo systemctl status ssh`
- **Test Config**: Run `sudo sshd -t` to validate syntax

---

**Version**: 1.0.0  
**Platform**: Linux with .NET 10.0  
**License**: Free for security auditing
