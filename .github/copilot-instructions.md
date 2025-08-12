# Fluent Stopwatch
Fluent Stopwatch is a cross-platform Uno Platform application built with .NET 9.0 that runs on Android, iOS, Desktop, WebAssembly, and WinUI. It provides stopwatch functionality with lap tracking, history, and customizable interface.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively
- Bootstrap, build, and test the repository:
  - Install .NET 9.0 SDK: `curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0`
  - Export PATH: `export PATH="$HOME/.dotnet:$PATH"`
  - Install uno-check tool: `dotnet tool install --global --version 1.28.3 uno.check`
  - Install workloads: `~/.dotnet/tools/uno-check --ci --fix --non-interactive --verbose --skip openjdk` -- takes 6-7 minutes. NEVER CANCEL. Set timeout to 15+ minutes.
- Build commands:
  - WebAssembly (works): `dotnet build src/Stopwatch/Stopwatch.csproj -f net9.0-browserwasm` -- takes 5 seconds
  - Desktop (works): `dotnet build src/Stopwatch/Stopwatch.csproj -f net9.0-desktop` -- takes 4 seconds
  - DO NOT attempt full multi-target build: `dotnet build src/Stopwatch/Stopwatch.csproj` -- FAILS due to network restrictions blocking Android dependencies from dl.google.com
- No formal unit tests exist in this repository.
- Code formatting: `dotnet format src/Stopwatch/Stopwatch.csproj` -- the code currently has formatting violations but the command works

## Validation
- Build for specific target frameworks only (WebAssembly or Desktop) to avoid network dependency issues.
- ALWAYS test the WebAssembly build by running it in browser:
  - Navigate to: `cd src/Stopwatch/bin/Debug/net9.0-browserwasm/wwwroot`
  - Start server: `python3 -m http.server 8080`
  - Open: `http://localhost:8080/`
  - Verify the stopwatch interface loads and displays "00:00:00.00"
- You can build and run the Desktop version but cannot interact with its UI in this environment.
- ALWAYS run `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes` to check for formatting issues before committing.

## Common tasks
The following are outputs from frequently run commands. Reference them instead of viewing, searching, or running bash commands to save time.

### Project Structure
```
src/
├── Stopwatch/
│   ├── Stopwatch.csproj          # Main Uno Platform project
│   ├── App.xaml                  # Application entry point
│   ├── WindowShell.xaml          # Main window shell
│   ├── Assets/                   # Images and resources
│   ├── Views/                    # XAML pages and controls
│   ├── ViewModels/               # MVVM view models
│   ├── Services/                 # Application services
│   ├── Models/                   # Data models
│   ├── Platforms/                # Platform-specific code
│   └── Properties/               # Project properties
├── Stopwatch.slnx               # Solution file
├── Directory.Build.props        # Shared build properties
├── Directory.Packages.props     # Centralized package management
└── global.json                  # .NET SDK version
```

### Key Files
- `src/Stopwatch/Stopwatch.csproj` - Main project targeting multiple platforms
- `src/.editorconfig` - Code style rules (tabs, C# conventions)
- `.github/workflows/ci.yml` - CI pipeline (Windows-only, uses MSBuild)
- `.github/workflows/package-windows.yml` - Windows packaging workflow

### Target Frameworks
- `net9.0-windows10.0.26100` - Windows (requires Windows SDK)
- `net9.0-android` - Android (FAILS: network restrictions)
- `net9.0-ios` - iOS 
- `net9.0-maccatalyst` - macOS Catalyst
- `net9.0-desktop` - Cross-platform desktop
- `net9.0-browserwasm` - WebAssembly

### Dependencies
- Uno Platform SDK 6.2.0-dev.81
- CommunityToolkit.WinUI packages for controls and converters
- LiteDB for local data storage
- SkiaSharp for graphics rendering
- MZikmund.Toolkit.WinUI for additional controls

### Build Timing Expectations
- uno-check workload installation: 6-7 minutes (NEVER CANCEL - timeout 15+ minutes)
- WebAssembly build: 5 seconds
- Desktop build: 4 seconds
- Full multi-target build: FAILS due to network restrictions

### Common Errors and Solutions
- XBD001 errors downloading from dl.google.com: Expected due to network restrictions. Use single target framework builds instead.
- Package version constraint warnings: Normal, do not affect build success.
- Formatting violations: Run `dotnet format` to fix whitespace and import ordering issues.

### Validation Scenarios
After making code changes, ALWAYS:
1. Build for WebAssembly: `dotnet build src/Stopwatch/Stopwatch.csproj -f net9.0-browserwasm`
2. Test in browser by serving the WebAssembly output and verifying the stopwatch UI loads
3. Check formatting: `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes`
4. Test basic stopwatch functionality: play button, timer display, lap recording

### CI/CD Information
- CI runs on Windows runners only
- Uses MSBuild for building: `msbuild ./src/Stopwatch/Stopwatch.csproj /r`
- Packaging creates MSIX packages for Windows Store distribution
- Build timeout set to 60 minutes in CI workflows