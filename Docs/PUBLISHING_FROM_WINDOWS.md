# Publishing from Windows - Quick Guide

## ? Problem Solved!

The cross-compilation error has been fixed. You can now publish from Windows to Linux.

## ?? What Was Changed

### 1. Fixed Project Configuration (`linux-ssh-security-check.csproj`)

**Before:**
```xml
<PublishAot>true</PublishAot>
<InvariantGlobalization>true</InvariantGlobalization>
```

**After:**
```xml
<PublishAot>false</PublishAot>
<InvariantGlobalization>false</InvariantGlobalization>
```

**Why:** `PublishAot=true` enables Native AOT compilation which doesn't support cross-OS builds from Windows to Linux.

### 2. Fixed Platform-Specific Code (`Program.cs`)

Added `OperatingSystem.IsLinux()` checks around `File.GetUnixFileMode()` calls to prevent platform compatibility warnings.

### 3. Updated Documentation (`PUBLISH.md`)

Added comprehensive sections about:
- Cross-compilation limitations
- Windows vs Linux build commands
- WSL and Docker solutions
- Troubleshooting for cross-compilation errors

---

## ?? How to Publish from Windows (Recommended)

### Option 1: Multi-File Self-Contained (Works from Windows)

```powershell
# Clean previous builds
dotnet clean

# Publish for Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -o publish/linux-x64
```

**Output:** `publish\linux-x64\` folder (~79 MB with multiple files)

**Distribution:**
```powershell
# Create tarball for Linux users
tar -czf linux-ssh-security-check-linux-x64.tar.gz -C publish/linux-x64 .

# Or create zip
Compress-Archive -Path publish/linux-x64/* -DestinationPath linux-ssh-security-check-linux-x64.zip
```

---

### Option 2: Framework-Dependent (Smallest)

```powershell
# Requires .NET 10 on target Linux system
dotnet publish -c Release -r linux-x64 --self-contained false -o publish/linux-x64-fd
```

**Output:** `publish\linux-x64-fd\` folder (~200 KB)

---

### Option 3: Single-File via WSL (Best User Experience)

**Step 1: Install WSL**
```powershell
wsl --install
```

**Step 2: Install .NET in WSL**
```bash
# In WSL terminal
cd ~
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Add to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
```

**Step 3: Build Single-File**
```bash
# Navigate to project (Windows drives are at /mnt/)
cd /mnt/d/P_Bikiran/linux-ssh-security-check

# Build single-file executable
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/linux-x64-single
```

**Output:** `publish/linux-x64-single/linux-ssh-security-check` (single ~79 MB file)

---

## ?? Verify Your Build

### Check What Was Created

```powershell
# List all files
Get-ChildItem -Path "publish/linux-x64"

# Check total size
Get-ChildItem -Path "publish/linux-x64" -Recurse -File | 
  Measure-Object -Property Length -Sum | 
  Select-Object @{Name="TotalSizeMB";Expression={[math]::Round($_.Sum / 1MB, 2)}}

# Find main executable
Get-ChildItem -Path "publish/linux-x64" -Filter "linux-ssh-security-check*"
```

---

## ?? Test on Linux

### Transfer Files

```powershell
# Using SCP
scp -r publish/linux-x64 user@linux-server:~/ssh-security-check

# Or create archive first
tar -czf linux-ssh-security-check.tar.gz -C publish/linux-x64 .
scp linux-ssh-security-check.tar.gz user@linux-server:~
```

### Run on Linux

```bash
# Extract (if using archive)
tar -xzf linux-ssh-security-check.tar.gz -C ~/ssh-security-check

# Make executable
chmod +x ~/ssh-security-check/linux-ssh-security-check

# Test
sudo ~/ssh-security-check/linux-ssh-security-check --help
sudo ~/ssh-security-check/linux-ssh-security-check --verbose
```

---

## ?? Build Matrix

| Build Method | Size | Single File | Build From Windows | Requires .NET on Target |
|--------------|------|-------------|-------------------|------------------------|
| Self-Contained | ~79 MB | No | ? Yes | ? No |
| Framework-Dependent | ~200 KB | No | ? Yes | ? Yes |
| Single-File (WSL) | ~79 MB | Yes | ?? Via WSL | ? No |
| Single-File (Docker) | ~79 MB | Yes | ?? Via Docker | ? No |

---

## ?? Current Configuration

Your project is now configured with:

```xml
<PublishAot>false</PublishAot>           <!-- Disabled for cross-compilation -->
<InvariantGlobalization>false</InvariantGlobalization>  <!-- Allow globalization -->
```

This configuration allows:
- ? Building from Windows to Linux
- ? Multi-file self-contained deployment
- ? Framework-dependent deployment
- ? Trimming (with multi-file output)

For single-file deployment, use WSL or build on Linux.

---

## ?? Quick Troubleshooting

### Error: "Cross-OS native compilation is not supported"
**Solution:** This was fixed by setting `PublishAot=false` in the project file.

### Warning: CA1416 about GetUnixFileMode
**Solution:** Fixed by adding `OperatingSystem.IsLinux()` checks in the code.

### Want Single-File from Windows?
**Solution:** Use WSL or Docker (see Option 3 above).

---

## ?? More Information

- Full publishing guide: [PUBLISH.md](PUBLISH.md)
- Quick start: [Docs/QUICK_START.md](Docs/QUICK_START.md)
- Technical docs: [Docs/TECHNICAL_DOCUMENTATION.md](Docs/TECHNICAL_DOCUMENTATION.md)

---

**Status:** ? Ready to publish from Windows!  
**Last Updated:** 2024-12-06  
**Tested:** Windows 11 ? Linux x64
