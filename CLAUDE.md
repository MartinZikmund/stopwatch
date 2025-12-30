# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

<!-- BACKLOG.MD MCP GUIDELINES START -->

<CRITICAL_INSTRUCTION>

## BACKLOG WORKFLOW INSTRUCTIONS

This project uses Backlog.md MCP for all task and project management activities.

**CRITICAL GUIDANCE**

- If your client supports MCP resources, read `backlog://workflow/overview` to understand when and how to use Backlog for this project.
- If your client only supports tools or the above request fails, call `backlog.get_workflow_overview()` tool to load the tool-oriented overview (it lists the matching guide tools).

- **First time working here?** Read the overview resource IMMEDIATELY to learn the workflow
- **Already familiar?** You should have the overview cached ("## Backlog.md Overview (MCP)")
- **When to read it**: BEFORE creating tasks, or when you're unsure whether to track work

These guides cover:
- Decision framework for when to create tasks
- Search-first workflow to avoid duplicates
- Links to detailed guides for task creation, execution, and completion
- MCP tools reference

You MUST read the overview resource to understand the complete workflow. The information is NOT summarized here.

</CRITICAL_INSTRUCTION>

<!-- BACKLOG.MD MCP GUIDELINES END -->

## Project Overview

Fluent Stopwatch is a cross-platform Uno Platform application built with .NET 9.0 that runs on Android, iOS, Desktop, WebAssembly, and WinUI. It provides stopwatch functionality with lap tracking, history, and customizable interface.

## Build and Development Commands

### Environment Setup
```bash
# Install .NET 9.0 SDK
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 9.0
export PATH="$HOME/.dotnet:$PATH"

# Install uno-check tool
dotnet tool install --global --version 1.28.3 uno.check

# Install workloads (takes 6-7 minutes - NEVER CANCEL, set timeout to 15+ minutes)
~/.dotnet/tools/uno-check --ci --fix --non-interactive --verbose --skip openjdk
```

### Build Commands
```bash
# WebAssembly build (recommended for testing) - 5 seconds
dotnet build src/Stopwatch/Stopwatch.csproj -f net9.0-browserwasm

# Desktop build - 4 seconds
dotnet build src/Stopwatch/Stopwatch.csproj -f net9.0-desktop

# DO NOT use full multi-target build - FAILS due to network restrictions blocking Android dependencies
# dotnet build src/Stopwatch/Stopwatch.csproj
```

### Code Formatting
```bash
# Format code
dotnet format src/Stopwatch/Stopwatch.csproj

# Verify formatting before committing
dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes
```

### Testing in Browser (WebAssembly)
```bash
cd src/Stopwatch/bin/Debug/net9.0-browserwasm/wwwroot
python3 -m http.server 8080
# Open http://localhost:8080/
```

### Validation Checklist
After making code changes, ALWAYS:
1. Build for WebAssembly: `dotnet build src/Stopwatch/Stopwatch.csproj -f net9.0-browserwasm`
2. Test in browser by serving the WebAssembly output and verifying the stopwatch UI loads
3. Check formatting: `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes`
4. Test basic stopwatch functionality: play button, timer display, lap recording

## High-Level Architecture

### MVVM Pattern
- **Base Classes**:
  - `PageViewModel` - Abstract base for ViewModels providing navigation and lifecycle hooks (ViewCreated, ViewLoading, ViewLoaded, ViewUnloaded, ViewNavigatedTo)
  - `PageBase<TViewModel>` - Base for Views that automatically resolves ViewModels from DI and manages lifecycle coordination
- **Source Generators**: Uses CommunityToolkit.Mvvm with `ObservableRecipient`, `ObservableProperty`, and `RelayCommand` attributes
- **Key ViewModels**:
  - `MainViewModel` - Orchestrates multiple stopwatches, manages tabs, handles pro features
  - `StopwatchViewModel` - Wraps StopwatchModel, handles start/stop/lap/reset commands
  - `HistoryViewModel` - Manages history entries display and deletion
  - `SettingsViewModel` - App settings management

### Service Layer Architecture

**Data Persistence** - Platform-Specific Strategy:
- **Interface**: `IDataSource` with two repositories: `Stopwatches` and `HistoryStopwatches`
- **LiteDB Implementation** (Desktop/WebAssembly/WinUI):
  - `LiteDbDataSource` - Creates database in LocalFolder/Data/stopwatch.db
  - `LiteDbRepository<T>` - Generic repository using LiteDB collections
- **File-based Implementation** (iOS/Android):
  - `FileDataSource` - Uses JSON files in LocalFolder/Data/
  - `FileRepository<T>` - Generic JSON file-based repository
- **Selection**: Platform-specific implementation chosen at DI registration using conditional compilation

**Core Services**:
- `StopwatchService` - Business logic for start/stop/reset/lap operations
- `HistoryService` - Manages saving/loading/deleting stopwatch history
- `ITimerFactory`/`TimerFactory` - Creates DispatcherQueueTimer instances for UI updates (50ms tick interval)
- `DisplayRequestManager` - Prevents screen sleep during active stopwatches

**Navigation**:
- `NavigationService` - Convention-based navigation (ViewModel → View name resolution)
- `IFrameProvider`/`FrameProvider` - Provides Frame for current scope
- Shell-based navigation: `App.xaml.cs` → `WindowShell` → `MainView` (default) → other views

