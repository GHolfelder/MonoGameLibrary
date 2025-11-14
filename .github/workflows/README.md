# GitHub Actions Workflows for MonoGame Library

This repository uses GitHub Actions to automate building, testing, and publishing the MonoGame Library.

## Workflows

### 🔧 Build and Test (`build.yml`)
- **Triggers**: Every push and pull request to main/develop branches, manual dispatch
- **Platforms**: Windows, Linux, macOS
- **Features**:
  - Multi-platform builds with .NET 9.0
  - Configurable build configuration (Release/Debug)
  - Optional test execution
  - NuGet package creation
  - Build artifact uploads

### 📦 Package and Release (`package.yml`)
- **Triggers**: Version tags (v*.*.*), manual dispatch
- **Features**:
  - Automated version calculation from git tags
  - Configurable version suffixes (alpha, beta, rc)
  - NuGet package publishing to NuGet.org
  - GitHub release creation with changelog
  - Manual control over all aspects

### 🔍 CodeQL Security Scan (`codeql.yml`)
- **Triggers**: Push/PR to main, weekly schedule (Mondays)
- **Features**:
  - Static security analysis
  - Vulnerability detection
  - Automated security advisories

### ⚡ Quick Test (`quick-test.yml`)
- **Triggers**: Manual dispatch only
- **Features**:
  - Fast test execution on Windows
  - Test filtering capabilities
  - Configurable verbosity levels
  - Optional code coverage collection

## Manual Workflow Execution

All workflows can be manually triggered from the GitHub Actions tab:

1. Go to **Actions** tab in GitHub
2. Select the workflow you want to run
3. Click **"Run workflow"** 
4. Configure parameters as needed
5. Click **"Run workflow"** to start

### Package and Release Parameters

- **Version**: Package version (e.g., 1.0.1)
- **Version Suffix**: Optional suffix (alpha, beta, rc1)
- **Publish to NuGet**: Whether to publish to NuGet.org
- **Create GitHub Release**: Whether to create a GitHub release
- **Build Configuration**: Release or Debug
- **Run Tests**: Whether to run tests before packaging

### Build and Test Parameters

- **Build Configuration**: Release or Debug
- **Run Tests**: Whether to execute tests
- **Target OS**: Single platform or all platforms

### Quick Test Parameters

- **Test Filter**: Filter specific tests (e.g., "TestClassName")
- **Verbosity**: Output detail level
- **Collect Coverage**: Generate code coverage reports

## Setup Requirements

### Required Secrets

Add these secrets in GitHub repository settings:

- `NUGET_API_KEY`: Your NuGet.org API key for package publishing
  - Create at: https://www.nuget.org/account/apikeys
  - Scope: "Push new packages and package versions"

### Creating Releases

**Automatic Release (Recommended):**
```bash
git tag v1.0.1
git push origin v1.0.1
```

**Manual Release:**
Use the "Package and Release" workflow with manual trigger.

## Workflow Features

✅ **Multi-platform Support**: Windows, Linux, macOS compatibility testing  
✅ **Flexible Versioning**: Automatic from git tags or manual specification  
✅ **Conditional Publishing**: Publish only when ready  
✅ **Security Scanning**: Weekly CodeQL analysis  
✅ **Dependency Management**: Dependabot for automated updates  
✅ **Comprehensive Testing**: Multiple test configurations  
✅ **Artifact Management**: Build outputs and coverage reports  
✅ **Release Automation**: Changelog generation and GitHub releases  

## Example Usage

### Development Workflow
```bash
# Regular development - triggers build and test
git commit -m "feat: add new feature"
git push origin main
```

### Release Workflow
```bash
# Create a release - triggers full package and publish pipeline
git tag v1.0.2
git push origin v1.0.2
```

### Testing Specific Changes
Use the "Quick Test" workflow manually with appropriate filters to test specific functionality without full builds.