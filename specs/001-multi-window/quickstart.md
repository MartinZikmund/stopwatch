# Quickstart: Pop Out Stopwatch to Secondary Window

## Prerequisites

- .NET 10 SDK
- Workloads restored: `dotnet workload restore src/Stopwatch.slnx`

## Build

```powershell
# Desktop build (primary target for this feature)
dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop

# Verify formatting
dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes
```

## Test the Feature

1. Build and run the Desktop target
2. Create two or more stopwatches using the "+" tab button
3. Click the pop-out button on a stopwatch tab
4. Verify: secondary window opens with only that stopwatch
5. Verify: the tab in the main window is disabled
6. Start/stop/lap/reset the stopwatch in the secondary window
7. Close the secondary window
8. Verify: the tab in the main window re-enables with preserved state

## Key Files

### New Files
| File | Purpose |
|------|---------|
| `Services/PopOut/IPopOutService.cs` | Interface for secondary window management |
| `Services/PopOut/PopOutService.cs` | Singleton service: creates, tracks, and cleans up secondary windows |
| `ViewModels/StopwatchWindowViewModel.cs` | ViewModel for secondary window (single stopwatch + timer) |
| `Views/StopwatchWindowView.xaml` | View for secondary window (StopwatchDisplayControl without tabs) |
| `Views/StopwatchWindowView.xaml.cs` | Code-behind for secondary window view |

### Modified Files
| File | Change |
|------|--------|
| `ViewModels/StopwatchViewModel.cs` | Add `IsPoppedOut` observable property |
| `ViewModels/MainViewModel.cs` | Add `PopOutStopwatchCommand`, subscribe to PopOutService events |
| `Views/MainView.xaml` | Add pop-out button to tab template, disabled tab styling |
| `Controls/StopwatchDisplayControl.xaml` | Move export command bindings from MainViewModel to Stopwatch |
| `Controls/StopwatchDisplayControl.xaml.cs` | Remove MainViewModel dependency property |
| `App.xaml.cs` | Register `IPopOutService` as singleton |

## Architecture Overview

```
Main Window                              Secondary Window
┌─────────────────────┐                  ┌─────────────────────┐
│ WindowShell (scope)  │                  │ WindowShell (scope)  │
│ ├─ MainViewModel     │   IPopOutService │ ├─ StopwatchWindow   │
│ │  ├─ Stopwatches[]  │◄──(singleton)───►│ │  ViewModel         │
│ │  ├─ IsPoppedOut    │   events/track   │ │  ├─ StopwatchVM    │
│ │  └─ Timer (50ms)   │                  │ │  └─ Timer (50ms)   │
│ ├─ NavigationService │                  │ ├─ NavigationService │
│ └─ TimerFactory      │                  │ └─ TimerFactory      │
└─────────────────────┘                  └─────────────────────┘
         │                                        │
         └──────────── IDataSource ───────────────┘
                      (singleton)
              Same StopwatchModel by ID
```
