# Data Model: Pop Out Stopwatch to Secondary Window

## Existing Entities (No Changes)

### StopwatchModel
No schema changes. The popped-out state is transient (FR-012) and not persisted.

**Fields** (unchanged):
- `Id`: int (identity)
- `Name`: string
- `Icon`: string
- `InitialStartTime`: DateTimeOffset?
- `LastStartTime`: DateTimeOffset?
- `PausedElapsedTime`: TimeSpan
- `Laps`: LapModel[]
- `Theme`: ElementTheme
- `BackgroundImageUri`: string?
- `BackgroundImageOpacity`: double
- `BackgroundColor`: string

## Modified Entities

### StopwatchViewModel (transient state only)

**New property**:
- `IsPoppedOut`: bool (observable, default false) — Indicates this stopwatch is currently displayed in a secondary window. Not persisted. Drives tab disabled state in MainView.

## New Entities

### PopOutWindowInfo (runtime tracking only, not persisted)

Tracks an active secondary window for a popped-out stopwatch.

**Fields**:
- `StopwatchId`: int — The ID of the popped-out stopwatch
- `Window`: Window — Reference to the secondary window
- `WindowShell`: WindowShell — Reference to the secondary window's shell (holds the DI scope)

**Lifecycle**:
- Created when a stopwatch is popped out
- Destroyed when the secondary window is closed or the main window is closed
- Never persisted to storage

## State Transitions

```
Normal (in main window tab)
    ↓ User triggers pop-out
PoppedOut (IsPoppedOut = true, secondary window open, tab disabled)
    ↓ User closes secondary window OR main window closes
Normal (IsPoppedOut = false, tab re-enabled)
    ↓ App restart (if was popped out)
Normal (all stopwatches return to tabs, FR-012)
```

## Data Integrity

- No changes to LiteDB schema or file-based JSON format.
- No migration needed.
- The `IDataSource` singleton ensures both windows operate on the same `StopwatchModel` instances.
- `StopwatchService.CurrentTime` derives from `DateTimeOffset.UtcNow`, so both windows display identical elapsed time without synchronization.
