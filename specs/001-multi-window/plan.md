# Implementation Plan: Pop Out Stopwatch to Secondary Window

**Branch**: `001-multi-window` | **Date**: 2026-02-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-multi-window/spec.md`

## Summary

Allow users to pop out any stopwatch into a dedicated secondary window. The secondary window displays only that stopwatch (no tab bar) with full interaction support. The corresponding tab in the main window becomes disabled while popped out, and re-enables when the secondary window closes. The feature reuses the existing `WindowShell` scoped DI pattern — each secondary window gets its own DI scope, timer, and navigation. A singleton `IPopOutService` manages window lifecycle and inter-window communication. Desktop/WinUI only; gracefully absent on other platforms.

## Technical Context

**Language/Version**: C# preview / .NET 10 (pinned via `src/global.json`)
**Primary Dependencies**: Uno Platform SDK 6.5.0-dev.39, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection
**Storage**: No schema changes — `IDataSource` (LiteDB/File) unchanged. Popped-out state is transient (not persisted).
**Testing**: Manual verification via Desktop build. No unit test framework in place.
**Target Platform**: Desktop (`net10.0-desktop`) and WinUI (`net10.0-windows10.0.26100`). Feature hidden on mobile/WebAssembly.
**Project Type**: Single Uno Platform project (UnoSingleProject mode)
**Performance Goals**: 50ms timer tick maintained per window. No visible jank when creating/closing secondary windows.
**Constraints**: Each secondary window creates a DI scope + DispatcherQueueTimer. Memory overhead per window is minimal (one ViewModel + timer).
**Scale/Scope**: Typically 1-5 stopwatches, so 0-5 secondary windows. Not a scalability concern.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Cross-Platform Parity | PASS | Multi-window is a platform-exclusive feature (Desktop/WinUI). Constitution 1.2.0 explicitly permits this: "Platform-exclusive features (e.g., desktop multi-window) are permitted." Feature will be absent on unsupported platforms with no degraded UX. |
| II. MVVM Architecture | PASS | New ViewModel (`StopwatchWindowViewModel`) inherits `PageViewModel`. New View (`StopwatchWindowView`) inherits `PageBase<T>`. Business logic in services/ViewModels only. |
| III. Performance Standards | PASS | Each secondary window has its own 50ms timer on its own DispatcherQueue. No impact on main window performance. |
| IV. Simplicity & Data Integrity | PASS | No storage schema changes. No migration needed. Popped-out state is transient. `IPopOutService` is a simple singleton tracker — YAGNI compliant. |
| V. User Experience First | PASS | Pop-out button follows Fluent Design with AutomationProperties and x:Uid localization. Disabled tabs use built-in WinUI disabled styling. Secondary window is responsive (Viewbox scaling in StopwatchDisplayControl). |
| Build Gates | PASS | Desktop build gate applies. No new dependencies required. |
| Commit Conventions | PASS | Granular commits planned per logical unit. |

### Post-Design Re-Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Cross-Platform Parity | PASS | No conditional compilation needed in shared code paths. Pop-out button visibility controlled by runtime platform check. |
| II. MVVM Architecture | PASS | Export commands moved from MainViewModel to StopwatchViewModel — improves separation. New StopwatchWindowViewModel follows existing patterns. |
| III. Performance Standards | PASS | Secondary window creation is lightweight (DI scope + Window). No startup time impact. |
| IV. Simplicity & Data Integrity | PASS | 3 new files + 5 modified files. No new abstractions beyond `IPopOutService`. No data model changes. |
| V. User Experience First | PASS | StopwatchDisplayControl reused in secondary window — identical UX. Responsive layout preserved. AutomationProperties and x:Uid localization included for new UI elements. Visual verification via uno-app MCP planned. |

## Project Structure

### Documentation (this feature)

```text
specs/001-multi-window/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research decisions
├── data-model.md        # Phase 1: data model (no schema changes)
├── quickstart.md        # Phase 1: build and test guide
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stopwatch/
├── Services/
│   └── PopOut/
│       ├── IPopOutService.cs          # NEW: Interface for secondary window management
│       └── PopOutService.cs           # NEW: Singleton — create, track, close secondary windows
├── ViewModels/
│   ├── MainViewModel.cs               # MODIFIED: Add PopOutStopwatchCommand, subscribe to PopOutService
│   ├── StopwatchViewModel.cs          # MODIFIED: Add IsPoppedOut observable property
│   └── StopwatchWindowViewModel.cs    # NEW: ViewModel for secondary window (single stopwatch + timer)
├── Views/
│   ├── MainView.xaml                  # MODIFIED: Pop-out button on tabs, disabled tab styling
│   ├── MainView.xaml.cs               # MODIFIED: Pop-out button click handler (if needed)
│   ├── StopwatchWindowView.xaml       # NEW: Secondary window view (StopwatchDisplayControl without tabs)
│   └── StopwatchWindowView.xaml.cs    # NEW: Code-behind for secondary window view
├── Controls/
│   ├── StopwatchDisplayControl.xaml   # MODIFIED: Export bindings use Stopwatch instead of MainViewModel
│   └── StopwatchDisplayControl.xaml.cs # MODIFIED: Remove MainViewModel dependency property
└── App.xaml.cs                        # MODIFIED: Register IPopOutService as singleton, StopwatchWindowViewModel as scoped
```

**Structure Decision**: All changes are within the existing single-project structure (`src/Stopwatch/`). New service goes in `Services/PopOut/` following the existing service subdirectory convention (e.g., `Services/Timer/`, `Services/DisplayRequest/`). New view/ViewModel follow existing naming conventions.

## Design Details

### Component Interaction

```
User clicks pop-out button on tab
    ↓
