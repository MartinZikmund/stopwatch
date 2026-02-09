<!--
  Sync Impact Report
  ==================
  Version change: 1.0.0 → 1.1.0 (MINOR: expand Cross-Platform Parity for platform-exclusive features)
  Modified principles:
    - I. Cross-Platform Parity: Added platform-exclusive feature allowance
      (Android Widgets, desktop multi-window, iOS Live Activities, etc.)
  Added sections: None
  Removed sections: None
  Templates requiring updates:
    - .specify/templates/plan-template.md: ✅ compatible (Constitution Check section aligns)
    - .specify/templates/spec-template.md: ✅ compatible (user stories + requirements align)
    - .specify/templates/tasks-template.md: ✅ compatible (phase structure aligns)
    - .specify/templates/checklist-template.md: ✅ compatible (generic structure)
    - .specify/templates/agent-file-template.md: ✅ compatible (generic structure)
  Follow-up TODOs: None
-->

# Fluent Stopwatch Constitution

## Core Principles

### I. Cross-Platform Parity

Every **core feature** MUST work on all supported platforms: Android, iOS,
Desktop, WebAssembly, and WinUI. Platform-specific code (conditional
compilation via `#if` directives) is acceptable when Uno Platform does not
provide a sufficient abstraction, but the resulting user-facing behavior
MUST be identical across all targets for shared functionality.

- New features MUST NOT be merged if they break any supported target.
- Platform-specific implementations (e.g., `IStoreService` for iOS vs
  Windows) are permitted when required by OS capabilities.
- Uno Platform abstractions MUST be preferred over raw platform APIs
  whenever an abstraction exists.
- **Platform-exclusive features** (e.g., Android Widgets, desktop
  multi-window, iOS Live Activities) are permitted when the capability
  has no meaningful equivalent on other platforms. These features:
  - MUST NOT degrade the experience on platforms where they are absent.
  - MUST be additive enhancements, not replacements for shared
    functionality.
  - MUST be cleanly isolated behind platform-specific abstractions or
    conditional compilation so they do not affect shared code paths.
  - SHOULD gracefully degrade or simply be absent on unsupported
    platforms (no error states, no broken UI).

**Rationale**: Users expect a consistent core experience regardless of
device, but each platform also has unique strengths. Allowing additive
platform-exclusive features lets the app feel native and take advantage
of OS capabilities without fragmenting the shared experience.

### II. MVVM Architecture

All application logic MUST follow the Model-View-ViewModel pattern with
pragmatic separation of concerns.

- Business logic and application state MUST reside in ViewModels and
  Services. Views are responsible for layout and data binding only.
- Code-behind (`.xaml.cs`) is permitted ONLY for pure UI concerns that
  cannot be expressed in XAML (e.g., animations, focus management,
  visual state coordination). No business logic in code-behind.
- CommunityToolkit.Mvvm source generators MUST be used: `ObservableProperty`,
  `RelayCommand`, `ObservableRecipient`. Manual `INotifyPropertyChanged`
  implementations are prohibited.
- New ViewModels MUST inherit from `PageViewModel`. New Views MUST
  inherit from `PageBase<TViewModel>` and resolve ViewModels via DI.

**Rationale**: Consistent architecture enables predictable code
navigation, testability, and maintainability across the codebase.

### III. Performance Standards

The application MUST meet the following performance thresholds on all
supported platforms:

- **Timer accuracy**: Stopwatch display MUST update at approximately
  50ms intervals with no visible jank or frame drops.
- **Startup time**: The application MUST be interactive within 3 seconds
  of launch on all platforms.
- **Memory budget**: The application MUST remain under 150MB of memory
  during normal use (active stopwatch with lap tracking).

Any change that measurably degrades these thresholds MUST include a
justification and a plan to restore compliance.

**Rationale**: A stopwatch app is a precision tool. Users rely on
smooth, responsive timing. Poor performance undermines core trust.

### IV. Simplicity, Data Compatibility & Integrity

Code MUST be simple and intentional. User data MUST never be lost.

- **YAGNI**: Every class, interface, and abstraction MUST serve a
  current, concrete feature need. No speculative design.
- **Data preservation**: User data (stopwatch history, preferences,
  in-progress stopwatch state) MUST never be silently lost or corrupted.
- **Schema evolution**: Any change to persisted data models (LiteDB
  schemas, JSON file formats) MUST include a migration path that
  preserves existing user data. Breaking changes to storage formats
  are prohibited without migration logic.
