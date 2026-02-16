# Feature Specification: Pop Out Stopwatch to Secondary Window

**Feature Branch**: `001-multi-window`
**Created**: 2026-02-10
**Status**: Draft
**Input**: User description: "Spec for https://github.com/MartinZikmund/stopwatch/issues/150. The feature should allow popping the stopwatch into secondary window. This secondary window will not have a tabview - it will display just that given stopwatch. Meanwhile the stopwatch tab in the main window will be disabled. Closing the secondary window will re-enable the tab. ~~If there is only a single stopwatch, secondary window should not be openable.~~" *(Note: The single-stopwatch restriction was removed during clarification — pop-out is always available regardless of stopwatch count.)*

## Clarifications

### Session 2026-02-10

- Q: When one stopwatch is already popped out, should the remaining stopwatch's pop-out still be available? Should there be a minimum stopwatch count requirement? → A: Pop-out is always available regardless of stopwatch count. All stopwatches can be popped out simultaneously, including the only one. No minimum count guard rail.
- Q: Should the popped-out state persist across app restarts, or should all stopwatches return to main window tabs? → A: Transient — all stopwatches return to main window tabs on app restart.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Pop Out a Stopwatch to a Secondary Window (Priority: P1)

A user wants to focus on a stopwatch by popping it out into its own dedicated window. The user initiates a "pop out" action on a stopwatch tab, which opens the stopwatch in a new, separate window. The secondary window displays only the selected stopwatch without any tab bar. The stopwatch continues running (or remains paused) exactly as it was before popping out. Meanwhile, the corresponding tab in the main window becomes visually disabled and non-interactive, indicating the stopwatch is being viewed elsewhere. This works regardless of how many stopwatches exist — even with a single stopwatch, the user can pop it out.

**Why this priority**: This is the core functionality of the feature. Without the ability to pop out a stopwatch, no other user stories are relevant.

**Independent Test**: Can be fully tested by having one or more stopwatches, clicking pop-out on one, and verifying it appears in a new window while the main tab becomes disabled.

**Acceptance Scenarios**:

1. **Given** the user has one or more stopwatch tabs, **When** the user triggers "pop out" on a stopwatch tab, **Then** a new window opens displaying only that stopwatch without a tab bar.
2. **Given** a stopwatch is running when popped out, **When** the secondary window opens, **Then** the stopwatch continues running seamlessly with no interruption in timing.
3. **Given** a stopwatch is paused when popped out, **When** the secondary window opens, **Then** the stopwatch remains paused, showing the same elapsed time.
4. **Given** a stopwatch has been popped out, **When** the user views the main window, **Then** the corresponding tab is visually disabled and cannot be selected or interacted with.
5. **Given** the user has only one stopwatch, **When** the user triggers "pop out", **Then** the stopwatch opens in a secondary window and the main window shows a single disabled tab.
6. **Given** the user has multiple stopwatches, **When** the user pops out all of them, **Then** each opens in its own secondary window and all tabs in the main window are disabled.

---

### User Story 2 - Return Stopwatch to Main Window by Closing the Secondary Window (Priority: P1)

A user who previously popped out a stopwatch wants to return it to the main window. When the user closes the secondary window, the stopwatch tab in the main window is automatically re-enabled and becomes interactive again. The stopwatch state (running, paused, elapsed time, laps) is preserved exactly as it was.

**Why this priority**: This completes the core pop-out lifecycle. Users must be able to return to normal single-window operation.

**Independent Test**: Can be tested by popping out a stopwatch, closing the secondary window, and verifying the tab becomes active again with the correct stopwatch state.

**Acceptance Scenarios**:

1. **Given** a stopwatch is displayed in a secondary window, **When** the user closes that secondary window, **Then** the corresponding tab in the main window becomes enabled and interactive again.
2. **Given** a running stopwatch is in a secondary window, **When** the secondary window is closed, **Then** the stopwatch continues running in the re-enabled main window tab without any time gap or reset.
3. **Given** a stopwatch with recorded laps is in a secondary window, **When** the secondary window is closed, **Then** all lap data is preserved in the re-enabled tab.

---

### User Story 3 - Interact with Stopwatch in the Secondary Window (Priority: P2)

A user viewing a stopwatch in a secondary window can fully interact with it: start, stop, reset, and record laps. The secondary window provides the same stopwatch controls as the main window, just without the tab bar.

