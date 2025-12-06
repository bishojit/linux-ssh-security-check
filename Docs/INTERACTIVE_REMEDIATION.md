# Interactive Remediation Feature

## ?? Overview

The SSH Security Check application now includes an **Interactive Remediation Mode** that allows you to automatically fix security issues with user prompts and safety measures.

## ? Key Features

### 1. **Interactive Selection** ???
- Review each failed security check
- Choose which issues to fix
- Skip issues you want to handle manually

### 2. **Automatic Backup** ??
- Creates timestamped backup before any changes
- Format: `/etc/ssh/sshd_config.backup_YYYYMMDD_HHMMSS`
- Easy restoration if needed

### 3. **Safe Configuration Update** ?
- Updates SSH configuration parameters
- Adds comments with timestamps
- Preserves existing configuration structure

### 4. **Clear Instructions** ??
- Shows next steps after fixes applied
- Warns about testing requirements
- Provides rollback commands

## ?? Usage

### Basic Interactive Fix

```bash
sudo dotnet run --fix
```

### With Verbose Output

```bash
sudo dotnet run --fix --verbose
```

### Fix with Report Generation

```bash
sudo dotnet run --fix --output security-report.txt
```

## ?? Example Session

```
=== SSH Security Check for Linux OS ===
Version: 1.0.0
Description: Comprehensive SSH security configuration analyzer

... [security checks run] ...

??????????????????????????????????????????????????????????????????
?           INTERACTIVE SECURITY REMEDIATION                     ?
??????????????????????????????????????????????????????????????????

Found 3 security issue(s) that can be automatically fixed.

?? Root Login Configuration
?  Issue: PermitRootLogin is not properly configured
?  Fix: Set 'PermitRootLogin no' in sshd_config
??
  Do you want to fix this issue? (y/n): y

?? Password Authentication
?  Issue: Password authentication is enabled
?  Fix: Set 'PasswordAuthentication no' in sshd_config and use SSH keys instead
??
  Do you want to fix this issue? (y/n): y

?? X11 Forwarding
?  Issue: X11Forwarding may be enabled
?  Fix: Set 'X11Forwarding no' in sshd_config unless X11 is required
??
  Do you want to fix this issue? (y/n): n

??  Creating backup of /etc/ssh/sshd_config...
? Backup created: /etc/ssh/sshd_config.backup_20241206_150530
??  Applying security fixes...
? Security fixes applied successfully!

??????????????????????????????????????????????????????????????????
?                  IMPORTANT NEXT STEPS                          ?
??????????????????????????????????????????????????????????????????

1. Test the SSH configuration:
   sudo sshd -t

2. If test passes, restart SSH service:
   sudo systemctl restart sshd

3. Keep your current SSH session open!
   ??  Test login in a NEW terminal before closing this one.

4. If something goes wrong, restore from backup:
   sudo cp /etc/ssh/sshd_config.backup_20241206_150530 /etc/ssh/sshd_config
   sudo systemctl restart sshd
```

## ?? Safety Features

### 1. **Permission Checks**
- Requires sudo/root privileges
- Prevents unauthorized modifications

### 2. **Automatic Backup**
- Created before any changes
- Timestamped for easy identification
- Can be restored manually or automatically

### 3. **Validation**
- Only fixes known parameters
- Uses safe configuration values
- Preserves existing comments

### 4. **Rollback Support**
- Automatic rollback on error
- Manual rollback instructions provided
- Backup files clearly labeled

## ?? What Can Be Fixed Automatically

The following security issues can be fixed automatically:

### Authentication Issues
- ? Root Login Configuration (`PermitRootLogin no`)
- ? Password Authentication (`PasswordAuthentication no`)
- ? Empty Passwords (`PermitEmptyPasswords no`)
- ? Public Key Authentication (`PubkeyAuthentication yes`)
- ? Host-Based Authentication (`HostbasedAuthentication no`)
- ? Challenge-Response Auth (`ChallengeResponseAuthentication no`)
- ? PAM Authentication (`UsePAM yes`)

### Access Control
- ? Maximum Auth Attempts (`MaxAuthTries 4`)
- ? Login Grace Time (`LoginGraceTime 60`)
- ? Strict Modes (`StrictModes yes`)
- ? User Environment (`PermitUserEnvironment no`)
- ? Ignore Rhosts (`IgnoreRhosts yes`)

### Network & Forwarding
- ? X11 Forwarding (`X11Forwarding no`)
- ? TCP Forwarding (`AllowTcpForwarding no`)
- ? Agent Forwarding (`AllowAgentForwarding no`)
- ? Permit Tunnel (`PermitTunnel no`)

### Session Management
- ? Client Alive Interval (`ClientAliveInterval 300`)
- ? Client Alive Count Max (`ClientAliveCountMax 2`)

## ?? What Cannot Be Fixed Automatically

Some issues require manual intervention:

- **SSH Service Status** - Service management
- **File Permissions** - Requires chmod commands
- **Private Key Permissions** - System-level changes
- **User Access Control** - Needs specific usernames/groups
- **Protocol Version** - Typically already defaults to 2

## ??? Best Practices

### Before Using --fix Mode

1. **Review Current Configuration**
   ```bash
   sudo dotnet run --verbose
   ```

2. **Understand the Issues**
   - Read each issue description
   - Check recommendations
   - Verify impact on your environment

3. **Have SSH Key Authentication Ready**
   - If fixing password authentication
   - Test key-based login first
   - Keep backup access method

4. **Keep a Terminal Open**
   - Always maintain an active SSH session
   - Don't close it until testing is complete

### During Fix Mode

1. **Read Each Prompt Carefully**
   - Understand what will be changed
   - Only fix what you need
   - Skip issues you want to handle manually

