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

Fluent Stopwatch is a cross-platform Uno Platform application built with .NET 10 that runs on Android, iOS, Desktop, WebAssembly, and WinUI. It provides stopwatch functionality with lap tracking, history, and customizable interface.

## Build and Development Commands

### Environment Setup
```powershell
# Install .NET 10 SDK (Windows - primary dev environment)
# Download from https://dotnet.microsoft.com/download/dotnet/10.0

# Install workloads
dotnet workload restore Stopwatch.slnx
dotnet workload install wasm-tools
```

### Build Commands
```powershell
# WebAssembly build (recommended for testing) - ~5 seconds
dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-browserwasm

# Desktop build - ~4 seconds
dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop

# DO NOT use full multi-target build - FAILS due to network restrictions blocking Android dependencies
# dotnet build src/Stopwatch/Stopwatch.csproj
```

### Code Formatting
```powershell
# Format code
dotnet format src/Stopwatch/Stopwatch.csproj

# Verify formatting before committing
dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes
```

### Testing in Browser (WebAssembly)
```powershell
cd src/Stopwatch/bin/Debug/net10.0-browserwasm/wwwroot
python -m http.server 8080
# Open http://localhost:8080/
```

### Validation Checklist
After making code changes, ALWAYS:
1. Build for WebAssembly: `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-browserwasm`
2. Check formatting: `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes`
3. No formal unit tests exist in this repository

## High-Level Architecture

### MVVM Pattern
- **Base Classes**: `PageViewModel` (abstract ViewModel base with lifecycle hooks) and `PageBase<TViewModel>` (View base that resolves ViewModels from DI)
- **Source Generators**: CommunityToolkit.Mvvm with `ObservableRecipient`, `ObservableProperty`, and `RelayCommand` attributes
- **Key ViewModels**: `MainViewModel` (orchestrates multiple stopwatches, manages tabs), `StopwatchViewModel` (wraps StopwatchModel via StopwatchService), `HistoryViewModel`, `SettingsViewModel`

### Service Layer Architecture

**Data Persistence** - Platform-Specific Strategy:
- **Interface**: `IDataSource` with two repositories: `Stopwatches` and `HistoryStopwatches`
- **LiteDB** (Desktop/WebAssembly/WinUI): `LiteDbDataSource` → `LiteDbRepository<T>` → `LocalFolder/Data/stopwatch.db`
- **File-based** (iOS/Android): `FileDataSource` → `FileRepository<T>` → JSON files in `LocalFolder/Data/`
- **Selection**: Conditional compilation (`#if __IOS__ || __ANDROID__`) in `App.xaml.cs` ConfigureServices

**Core Services**:
- `StopwatchService` - Business logic for start/stop/reset/lap
- `HistoryService` - Saving/loading/deleting stopwatch history
- `TimerFactory` - Creates DispatcherQueueTimer instances (50ms tick interval)
- `DisplayRequestManager` - Prevents screen sleep during active stopwatches

**Navigation**: Convention-based (`NavigationService` maps ViewModel name → View name). Shell flow: `App.xaml.cs` → `WindowShell` → `MainView` → other views

**Platform Services**: `IStoreService` (in-app purchases with iOS/Windows/Debug implementations), `IThemeManager`, `IImagePickerService`

### Data Flow Patterns

**Stopwatch State**: `StopwatchModel` (data) → `StopwatchService` (business logic) → `StopwatchViewModel` (UI logic) → `StopwatchDisplayControl` (view)

**Timer Updates**: `MainViewModel` creates DispatcherQueueTimer (50ms) → `OnTick()` → `StopwatchViewModel.OnTick()` updates observable properties → UI bindings update

**Persistence**: User action → `RelayCommand` → `StopwatchService` modifies model → `_dataSource.Stopwatches.Update()` → LiteDB/File repository persists

### Dependency Injection Scopes
- **Singleton (Host-level)**: `IDataSource`, `IHistoryService`, `IDisplayRequestManager`, `IPreferences`, `IAppPreferences`
- **Scoped (Window-level)**: All ViewModels, `INavigationService`, `IDialogService`, `IThemeManager`, `IStoreService`, `ITimerFactory`
- **WindowShell**: Creates scoped `ServiceProvider` for its window; ViewModels resolved from scoped provider when `PageBase` loads

### Platform Abstractions
- **Target frameworks**: net10.0-windows10.0.26100, net10.0-android, net10.0-ios, net10.0-desktop, net10.0-browserwasm
- **UnoSingleProject** mode with conditional compilation (`#if __IOS__`, `#if HAS_UNO`, etc.)
- **Platform Entry Points**: `Platforms/Desktop/Program.cs`, `Platforms/WebAssembly/Program.cs` (IDBFS enabled), `Platforms/Android/`, `Platforms/iOS/`

## Key Technologies

- **Uno Platform** SDK 6.5.0-dev.39 (via `src/global.json`)
- **.NET 10** with C# preview language features
- **LiteDB** 5.0.21 - Local database (Desktop/WebAssembly/WinUI only)
- **CommunityToolkit.WinUI** - Controls, converters, and helpers
- **MZikmund.Toolkit.WinUI** - Additional WinUI controls
- **Plugin.InAppBilling** - In-app purchases
- **Nerdbank.GitVersioning** - Version management

## Important Files

- `src/Stopwatch/Stopwatch.csproj` - Main project file (Uno.Sdk, UnoFeatures: Hosting, Mvvm, Localization, ThemeService, SkiaRenderer, Skia)
- `src/Stopwatch/App.xaml.cs` - DI container configuration and platform-specific service registration
- `src/Stopwatch/WindowShell.xaml.cs` - Creates scoped ServiceProvider per window
- `src/.editorconfig` - Code style: **tabs** for indentation, file-scoped namespaces, `_camelCase` for private fields
- `src/global.json` - Uno SDK version pin
- `src/Directory.Packages.props` - Centralized package version management
- `.github/workflows/ci.yml` - CI pipeline (Windows-only, MSBuild, .NET 10)

## Known Limitations

- **Full multi-target build FAILS** due to network restrictions blocking Android dependencies (dl.google.com). Always use `-f net10.0-browserwasm` or `-f net10.0-desktop`.
- **XBD001 errors** downloading from dl.google.com are expected. Use single target framework builds.
- **Package version constraint warnings** are normal and don't affect build success.
- **CI runs on Windows runners only** and uses MSBuild (`msbuild $env:PROJECT_FILE /r`).

## Contributing Guidelines

- Follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) for commit messages
- Run `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes` before committing
- Test WebAssembly build to verify UI loads correctly