**Why this priority**: Essential for the secondary window to be useful, but depends on the pop-out mechanism working first.

**Independent Test**: Can be tested by popping out a stopwatch and performing all stopwatch operations (start, stop, lap, reset) in the secondary window.

**Acceptance Scenarios**:

1. **Given** a paused stopwatch is in a secondary window, **When** the user clicks start, **Then** the stopwatch begins timing.
2. **Given** a running stopwatch is in a secondary window, **When** the user clicks stop, **Then** the stopwatch pauses.
3. **Given** a running stopwatch is in a secondary window, **When** the user records a lap, **Then** the lap is added to the lap list.
4. **Given** a paused stopwatch is in a secondary window, **When** the user resets, **Then** the stopwatch returns to zero and laps are cleared.

---

### Edge Cases

- What happens if the user closes the main window while a secondary window is open? The secondary window should also close, as the main window is the application's primary lifecycle owner.
- What happens if the user tries to pop out a stopwatch that is already popped out? This should not be possible since the tab is disabled and non-interactive.
- What happens if the user adds a new stopwatch tab while one is popped out? The new tab should appear normally in the main window; the popped-out stopwatch remains in its secondary window.
- What happens if the user removes a non-popped-out stopwatch tab, leaving only the popped-out one in the main window? The popped-out stopwatch remains in its secondary window; the main window shows only disabled tabs until the secondary window is closed.
- What happens if the secondary window is resized? The stopwatch display should scale appropriately to the window size.
- What happens if all stopwatches are popped out? The main window remains open showing only disabled tabs. The user can still add a new stopwatch tab via the add tab button.
- What happens on app restart when stopwatches were popped out? All stopwatches return to main window tabs; the popped-out state is not persisted.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a pop-out action on each stopwatch tab that opens the stopwatch in a new dedicated window.
- **FR-002**: The pop-out action MUST be available on any stopwatch tab regardless of the total number of stopwatches.
- **FR-003**: The secondary window MUST display only the selected stopwatch without any tab bar or tab management controls.
- **FR-004**: When a stopwatch is popped out, its corresponding tab in the main window MUST become visually disabled and non-interactive.
- **FR-005**: The disabled tab MUST remain visible in the main window tab bar to indicate the stopwatch still exists.
- **FR-006**: Closing the secondary window MUST re-enable the corresponding tab in the main window.
- **FR-007**: The stopwatch state (running/paused, elapsed time, laps) MUST be preserved when popping out and when returning to the main window.
- **FR-008**: Users MUST be able to start, stop, reset, and record laps on a stopwatch displayed in a secondary window.
- **FR-009**: Closing the main window MUST also close all secondary stopwatch windows.
- **FR-010**: The system MUST prevent more than one secondary window per stopwatch.
- **FR-011**: The secondary window MUST maintain the stopwatch's visual customization (theme, background, colors).
- **FR-012**: The popped-out state MUST be transient; on app restart, all stopwatches MUST return to main window tabs.

### Key Entities

- **Stopwatch**: A timer instance with elapsed time, running state, lap records, and visual customization. Can be displayed in either the main window tab or a dedicated secondary window.
- **Secondary Window**: A standalone window that displays a single stopwatch without tab management. Linked to exactly one stopwatch and one disabled tab in the main window.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can pop out a stopwatch to a secondary window in a single action (one click or keyboard shortcut).
- **SC-002**: The stopwatch timing continues without any visible interruption (no reset, no pause, no time gap) when transitioning between main window and secondary window.
- **SC-003**: The pop-out and return-to-main-window cycle preserves 100% of stopwatch state (elapsed time, running state, all lap records).
- **SC-004**: All stopwatch controls (start, stop, reset, lap) function identically in the secondary window as in the main window.

## Assumptions

- This feature is available on platforms that support multiple windows (Desktop, WinUI). On platforms that do not support multiple windows (mobile, WebAssembly), the pop-out action will not be shown.
- The secondary window can be independently moved, resized, and positioned by the user via standard OS window management.
- The "pop out" action will be accessible from the tab context or tab area (exact UI placement to be determined in planning).
- Screen-sleep prevention (display request) applies globally: if any stopwatch is running in any window, screen sleep is prevented.
