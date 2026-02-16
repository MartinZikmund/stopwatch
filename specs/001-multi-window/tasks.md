# Tasks: Pop Out Stopwatch to Secondary Window

**Input**: Design documents from `/specs/001-multi-window/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: No automated tests (no test framework in place). Manual verification via Desktop build.

**Organization**: Tasks are grouped by user story. US1 and US2 are in the same phase because they form a single lifecycle (pop-out requires close, return requires pop-out).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/Stopwatch/` at repository root
- All paths relative to repository root

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: Refactor StopwatchDisplayControl to remove MainViewModel dependency, create the PopOutService infrastructure, and register in DI. These changes MUST complete before any user story work.

**Why blocking**: StopwatchDisplayControl is reused in the secondary window (US1) and must not depend on MainViewModel. IPopOutService is needed for pop-out (US1) and return (US2).

- [X] T001 Move ExportToCsvCommand and ExportToJsonCommand from MainViewModel to StopwatchViewModel — cut the two RelayCommand methods and their helpers from src/Stopwatch/ViewModels/MainViewModel.cs and add them to src/Stopwatch/ViewModels/StopwatchViewModel.cs, adjusting references from `SelectedStopwatch` to `this`
- [X] T002 Remove MainViewModel dependency property from StopwatchDisplayControl — delete the MainViewModel DependencyProperty and its backing field from src/Stopwatch/Controls/StopwatchDisplayControl.xaml.cs, then update export command XAML bindings from `MainViewModel.ExportToCsvCommand` to `Stopwatch.ExportToCsvCommand` (and same for JSON) in src/Stopwatch/Controls/StopwatchDisplayControl.xaml
- [X] T003 Remove MainViewModel property binding from StopwatchDisplayControl usage in src/Stopwatch/Views/MainView.xaml — delete the `MainViewModel="{x:Bind ViewModel}"` attribute from the StopwatchDisplayControl element
- [X] T004 Add IsPoppedOut observable property (default false) to StopwatchViewModel using [ObservableProperty] attribute in src/Stopwatch/ViewModels/StopwatchViewModel.cs
- [X] T005 [P] Create IPopOutService interface in src/Stopwatch/Services/PopOut/IPopOutService.cs — define methods: `void PopOut(int stopwatchId, IServiceProvider hostServices)`, `void CloseAll()`, `bool IsPoppedOut(int stopwatchId)`, and event `Action<int>? StopwatchReturned`
- [X] T006 [P] Create PopOutService singleton implementing IPopOutService in src/Stopwatch/Services/PopOut/PopOutService.cs — implement window creation (new Window + new WindowShell with scoped DI), PopOutWindowInfo tracking dictionary (stopwatchId → Window/WindowShell), Window.Closed event subscription for cleanup, and navigation to StopwatchWindowView with stopwatchId parameter
- [X] T007 Register IPopOutService as singleton and StopwatchWindowViewModel as scoped in DI container in src/Stopwatch/App.xaml.cs — add `services.AddSingleton<IPopOutService, PopOutService>()` and `services.AddScoped<StopwatchWindowViewModel>()` in the ConfigureServices method
- [X] T008 Verify foundational changes compile — run `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop` and `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes`

**Checkpoint**: Foundation ready — StopwatchDisplayControl is reusable without MainViewModel, PopOutService infrastructure exists, build passes

---

## Phase 2: User Story 1 + User Story 2 — Pop Out & Return (Priority: P1) MVP

**Goal**: Users can pop out any stopwatch to a secondary window and return it by closing that window. The main window tab disables while popped out and re-enables on return.

**Independent Test**: Create 2+ stopwatches → start one → pop it out → verify secondary window shows running stopwatch → verify main window tab is disabled → close secondary window → verify tab re-enables with correct state

### Implementation