**Platform Services**:
- `IStoreService` - In-app purchase abstraction with platform-specific implementations (StoreService.iOS.cs, StoreService.Windows.cs, FakeStoreService for debug)
- `IThemeManager`/`ThemeManager` - Manages app theme and title bar theming
- `IImagePickerService`/`ImagePickerService` - Background image selection

### Data Flow Patterns

**Stopwatch State Flow**:
```
StopwatchModel (data)
  ↓ (wrapped by)
StopwatchService (business logic)
  ↓ (wrapped by)
StopwatchViewModel (UI logic)
  ↓ (rendered by)
MainView → StopwatchDisplayControl
```

**Timer Update Flow**:
```
MainViewModel creates DispatcherQueueTimer (50ms interval)
  ↓ (on tick)
MainViewModel.OnTick() → SelectedStopwatch?.OnTick()
  ↓
StopwatchViewModel.OnTick() updates properties
  ↓ (via ObservableProperty)
UI automatically updates via data binding
```

**Persistence Flow**:
```
User Action (Start/Stop/Lap)
  ↓
StopwatchViewModel.RelayCommand
  ↓
StopwatchService modifies StopwatchModel
  ↓
_dataSource.Stopwatches.Update(stopwatch)
  ↓
LiteDbRepository/FileRepository persists to disk
```

### Dependency Injection Scopes
- **Host-level (App.xaml.cs)**: Singleton services (IDataSource, IHistoryService, IDisplayRequestManager, IPreferences, IAppPreferences)
- **Window-level**: Scoped services per window (ViewModels, Navigation, Dialogs, ThemeManager, StoreService)
- **WindowShell**: Creates scoped ServiceProvider for its window
- **ViewModels**: Resolved from scoped provider when PageBase loads

### Platform Abstractions
- **Single codebase** targets: net9.0-windows10.0.26100, net9.0-android, net9.0-ios, net9.0-desktop, net9.0-browserwasm
- **UnoSingleProject** mode with conditional compilation
- **Platform Entry Points**:
  - Desktop: `Platforms/Desktop/Program.cs`
  - WebAssembly: `Platforms/WebAssembly/Program.cs` (IDBFS enabled)
  - Android: `Platforms/Android/Main.Android.cs` and `MainActivity.Android.cs`
  - iOS: `Platforms/iOS/Main.iOS.cs`

### Key Architectural Patterns
1. **Repository Pattern** - IRepository<T>/IStopwatchRepository with platform-specific implementations
2. **Service Locator** - ServiceProvider accessed via WindowShell
3. **MVVM with Commands** - CommunityToolkit.Mvvm source generators
4. **Dependency Injection** - Microsoft.Extensions.DependencyInjection with Uno.Extensions.Hosting
5. **Convention-based Navigation** - ViewModel name → View name mapping
6. **Strategy Pattern** - Platform-specific IDataSource implementations selected at DI registration

## Key Technologies

- **Uno Platform** SDK 6.2.0-dev.81 - Cross-platform UI framework
- **LiteDB** - Local database storage (Desktop/WebAssembly/WinUI)
- **CommunityToolkit.WinUI** - Controls, converters, and helpers
- **MZikmund.Toolkit.WinUI** - Additional WinUI controls
- **SkiaSharp** - Graphics rendering
- **Plugin.InAppBilling** - In-app purchases

## Project Structure

```
src/
├── Stopwatch/
│   ├── Stopwatch.csproj          # Main Uno Platform project
│   ├── App.xaml                  # Application entry point and DI configuration
│   ├── WindowShell.xaml          # Main window shell with navigation
│   ├── Views/                    # XAML pages and controls
│   ├── ViewModels/               # MVVM view models
│   ├── Services/                 # Application services
│   ├── Models/                   # Data models
│   ├── Platforms/                # Platform-specific code and entry points
│   └── Properties/               # Project properties
├── Stopwatch.SourceGenerators/   # Source generators
├── Stopwatch.slnx                # Solution file
├── Directory.Build.props         # Shared build properties
└── Directory.Packages.props      # Centralized package management
```

## Important Files

- `src/Stopwatch/Stopwatch.csproj` - Main project targeting multiple platforms
- `src/.editorconfig` - Code style rules (tabs, C# conventions)
- `src/Stopwatch/App.xaml.cs` - DI container configuration and platform-specific service registration
- `src/Stopwatch/WindowShell.xaml.cs` - Creates scoped ServiceProvider per window
- `.github/workflows/ci.yml` - CI pipeline (Windows-only, uses MSBuild)

## Build Timing and Known Limitations

- **uno-check workload installation**: 6-7 minutes (NEVER CANCEL - set timeout to 15+ minutes)
- **WebAssembly build**: ~5 seconds
- **Desktop build**: ~4 seconds
- **Full multi-target build**: FAILS due to network restrictions blocking Android dependencies from dl.google.com
- **No formal unit tests** exist in this repository
- **CI runs on Windows runners only** and uses MSBuild

## Common Errors

- **XBD001 errors** downloading from dl.google.com: Expected due to network restrictions. Use single target framework builds instead.
- **Package version constraint warnings**: Normal, do not affect build success.
- **Formatting violations**: Run `dotnet format` to fix whitespace and import ordering issues.

## Contributing Guidelines

- Follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification for commit messages
- Create descriptive branch names (e.g., `feature-name`, `bugfix-name`)
- Ensure code follows the project's coding standards in `.editorconfig`
- Always run formatting checks before committing
- Test WebAssembly build in browser to verify UI functionality