MainViewModel.PopOutStopwatchCommand(StopwatchViewModel)
    ↓
IPopOutService.PopOut(stopwatchId, hostServices)
    ├─ Creates new Window
    ├─ Creates new WindowShell(hostServices, newWindow) → new DI scope
    ├─ Navigates to StopwatchWindowView with stopwatchId parameter
    ├─ Activates window
    └─ Tracks PopOutWindowInfo { StopwatchId, Window, WindowShell }
    ↓
StopwatchViewModel.IsPoppedOut = true
    ↓
MainView TabViewItem IsEnabled binds to !IsPoppedOut → tab disables
```

```
User closes secondary window
    ↓
Window.Closed event fires
    ↓
PopOutService handles cleanup
    ├─ Removes PopOutWindowInfo tracking
    ├─ Disposes the DI scope
    └─ Raises StopwatchReturned(stopwatchId) event
    ↓
MainViewModel receives StopwatchReturned event
    ↓
StopwatchViewModel.IsPoppedOut = false → tab re-enables
```

### Key Implementation Notes

1. **StopwatchDisplayControl refactor**: Move `ExportToCsvCommand` and `ExportToJsonCommand` from `MainViewModel` to `StopwatchViewModel`. Remove the `MainViewModel` dependency property. Update XAML bindings from `MainViewModel.ExportToCsvCommand` to `Stopwatch.ExportToCsvCommand`. This enables the control to be reused in the secondary window without a MainViewModel.

2. **StopwatchWindowViewModel**: Inherits `PageViewModel` per constitution principle II. `StopwatchWindowView` inherits `PageBase<StopwatchWindowViewModel>` and resolves the ViewModel via DI. The ViewModel:
   - Is registered as scoped in DI (one per window)
   - Receives stopwatch ID via navigation parameter (passed through `Frame.Navigate`)
   - Resolves `StopwatchModel` from `IDataSource` using the navigation parameter
   - Creates a `StopwatchViewModel` wrapping that model
   - Creates and manages a 50ms `DispatcherQueueTimer` via scoped `ITimerFactory`
   - Uses `ViewLoaded()`/`ViewUnloaded()` lifecycle hooks to start/stop the timer
   - Calls `StopwatchViewModel.OnTick()` on each tick

3. **Platform check**: Use `RuntimeInformation` or Uno Platform's target detection to determine if multi-window is supported. The pop-out button is hidden (not disabled) on unsupported platforms.

4. **Main window close**: `PopOutService` subscribes to the main window's `Closed` event and closes all tracked secondary windows.

5. **Timer lifecycle**: `StopwatchWindowViewModel` uses `PageViewModel.ViewLoaded()` to start its timer and `ViewUnloaded()` to stop it, mirroring `MainViewModel`'s pattern exactly.

6. **Tab selection**: When a popped-out tab is the currently selected tab, selection should move to the next available non-popped-out tab. If all tabs are popped out, `SelectedStopwatch` becomes null and the main window shows an empty state.

## Complexity Tracking

No constitution violations. All design decisions use existing patterns and minimal new abstractions.
