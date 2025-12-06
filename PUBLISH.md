# SSH Security Check - Publishing Guide

**Complete guide for building, publishing, and distributing the SSH Security Check application**

---

## ?? Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Publishing Methods](#publishing-methods)
  - [Self-Contained Deployment](#self-contained-deployment)
  - [Framework-Dependent Deployment](#framework-dependent-deployment)
  - [Single-File Deployment](#single-file-deployment)
- [Platform-Specific Publishing](#platform-specific-publishing)
- [Trimming and Optimization](#trimming-and-optimization)
- [Distribution](#distribution)
- [Installation Instructions](#installation-instructions)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

---

## Overview

This guide covers all methods to publish the SSH Security Check application for distribution on Linux systems. The application can be published in several formats depending on your deployment needs.

**Publishing Options:**

| Method | Size | Requires .NET Runtime | Portability | Use Case |
|--------|------|----------------------|-------------|----------|
| Self-Contained | ~65 MB | No | High | Production servers without .NET |
| Framework-Dependent | ~200 KB | Yes | Low | Systems with .NET pre-installed |
| Single-File | ~65 MB | No | Highest | Easy distribution, portable |
| Trimmed | ~30 MB | No | High | Size-optimized production |

---

## Prerequisites

### Required Software

1. **.NET SDK 10.0** or later
   ```bash
   dotnet --version
   # Should show 10.0 or higher
   ```

2. **Build Tools** (usually included with .NET SDK)
   ```bash
   dotnet build --version
   ```

### Workspace Preparation

1. **Navigate to project directory:**
   ```bash
   cd D:\P_Bikiran\linux-ssh-security-check\
   ```

2. **Clean previous builds:**
   ```bash
   dotnet clean
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Verify build:**
   ```bash
   dotnet build
   ```

---

## Publishing Methods

### Self-Contained Deployment

**Description:** Includes .NET runtime with the application. No .NET installation required on target system.

**Advantages:**
- Target system doesn't need .NET installed
- Version-independent (won't break with .NET updates)
- Fully self-contained

**Disadvantages:**
- Larger file size (~65 MB)
- Separate builds needed for each platform

#### Linux x64 (Most Common)

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=false
```

**Output:** `bin\Release\net10.0\linux-x64\publish\`

#### Linux ARM64 (Raspberry Pi, etc.)

```bash
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=false
```

**Output:** `bin\Release\net10.0\linux-arm64\publish\`

#### Linux ARM (32-bit)

```bash
dotnet publish -c Release -r linux-arm --self-contained true -p:PublishSingleFile=false
```

**Output:** `bin\Release\net10.0\linux-arm\publish\`

---

### Framework-Dependent Deployment

**Description:** Requires .NET 10 runtime on target system. Smallest deployment size.

**Advantages:**
- Very small size (~200 KB)
- Automatic updates with .NET runtime updates
- Cross-platform binary

**Disadvantages:**
- Requires .NET 10 runtime on target
- May break with major .NET updates

#### Publish Command

```bash
dotnet publish -c Release --self-contained false
```

**Output:** `bin\Release\net10.0\publish\`

#### Target System Requirements

User must install .NET 10 runtime:

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 10.0 --runtime dotnet

# Or use package manager
sudo apt-get update
sudo apt-get install -y dotnet-runtime-10.0
```

---

### Single-File Deployment

**Description:** Publishes entire application as a single executable file. Best for easy distribution.

**Advantages:**
- Single file to distribute
- No .NET runtime required on target
- Easy to deploy and manage
- Simplest for end users

**Disadvantages:**
- Larger file size (~65 MB)
- Slower first startup (extracts to temp)

#### Linux x64 Single-File

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

**Output:** `bin\Release\net10.0\linux-x64\publish\linux-ssh-security-check`

#### Linux ARM64 Single-File

```bash
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

### Trimmed Deployment (Size Optimized)

**Description:** Removes unused code to reduce size. Recommended for production.

**Advantages:**
- Smallest self-contained deployment (~30 MB)
- Faster startup
- Reduced memory footprint

**Disadvantages:**
- Requires testing (may break reflection)
- Longer build time

#### Linux x64 Trimmed

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link
```

**Output:** `bin\Release\net10.0\linux-x64\publish\linux-ssh-security-check`

---

## Platform-Specific Publishing

### Ubuntu / Debian

**Recommended Method:** Single-file, self-contained

```bash
# Build
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Output location
cd bin\Release\net10.0\linux-x64\publish\

# File produced
# linux-ssh-security-check (executable)
```

**Installation on Target:**

```bash
# Copy to system
sudo cp linux-ssh-security-check /usr/local/bin/ssh-security-check

# Make executable
sudo chmod +x /usr/local/bin/ssh-security-check

# Run
sudo ssh-security-check
```

---

### CentOS / RHEL / Fedora

**Same as Ubuntu** - use linux-x64 runtime identifier

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

### Alpine Linux

**Special Case:** Requires musl runtime

```bash
dotnet publish -c Release -r linux-musl-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

### Raspberry Pi

#### Raspberry Pi 3/4/5 (64-bit OS)

```bash
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

#### Raspberry Pi 2/3 (32-bit OS)

```bash
dotnet publish -c Release -r linux-arm --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

## Trimming and Optimization

### Aggressive Optimization

For production deployments where size matters:

```bash
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:TrimMode=link \
  -p:EnableCompressionInSingleFile=true \
  -p:DebugType=None \
  -p:DebugSymbols=false
```

**Options Explained:**
- `PublishSingleFile=true` - Single executable
- `PublishTrimmed=true` - Remove unused code
- `TrimMode=link` - Aggressive trimming
- `EnableCompressionInSingleFile=true` - Compress embedded files
- `DebugType=None` - No debug info
- `DebugSymbols=false` - No symbols

**Expected Size:** ~25-35 MB

### Testing Trimmed Builds

**Important:** Always test trimmed builds thoroughly!

```bash
# Build trimmed version
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:TrimMode=link

# Test on Linux VM or container
# Run all security checks
# Verify all features work
```

---

## Distribution

### Method 1: GitHub Releases

**Best for:** Open source projects

#### Step 1: Build for Multiple Platforms

```bash
# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64

# Linux ARM64
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -o publish/linux-arm64

# Linux ARM
dotnet publish -c Release -r linux-arm --self-contained true -p:PublishSingleFile=true -o publish/linux-arm
```

#### Step 2: Create Archives

```bash
# PowerShell on Windows
Compress-Archive -Path publish/linux-x64/* -DestinationPath ssh-security-check-v1.0.0-linux-x64.zip
Compress-Archive -Path publish/linux-arm64/* -DestinationPath ssh-security-check-v1.0.0-linux-arm64.zip
Compress-Archive -Path publish/linux-arm/* -DestinationPath ssh-security-check-v1.0.0-linux-arm.zip

# Or on Linux
tar -czf ssh-security-check-v1.0.0-linux-x64.tar.gz -C publish/linux-x64 .
tar -czf ssh-security-check-v1.0.0-linux-arm64.tar.gz -C publish/linux-arm64 .
tar -czf ssh-security-check-v1.0.0-linux-arm.tar.gz -C publish/linux-arm .
```

#### Step 3: Upload to GitHub

1. Create a release on GitHub
2. Upload the archives
3. Include SHA256 checksums

---

### Method 2: Package Repository

**Best for:** Internal distribution

#### Create DEB Package (Ubuntu/Debian)

```bash
# Structure
mkdir -p ssh-security-check-1.0.0/usr/local/bin
mkdir -p ssh-security-check-1.0.0/DEBIAN

# Copy binary
cp publish/linux-x64/linux-ssh-security-check ssh-security-check-1.0.0/usr/local/bin/ssh-security-check

# Create control file
cat > ssh-security-check-1.0.0/DEBIAN/control << EOF
Package: ssh-security-check
Version: 1.0.0
Section: admin
Priority: optional
Architecture: amd64
Maintainer: Your Name <your.email@example.com>
Description: SSH Security Configuration Checker
 Comprehensive SSH security audit tool for Linux systems.
 Validates 23+ security parameters and provides recommendations.
EOF

# Build package
dpkg-deb --build ssh-security-check-1.0.0

# Install
sudo dpkg -i ssh-security-check-1.0.0.deb
```

#### Create RPM Package (CentOS/RHEL)

```bash
# Create directory structure
mkdir -p ~/rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

# Create spec file
cat > ~/rpmbuild/SPECS/ssh-security-check.spec << EOF
Name:           ssh-security-check
Version:        1.0.0
Release:        1%{?dist}
Summary:        SSH Security Configuration Checker

License:        MIT
URL:            https://github.com/yourusername/ssh-security-check
Source0:        ssh-security-check-1.0.0.tar.gz

%description
Comprehensive SSH security audit tool for Linux systems.

%prep
%setup -q

%install
mkdir -p %{buildroot}/usr/local/bin
install -m 755 linux-ssh-security-check %{buildroot}/usr/local/bin/ssh-security-check

%files
/usr/local/bin/ssh-security-check

%changelog
* Sat Dec 07 2024 Your Name <your.email@example.com> - 1.0.0-1
- Initial release
EOF

# Build RPM
rpmbuild -ba ~/rpmbuild/SPECS/ssh-security-check.spec
```

---

### Method 3: Docker Container

**Best for:** Containerized environments

#### Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["linux-ssh-security-check.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish -r linux-musl-x64 --self-contained true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./linux-ssh-security-check"]
```

#### Build and Run

```bash
# Build image
docker build -t ssh-security-check:1.0.0 .

# Run (mount SSH config)
docker run --rm -v /etc/ssh:/etc/ssh:ro ssh-security-check:1.0.0
```

---

### Method 4: Direct Download

**Best for:** Simple distribution

#### Setup Web Server

```bash
# Create download directory
mkdir -p /var/www/downloads/ssh-security-check

# Copy binaries
cp publish/linux-x64/linux-ssh-security-check /var/www/downloads/ssh-security-check/ssh-security-check-linux-x64

# Generate checksums
cd /var/www/downloads/ssh-security-check
sha256sum ssh-security-check-linux-x64 > SHA256SUMS
```

#### User Installation

```bash
# Download
wget https://yourserver.com/downloads/ssh-security-check/ssh-security-check-linux-x64

# Verify checksum
wget https://yourserver.com/downloads/ssh-security-check/SHA256SUMS
sha256sum -c SHA256SUMS

# Install
sudo mv ssh-security-check-linux-x64 /usr/local/bin/ssh-security-check
sudo chmod +x /usr/local/bin/ssh-security-check
```

---

## Installation Instructions

### For End Users

Include these instructions with your distribution:

#### Method 1: Single Binary Installation

```bash
# 1. Download the binary
wget https://github.com/yourusername/ssh-security-check/releases/download/v1.0.0/ssh-security-check-linux-x64

# 2. Verify checksum (optional but recommended)
wget https://github.com/yourusername/ssh-security-check/releases/download/v1.0.0/SHA256SUMS
sha256sum -c SHA256SUMS

# 3. Make executable
chmod +x ssh-security-check-linux-x64

# 4. Move to system path
sudo mv ssh-security-check-linux-x64 /usr/local/bin/ssh-security-check

# 5. Verify installation
ssh-security-check --help

# 6. Run security check
sudo ssh-security-check
```

#### Method 2: Package Manager Installation

**Ubuntu/Debian:**

```bash
# Download package
wget https://yourserver.com/ssh-security-check_1.0.0_amd64.deb

# Install
sudo dpkg -i ssh-security-check_1.0.0_amd64.deb

# Run
sudo ssh-security-check
```

**CentOS/RHEL:**

```bash
# Download package
wget https://yourserver.com/ssh-security-check-1.0.0-1.x86_64.rpm

# Install
sudo rpm -i ssh-security-check-1.0.0-1.x86_64.rpm

# Run
sudo ssh-security-check
```

---

## Verification

### Post-Publish Verification

After publishing, verify the build:

#### 1. Check File Size

```bash
# Self-contained should be 60-70 MB
ls -lh bin/Release/net10.0/linux-x64/publish/linux-ssh-security-check

# Framework-dependent should be ~200 KB
ls -lh bin/Release/net10.0/publish/linux-ssh-security-check.dll
```

#### 2. Test on Linux

```bash
# Transfer to Linux system
scp bin/Release/net10.0/linux-x64/publish/linux-ssh-security-check user@linux-server:~

# On Linux system
chmod +x linux-ssh-security-check
sudo ./linux-ssh-security-check --help
sudo ./linux-ssh-security-check --verbose
```

#### 3. Verify All Features

```bash
# Test help
./linux-ssh-security-check --help

# Test basic run
sudo ./linux-ssh-security-check

# Test verbose mode
sudo ./linux-ssh-security-check --verbose

# Test report generation
sudo ./linux-ssh-security-check --output test-report.txt

# Test custom config
sudo ./linux-ssh-security-check --config /etc/ssh/sshd_config
```

#### 4. Check Dependencies

```bash
# Should show "not a dynamic executable" for self-contained
ldd linux-ssh-security-check

# Or for framework-dependent
ldd linux-ssh-security-check.dll
```

---

## Troubleshooting

### Common Issues

#### Issue 1: "Permission denied" when running

**Problem:**
```bash
./linux-ssh-security-check
bash: ./linux-ssh-security-check: Permission denied
```

**Solution:**
```bash
chmod +x linux-ssh-security-check
```

---

#### Issue 2: Missing .NET runtime (framework-dependent)

**Problem:**
```
You must install .NET to run this application.
```

**Solution:**
```bash
# Install .NET 10 runtime
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 10.0 --runtime dotnet
```

---

#### Issue 3: Large binary size

**Problem:** Self-contained binary is 100+ MB

**Solution:** Use trimmed publishing:
```bash
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:TrimMode=link
```

---

#### Issue 4: Trimmed binary crashes

**Problem:** Application crashes or features don't work after trimming

**Solution:** Disable trimming or use partial trimming:
```bash
# Disable trimming for problematic assemblies
<ItemGroup>
  <TrimmerRootAssembly Include="System.Text.RegularExpressions" />
</ItemGroup>
```

---

#### Issue 5: Cross-compilation errors

**Problem:** Building on Windows for Linux fails

**Solution:** Ensure you have the correct SDK and workloads:
```bash
# Check installed workloads
dotnet workload list

# Install if missing
dotnet workload install linux-x64
```

---

## Publishing Checklist

Use this checklist before distributing:

### Pre-Publish
- [ ] Code reviewed and tested
- [ ] All unit tests pass
- [ ] Documentation updated
- [ ] Version number incremented
- [ ] CHANGELOG.md updated

### Build
- [ ] Clean build completed
- [ ] Published for all target platforms
- [ ] Binaries verified on target systems
- [ ] All features tested
- [ ] Performance acceptable

### Distribution
- [ ] Archives created
- [ ] Checksums generated
- [ ] Release notes prepared
- [ ] Installation instructions included
- [ ] GitHub release created (if applicable)

### Post-Release
- [ ] Download links tested
- [ ] Installation instructions verified
- [ ] User feedback channel established
- [ ] Support documentation available

---

## Quick Reference

### Common Publish Commands

```bash
# Single-file, self-contained (RECOMMENDED)
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Trimmed for size optimization
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link

# Framework-dependent (requires .NET on target)
dotnet publish -c Release --self-contained false

# ARM64 (Raspberry Pi, ARM servers)
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true

# Alpine Linux (musl)
dotnet publish -c Release -r linux-musl-x64 --self-contained true -p:PublishSingleFile=true
```

### Runtime Identifiers (RID)

| Platform | RID | Description |
|----------|-----|-------------|
| Ubuntu/Debian 64-bit | `linux-x64` | Most common |
| CentOS/RHEL 64-bit | `linux-x64` | Same as Ubuntu |
| Alpine Linux | `linux-musl-x64` | Special musl libc |
| ARM64 | `linux-arm64` | Raspberry Pi 3/4/5 (64-bit) |
| ARM32 | `linux-arm` | Raspberry Pi 2/3 (32-bit) |

### File Sizes

| Build Type | Approximate Size |
|------------|------------------|
| Framework-dependent | ~200 KB |
| Self-contained | ~65 MB |
| Self-contained + trimmed | ~30 MB |
| Single-file self-contained | ~65 MB |
| Single-file + trimmed | ~30 MB |

---

## Additional Resources

### Official Documentation
- [.NET Publishing Overview](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [Single-File Deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file)
- [Trimming Options](https://docs.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)
- [Runtime Identifiers](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

### Tools
- **Advanced Installer** - Create installers for Windows
- **Inno Setup** - Free installer for Windows
- **checkinstall** - Create packages from source
- **fpm** - Build packages for multiple platforms

---

## Support

For issues with publishing:

1. Check [.NET GitHub Issues](https://github.com/dotnet/runtime/issues)
2. Review [Documentation](Docs/INDEX.md)
3. Open an issue in the project repository

---

**Version:** 1.0.0  
**Last Updated:** 2024  
**Target Framework:** .NET 10.0  
**Supported Platforms:** Linux (x64, ARM64, ARM, musl)

**Ready to Publish! ??**