- **Extensibility through simplicity**: Prefer designs that are easy
  to extend later over complex upfront abstractions. Future updates
  MUST NOT break user data.

**Rationale**: Users accumulate stopwatch history and preferences over
time. Data loss destroys trust. Simple code is easier to extend safely.

### V. User Experience First

UI and UX decisions MUST prioritize the end-user experience above
developer convenience.

- Visual design MUST follow Fluent Design principles and leverage
  Uno Platform's theming and styling system.
- Layouts MUST be responsive and adapt to different screen sizes
  and orientations.
- Accessibility MUST be considered: meaningful `AutomationProperties`,
  sufficient contrast ratios, and adequate touch target sizes (44x44dp
  minimum).
- User-facing text MUST support localization via `x:Uid` attributes.

**Rationale**: A stopwatch app competes with built-in OS tools. Superior
UX is the primary differentiator and reason users choose this app.

## Technology Constraints

The following technology decisions are constitutional and MUST be
maintained unless amended through the governance process:

- **Framework**: Uno Platform (UnoSingleProject mode)
- **Runtime**: .NET with C# (latest stable or preview as pinned in
  `global.json`)
- **UI Framework**: WinUI / XAML with Uno Platform extensions
- **MVVM Toolkit**: CommunityToolkit.Mvvm (source generators)
- **Local Storage**: LiteDB for Desktop/WebAssembly/WinUI; file-based
  JSON for iOS/Android (via `IDataSource` abstraction)
- **Navigation**: Convention-based `NavigationService` (ViewModel name
  to View name mapping)
- **DI Container**: Microsoft.Extensions.DependencyInjection with
  host-level singletons and window-scoped services
- **Version Management**: Nerdbank.GitVersioning
- **No secrets in code**: API keys, credentials, tokens, and secrets
  MUST never be committed to the repository. Use platform-specific
  secure storage or environment variables.

New NuGet dependencies MUST be justified. Prefer built-in .NET and
Uno Platform APIs over third-party packages when equivalent
functionality exists.

## Development Workflow

### Build Gates

Every change MUST pass these gates before merge:

1. **Desktop build**: `dotnet build src/Stopwatch/Stopwatch.csproj -f net10.0-desktop`
   MUST succeed with zero errors.
2. **Code formatting**: `dotnet format src/Stopwatch/Stopwatch.csproj --verify-no-changes`
   MUST pass. Tabs for indentation as defined in `.editorconfig`.
3. **Pull request review**: All PRs MUST be reviewed before merge to
   the main branch. No direct pushes to main.

### Commit Conventions

- All commits MUST follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
  (e.g., `feat:`, `fix:`, `refactor:`, `docs:`).
- Commit messages MUST describe the "why", not just the "what".

### Testing Policy

- No formal unit tests exist currently. When unit tests are introduced,
  they MUST be required for all new features going forward.
- Until then, build verification on Desktop target serves as the
  minimum quality gate.
- Manual testing via WebAssembly (`python -m http.server`) is
  recommended for UI verification.

### Known Build Limitations

- Full multi-target builds fail due to network restrictions blocking
  Android dependencies. Always use single target framework builds
  (`-f net10.0-desktop` or `-f net10.0-browserwasm`).
- XBD001 errors from dl.google.com are expected and do not indicate
  real failures.

## Governance

This constitution is the authoritative source of project principles
and constraints. It supersedes all other documentation when conflicts
arise.

- **Amendments**: Any change to this constitution MUST be documented
  with a version bump, rationale, and update to the Sync Impact Report.
- **Versioning**: The constitution follows semantic versioning:
  - MAJOR: Principle removal, redefinition, or backward-incompatible
    governance change.
  - MINOR: New principle or section added, or material expansion of
    existing guidance.
  - PATCH: Clarifications, wording improvements, typo fixes.
- **Compliance**: All PRs and code reviews SHOULD verify alignment
  with constitutional principles. Violations MUST be flagged and
  resolved before merge.
- **Conflict resolution**: When a principle conflicts with another,
  the higher-numbered principle yields to the lower-numbered one
  (I > II > III > IV > V), unless the specific context justifies
  an exception documented in the PR.
- **Runtime guidance**: See `CLAUDE.md` for development commands,
  build instructions, and architecture reference.

**Version**: 1.1.0 | **Ratified**: 2026-02-09 | **Last Amended**: 2026-02-09