- [X] T009 [P] [US1] Create StopwatchWindowViewModel inheriting PageViewModel in src/Stopwatch/ViewModels/StopwatchWindowViewModel.cs — registered as scoped in DI, receives stopwatchId via navigation parameter, resolves StopwatchModel from IDataSource, creates StopwatchViewModel wrapping it, creates 50ms DispatcherQueueTimer via ITimerFactory, overrides ViewLoaded() to start timer and ViewUnloaded() to stop timer, implements OnTick calling StopwatchViewModel.OnTick()
- [X] T010 [P] [US1] Create StopwatchWindowView inheriting PageBase<StopwatchWindowViewModel> in src/Stopwatch/Views/StopwatchWindowView.xaml and src/Stopwatch/Views/StopwatchWindowView.xaml.cs — Page containing a StopwatchDisplayControl bound to ViewModel.Stopwatch property, no TabView or tab management UI, ViewModel resolved via DI through PageBase pattern
- [X] T011 [US1] Add pop-out button to stopwatch tab template in src/Stopwatch/Views/MainView.xaml — add a button with a pop-out icon (e.g., OpenInNewWindow glyph) in the TabViewItem template or adjacent to the tab area, with platform visibility (hidden on mobile/WebAssembly via runtime check), bound to MainViewModel.PopOutStopwatchCommand with the StopwatchViewModel as CommandParameter
- [X] T012 [US1] Implement PopOutStopwatchCommand in MainViewModel in src/Stopwatch/ViewModels/MainViewModel.cs — RelayCommand<StopwatchViewModel> that calls IPopOutService.PopOut with the stopwatch ID and Host.Services, then sets viewModel.IsPoppedOut = true, inject IPopOutService via constructor
- [X] T013 [US1] Add disabled tab styling for popped-out stopwatches in src/Stopwatch/Views/MainView.xaml — bind TabViewItem.IsEnabled to the inverse of StopwatchViewModel.IsPoppedOut (use converter or negation binding), ensure disabled tabs remain visible but dimmed and non-selectable
- [X] T014 [US1] Handle tab selection when current tab is popped out — in MainViewModel.PopOutStopwatchCommand (or OnSelectedStopwatchChanged), if the popped-out stopwatch was SelectedStopwatch, auto-select the next non-popped-out tab; if all are popped out, set SelectedStopwatch to null in src/Stopwatch/ViewModels/MainViewModel.cs
- [X] T015 [US2] Implement secondary window close handling in PopOutService — in the Window.Closed event handler, remove the PopOutWindowInfo entry, dispose the DI scope, and raise StopwatchReturned(stopwatchId) event in src/Stopwatch/Services/PopOut/PopOutService.cs
- [X] T016 [US2] Subscribe MainViewModel to IPopOutService.StopwatchReturned event — on event, find the matching StopwatchViewModel in Stopwatches collection and set IsPoppedOut = false, handle unsubscription in ViewUnloaded in src/Stopwatch/ViewModels/MainViewModel.cs
- [X] T017 [US1] Handle main window close — subscribe PopOutService to main window's Closed event and call CloseAll() to close all tracked secondary windows in src/Stopwatch/Services/PopOut/PopOutService.cs and src/Stopwatch/App.xaml.cs
- [X] T018 Verify US1+US2 build — run `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop`

**Checkpoint**: Core pop-out lifecycle works — pop out any stopwatch, tab disables, close secondary window, tab re-enables. State preserved through the cycle.

---

## Phase 3: User Story 3 — Interact with Stopwatch in Secondary Window (Priority: P2)

**Goal**: Users can fully interact with a popped-out stopwatch (start, stop, lap, reset) and the secondary window reflects the stopwatch's visual customization.

**Independent Test**: Pop out a paused stopwatch → start it in secondary window → add laps → stop → reset → verify all operations work identically to main window

### Implementation

- [X] T019 [US3] Apply stopwatch theme to secondary window on load — call IThemeManager.SetTheme with the StopwatchModel.Theme when StopwatchWindowView loads in src/Stopwatch/Views/StopwatchWindowView.xaml.cs
- [X] T020 [US3] Ensure stopwatch background styling renders in secondary window — bind StopwatchWindowView background elements to StopwatchViewModel.BackgroundColor and BackgroundImageUri properties in src/Stopwatch/Views/StopwatchWindowView.xaml
- [X] T021 Verify US3 build — run `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop`

**Checkpoint**: Full stopwatch interaction works in secondary window with visual customization preserved

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases, code quality, and final verification

