# Research: Pop Out Stopwatch to Secondary Window

## Decision 1: Secondary Window Architecture

**Decision**: Reuse the existing `WindowShell` + scoped DI pattern to create secondary windows. Each secondary window gets its own `Window`, `WindowShell`, and `IServiceScope`.

**Rationale**: The codebase already has a well-designed per-window scoped DI pattern. `WindowShell` creates an `IServiceScope` from the host services, gives each window its own `IWindowShellProvider`, `ITimerFactory`, `INavigationService`, etc. This pattern can be directly reused for secondary windows with no architectural changes.

**Alternatives considered**:
- Custom lightweight window without DI scope: Rejected — would lose timer, navigation, and dialog support needed for full stopwatch interaction.
- Sharing MainViewModel across windows: Rejected — MainViewModel manages tabs and multiple stopwatches, which is unnecessary for a single-stopwatch secondary window.

## Decision 2: Stopwatch Sharing Strategy

**Decision**: The secondary window creates its own `StopwatchViewModel` wrapping the same `StopwatchModel` (by ID from shared `IDataSource`). Both windows independently read/write the same data through the singleton `IDataSource`.

**Rationale**:
- `StopwatchService.CurrentTime` is computed from `DateTimeOffset.UtcNow` — both windows show identical time without synchronization.
- `IDataSource` is a singleton; all mutations (`Start`, `Stop`, `Reset`, `AddLap`) persist immediately.
- Each window needs its own `DispatcherQueueTimer` on its own `DispatcherQueue`, so each window needs its own `StopwatchViewModel.OnTick()` cycle.

**Alternatives considered**:
- Sharing a single StopwatchViewModel reference across windows: Rejected — UI binding updates must fire on each window's dispatcher thread. A shared ViewModel would fire PropertyChanged on only one dispatcher.
- Event-based synchronization between ViewModels: Rejected — unnecessary complexity. The shared StopwatchModel reference already ensures state consistency.

## Decision 3: Inter-Window Communication

**Decision**: Use a singleton `IPopOutService` that tracks popped-out stopwatches and raises events when secondary windows close.

**Rationale**: The main window's `MainViewModel` needs to know when a secondary window closes to re-enable the tab. A singleton service is the simplest communication mechanism that works across window scopes.

**Alternatives considered**:
- Messenger/EventAggregator pattern (CommunityToolkit.Mvvm `IMessenger`): Viable but heavier than needed for a simple "window closed" signal.
- Direct callback/delegate from secondary window to MainViewModel: Rejected — creates tight coupling between window-scoped instances.

## Decision 4: StopwatchDisplayControl Dependency on MainViewModel

**Decision**: Move export commands (`ExportToCsvCommand`, `ExportToJsonCommand`) from `MainViewModel` to `StopwatchViewModel`, then remove the `MainViewModel` dependency property from `StopwatchDisplayControl`.

**Rationale**: `StopwatchDisplayControl` currently requires a `MainViewModel` reference solely for two export commands. Moving these to `StopwatchViewModel` eliminates the cross-ViewModel dependency and allows `StopwatchDisplayControl` to be reused in any context (main window, secondary window) with only a `StopwatchViewModel`.

**Alternatives considered**:
- Extract an interface for export commands: Adds abstraction complexity for just two commands.
- Duplicate the display control for secondary windows: Code duplication violates DRY.
- Pass null MainViewModel and hide export in secondary window: Reduces secondary window functionality without reason.

## Decision 5: Platform Availability

**Decision**: Pop-out is available on Desktop and WinUI only. Use runtime platform check (not conditional compilation) to show/hide the pop-out button.

**Rationale**:
- Mobile (iOS/Android) and WebAssembly do not support multiple windows.
- A runtime check keeps the code unified and avoids `#if` fragmentation for UI elements.
- The constitution explicitly permits platform-exclusive features that "gracefully degrade or simply be absent on unsupported platforms."

**Alternatives considered**:
- Conditional compilation (`#if`): Works but fragments the XAML/ViewModel for a single button visibility toggle.
- Always show button with error message on unsupported platforms: Poor UX per constitution principle V.

## Decision 6: Tab Disabled State

**Decision**: Add an `IsPoppedOut` observable property to `StopwatchViewModel`. The `TabViewItem` template binds `IsEnabled` to `!IsPoppedOut`. The disabled tab remains visible but non-selectable and visually dimmed.

**Rationale**: `IsPoppedOut` is a transient UI state (not persisted per FR-012). Binding `IsEnabled` to the ViewModel property keeps the logic in the ViewModel per MVVM principles and allows the TabView's built-in disabled styling to handle the visual state.

**Alternatives considered**:
- Remove the tab from the collection and re-add on close: Loses tab position and complicates the lifecycle.
- Custom visual state with overlay: More complex than needed when `IsEnabled=false` provides the built-in behavior.

## Decision 7: Secondary Window View

**Decision**: Create a dedicated `StopwatchWindowView` page that contains a `StopwatchDisplayControl` and a `StopwatchWindowViewModel` that manages the single stopwatch and timer.

**Rationale**: A dedicated page+ViewModel allows the secondary window to navigate to it via the existing `NavigationService` pattern. `StopwatchWindowViewModel` handles the timer lifecycle and resolves the stopwatch from the shared `IDataSource`.

**Alternatives considered**:
- Reuse MainView without TabView: MainView has too much tab-specific logic and layout.
- Set Window.Content directly to StopwatchDisplayControl: Loses navigation, title bar, and lifecycle management.
