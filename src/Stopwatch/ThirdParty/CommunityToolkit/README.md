# Community Toolkit (source copy)

The parts of [CommunityToolkit/Windows](https://github.com/CommunityToolkit/Windows) Stopwatch uses, copied at tag
`v8.2.251219`, because no published build targets Uno 7 yet. MIT licensed, see [License.md](License.md).

| Folder | Upstream package |
|---|---|
| `SettingsControls`, `Triggers`, `Helpers/WeakEventListener.cs` | `CommunityToolkit.WinUI.Controls.SettingsControls` |
| `Converters` | `CommunityToolkit.WinUI.Converters` |
| `Helpers/ColorHelper`, `Helpers/PackageVersionHelper.cs` | `CommunityToolkit.WinUI.Helpers` |
| `Extensions` | `CommunityToolkit.WinUI.Extensions` (transitive) |

Only the files in use are copied. The folder layout and namespaces match the packages, so nothing outside this folder
knows it is a copy except the path in `App.xaml`. The only edits to the upstream files:

- `using` lines at the top of the C# files, which upstream's build tooling supplies as global usings
- the `ms-appx` paths in `SettingsControls/Themes/Generic.xaml` and `SettingsExpander.xaml`
- `#if HAS_UNO && WINUI2` in `DependencyObjectExtensions.cs`, because `DependencyObject` is a class from Uno 7 on, so
  the old interface-era `class` constraint no longer compiles

## Going back to the packages

Once CommunityToolkit builds target Uno 7:

1. Delete this folder.
2. In `src/Directory.Packages.props`, replace the comment pointing here with `PackageVersion` items for
   `CommunityToolkit.WinUI.Controls.SettingsControls`, `CommunityToolkit.WinUI.Converters` and
   `CommunityToolkit.WinUI.Helpers`.
3. In `Stopwatch.csproj`, add a `PackageReference` for each of them.
4. In `App.xaml`, remove the merged dictionary that points at this folder.