- [X] T022 Handle edge case — disable close button on popped-out tabs by binding TabViewItem.IsClosable to !IsPoppedOut in src/Stopwatch/Views/MainView.xaml
- [X] T023 Handle edge case — adding new stopwatch while others are popped out should work normally and the new tab should be active and selectable; also handle removing a non-popped-out tab that leaves only popped-out tabs (main window transitions to disabled-tabs-only state, SelectedStopwatch becomes null) in src/Stopwatch/ViewModels/MainViewModel.cs
- [X] T024 Handle edge case — if all stopwatches are popped out, main window shows empty state with disabled tabs but the add-tab button remains functional in src/Stopwatch/Views/MainView.xaml
- [X] T025 Run code formatting verification — `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes`
- [X] T026 Final Desktop build verification — `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop`
- [X] T027 Final WebAssembly build verification (ensure feature doesn't break wasm) — `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-browserwasm`
- [X] T028 [P] Add accessibility attributes to new UI elements — add AutomationProperties.Name on the pop-out button in src/Stopwatch/Views/MainView.xaml, and set AutomationProperties on key elements in src/Stopwatch/Views/StopwatchWindowView.xaml (window title, stopwatch display)
- [X] T029 [P] Add localization keys (x:Uid) for all new user-facing strings — pop-out button tooltip/label in src/Stopwatch/Views/MainView.xaml, secondary window title in src/Stopwatch/Views/StopwatchWindowView.xaml, and add corresponding entries to src/Stopwatch/Strings/en/Resources.resw
- [X] T030 Visual verification via Desktop build — run the Desktop app and visually verify: pop-out button placement and styling, disabled tab appearance, secondary window layout, theme/background rendering in secondary window, and edge cases (all popped out, add new tab, close main window)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — can start immediately. BLOCKS all user stories.
- **US1+US2 (Phase 2)**: Depends on Phase 1 completion
- **US3 (Phase 3)**: Depends on Phase 2 — secondary window must exist to test interaction
- **Polish (Phase 4)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (Pop Out)**: Requires Phase 1 — creates the secondary window
- **US2 (Return)**: Requires US1 — cannot test return without pop-out
- **US3 (Interact)**: Requires US1+US2 — secondary window must function before testing interaction

### Within Each Phase

- Tasks without [P] must be completed in order
- [P] tasks can run in parallel with other [P] tasks in the same phase
- T001 → T002 → T003 → T004 must be sequential (same refactoring chain)
- T005 and T006 can be parallel with each other and with T003-T004 (different files)
- T007 depends on T005 and T006 (registers the types they create)
- T009 and T010 can be parallel (different files)
- T028 and T029 can be parallel (different files, both in Polish)

### Parallel Opportunities

```
Phase 1 Parallel Group 1 (after T002):
  T003: Remove MainViewModel binding from MainView
  T004: Add IsPoppedOut to StopwatchViewModel
  (Different files, no dependency between them)

Phase 1 Parallel Group 2:
  T005 [P]: Create IPopOutService interface
  T006 [P]: Create PopOutService singleton
  (Different files, can run alongside T003-T004)

Phase 2 Parallel Group:
  T009 [P]: Create StopwatchWindowViewModel
  T010 [P]: Create StopwatchWindowView

Phase 4 Parallel Group:
  T028 [P]: Accessibility attributes
  T029 [P]: Localization keys
```

---

## Implementation Strategy

### MVP First (Phase 1 + 2)

1. Complete Phase 1: Foundational (refactor + service infrastructure + DI registration)
2. Complete Phase 2: US1 + US2 (pop out + return lifecycle)
3. **STOP and VALIDATE**: Test pop-out/return cycle manually on Desktop
4. At this point the feature is functional — all core flows work

### Incremental Delivery

1. Phase 1: Foundational → Infrastructure ready
2. Phase 2: US1 + US2 → Pop-out lifecycle works → **MVP complete**
3. Phase 3: US3 → Theme and visual customization in secondary window
4. Phase 4: Polish → Edge cases, accessibility, localization, and cross-platform build verification

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- US1 and US2 share a phase because they form an inseparable lifecycle
- No automated tests — manual verification via Desktop build per project testing policy
- Commit after each task or logical group per constitution's granular commit requirement
- Stop at any checkpoint to validate story independently