2. **Note the Backup Path**
   - Save the backup file location
   - You'll need it if rollback is necessary

3. **Follow Instructions**
   - Complete all testing steps
   - Don't skip the verification

### After Applying Fixes

1. **Test Configuration Syntax**
   ```bash
   sudo sshd -t
   ```

2. **Test in New Terminal** (IMPORTANT!)
   ```bash
   # In a NEW terminal window
   ssh your-username@your-server
   ```

3. **Verify Fixes Applied**
   ```bash
   sudo dotnet run --verbose
   ```

4. **Only Then Restart SSH**
   ```bash
   sudo systemctl restart sshd
   ```

## ?? Rollback Procedures

### Automatic Rollback

If the remediation service encounters an error, it will automatically attempt to restore from backup.

### Manual Rollback

If you need to manually restore configuration:

```bash
# Find available backups
ls -la /etc/ssh/sshd_config.backup_*

# Restore from specific backup
sudo cp /etc/ssh/sshd_config.backup_YYYYMMDD_HHMMSS /etc/ssh/sshd_config

# Test configuration
sudo sshd -t

# Restart SSH service
sudo systemctl restart sshd
```

## ?? Configuration Changes

### How Parameters Are Updated

1. **Existing Parameters**
   - Uncommented if commented
   - Value updated to secure setting
   - Original line replaced

2. **Missing Parameters**
   - Added at end of file
   - Includes timestamp comment
   - Clearly marked as automated

### Example Changes

**Before:**
```
#PermitRootLogin yes
PasswordAuthentication yes
```

**After:**
```
PermitRootLogin no
PasswordAuthentication no

# Added by SSH Security Check on 2024-12-06 15:05:30
MaxAuthTries 4
```

## ?? Testing Strategy

### Test Checklist

After applying fixes:

- [ ] Configuration syntax is valid (`sudo sshd -t`)
- [ ] Can connect via SSH in new terminal
- [ ] Authentication method works (password/key)
- [ ] User permissions are correct
- [ ] No error messages in logs (`/var/log/auth.log`)
- [ ] All required services accessible

### If Tests Fail

1. **Keep calm** - Your backup exists
2. **Don't close active SSH session**
3. **Check logs**: `sudo tail -f /var/log/auth.log`
4. **Restore backup** if needed
5. **Report issue** with details

## ?? Tips & Tricks

### Quick Fix Common Issues

```bash
# Fix only critical issues
sudo dotnet run --fix --verbose

# Review before committing
sudo dotnet run --verbose --output pre-fix-report.txt
sudo dotnet run --fix
sudo dotnet run --verbose --output post-fix-report.txt
```

### Backup Management

```bash
# List all backups
ls -lht /etc/ssh/sshd_config.backup_* | head -5

# Remove old backups (keep last 5)
ls -t /etc/ssh/sshd_config.backup_* | tail -n +6 | xargs sudo rm --
```

### Automated Testing

```bash
#!/bin/bash
# Save as test-ssh-fixes.sh

echo "Testing SSH configuration..."
sudo sshd -t
if [ $? -eq 0 ]; then
    echo "? Configuration is valid"
else
    echo "? Configuration has errors"
    exit 1
fi

echo "Restarting SSH service..."
sudo systemctl restart sshd
if [ $? -eq 0 ]; then
    echo "? SSH service restarted"
else
    echo "? Failed to restart SSH"
    exit 1
fi

echo "All tests passed!"
```

## ?? Troubleshooting

### Issue: Permission Denied

**Problem**: Cannot write to SSH config file

**Solution**: 
```bash
sudo dotnet run --fix
```

### Issue: Backup Creation Failed

**Problem**: Insufficient disk space or permissions

**Solution**: 
```bash
# Check disk space
df -h /etc/ssh

# Check permissions
ls -la /etc/ssh
```

### Issue: Cannot Connect After Fixes

**Problem**: Configuration broke SSH access

**Solution**: 
```bash
# From existing session
sudo cp /etc/ssh/sshd_config.backup_YYYYMMDD_HHMMSS /etc/ssh/sshd_config
sudo systemctl restart sshd

# Or from console/physical access
# Boot into recovery mode and restore backup
```

### Issue: Specific Parameter Not Working

**Problem**: A fix doesn't work as expected

**Solution**: 
```bash
# Check what was changed
sudo diff /etc/ssh/sshd_config.backup_* /etc/ssh/sshd_config

# Check logs
sudo journalctl -u sshd -n 50

# Verify syntax
sudo sshd -T | grep ParameterName
```

## ?? Related Documentation

- [README.md](../README.md) - Main documentation
- [QUICK_START.md](../Docs/QUICK_START.md) - Getting started guide
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Code structure
- [sshd_config.secure.example](../sshd_config.secure.example) - Reference config

## ?? Advanced Usage

### Scripted Remediation

```bash
# Auto-confirm all fixes (use with caution!)
echo -e "y\ny\ny\ny\ny" | sudo dotnet run --fix
```

### Pre-Production Testing

```bash
# Test on staging server first
sudo dotnet run --fix --config /tmp/test-sshd-config

# Review changes
diff /etc/ssh/sshd_config /tmp/test-sshd-config
```

### Integration with CI/CD

```yaml
# Example GitHub Actions workflow
- name: Check SSH Security
  run: |
    sudo dotnet run --output security-report.txt
    if [ $? -ne 0 ]; then
      echo "Security issues detected"
      # Optionally auto-fix in dev environment
      sudo dotnet run --fix --verbose
    fi
```

---

**Feature Version**: 2.1.0  
**Introduced**: 2024-12-06  
**Status**: Production Ready  
**Requires**: .NET 10.0, Linux, sudo/root privileges
