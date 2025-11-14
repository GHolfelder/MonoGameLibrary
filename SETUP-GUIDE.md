# MonoGame Library CI/CD Setup Guide

This guide explains how to set up and use the automated build, test, and deployment system for MonoGame Library.

## 🚀 Quick Start

### 1. **Repository Setup**
The workflows are now configured and ready to use! Here's what you get:

- ✅ **Automated builds** on every push/PR
- ✅ **Multi-platform testing** (Windows, Linux, macOS)
- ✅ **NuGet package generation**
- ✅ **Automated publishing** to NuGet.org
- ✅ **GitHub releases** with changelogs
- ✅ **Security scanning** with CodeQL
- ✅ **Dependency updates** via Dependabot

### 2. **Required Setup: NuGet API Key**

To enable automatic NuGet publishing, you need to add your API key:

1. **Get your NuGet API Key**:
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create" 
   - Name: `MonoGameLibrary`
   - Scopes: Select "Push new packages and package versions"
   - Glob Pattern: `MonoGameLibrary*`
   - Copy the generated key

2. **Add to GitHub Secrets**:
   - Go to your GitHub repo → Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Paste your API key
   - Click "Add secret"

### 3. **Your First Release**

```bash
# Create and push a version tag
git tag v1.0.1
git push origin v1.0.1
```

This will automatically:
- Build the library on all platforms
- Run tests
- Create NuGet package
- Publish to NuGet.org
- Create GitHub release with changelog

## 📋 Manual Workflow Execution

### **Package and Release** (Most Common)

1. Go to **Actions** tab → **Package and Release**
2. Click **"Run workflow"**
3. Configure:
   ```
   Version: 1.0.2
   Version suffix: (leave empty for stable)
   Publish to NuGet: true
   Create GitHub release: true
   Build configuration: Release
   Run tests: true
   ```
4. Click **"Run workflow"**

### **Build and Test** (For Testing)

1. Go to **Actions** tab → **Build and Test**  
2. Click **"Run workflow"**
3. Configure:
   ```
   Build configuration: Debug
   Run tests: true
   Target OS: windows-latest (or 'all')
   ```

### **Quick Test** (For Development)

1. Go to **Actions** tab → **Quick Test**
2. Click **"Run workflow"**
3. Configure:
   ```
   Test filter: (leave empty for all tests)
   Verbosity: normal
   Collect coverage: false
   ```

## 🔄 Development Workflows

### **Regular Development**
```bash
git commit -m "feat: add new sprite feature"
git push origin main
# → Triggers build and test automatically
```

### **Pre-Release Testing**
```bash
# Manual workflow: Package and Release
# Version: 1.1.0
# Version suffix: beta
# Publish to NuGet: true
# Create GitHub release: false
```

### **Hotfix Release**
```bash
git tag v1.0.3
git push origin v1.0.3
# → Full automated release pipeline
```

### **Development Testing**
```bash
# Use "Quick Test" workflow manually
# Test filter: "InputManager" 
# Verbosity: detailed
```

## 📦 Using the NuGet Package

Once published, users can install your library:

### **In a New Project**
```bash
dotnet new console -n MyGame
cd MyGame
dotnet add package MonoGameLibrary
```

### **Update Project File**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MonoGameLibrary" Version="1.0.1" />
  </ItemGroup>
</Project>
```

### **Create Standalone Executable**
```bash
# Self-contained (includes .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Framework-dependent (requires .NET on target machine)  
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## 🛠️ Troubleshooting

### **Workflow Failed to Publish to NuGet**
- Check that `NUGET_API_KEY` secret is properly set
- Ensure version number is unique (increment version)
- Verify package ID doesn't conflict with existing packages

### **Build Failed on Specific Platform**
- Check the build logs for platform-specific issues
- Use "Build and Test" workflow with single OS to isolate
- Most common: Path separator differences (use `Path.Combine`)

### **Tests Failed**
- Use "Quick Test" with detailed verbosity to debug
- Check for platform-specific test issues
- Verify test dependencies are included

### **Package Not Working in Consuming Project**
- Ensure dependencies are properly declared (no `PrivateAssets=All`)
- Check target framework compatibility
- Verify all required assemblies are included

## 📊 Monitoring

### **Build Status**
- Check Actions tab for build status
- Build badges can be added to README
- Email notifications for failed builds

### **Package Statistics**  
- View download stats on NuGet.org
- Monitor package usage and versions
- Track dependency usage

### **Security**
- CodeQL runs weekly for security analysis  
- Dependabot updates dependencies automatically
- Security advisories for vulnerabilities

## 🔧 Customization

### **Adding New Workflows**
Create `.github/workflows/new-workflow.yml`:

```yaml
name: Custom Workflow
on:
  workflow_dispatch:
    inputs:
      parameter:
        description: 'Description'
        required: true
        default: 'value'

jobs:
  custom:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    # Add your steps
```

### **Modifying Existing Workflows**
- Edit files in `.github/workflows/`
- Test with manual dispatch before committing
- Use draft releases for testing

### **Custom Package Sources**
For private/internal packages, modify the publish step:

```yaml
- name: Publish to Private Feed
  run: |
    dotnet nuget push ./packages/*.nupkg \
      --api-key ${{ secrets.PRIVATE_FEED_KEY }} \
      --source https://your-private-feed.com/v3/index.json
```

## 🎯 Best Practices

1. **Version Management**: Use semantic versioning (MAJOR.MINOR.PATCH)
2. **Testing**: Always run tests before releases
3. **Documentation**: Update README for breaking changes
4. **Changelogs**: Use meaningful commit messages for auto-generated changelogs
5. **Security**: Keep dependencies updated via Dependabot

This setup provides a complete, production-ready CI/CD pipeline for your MonoGame Library! 🎮