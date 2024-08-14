using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Windowing;
using Stopwatch.Model;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public partial class MainViewModel : PageViewModel
{
	private readonly ITimerFactory _timerFactory;
	private readonly IDataSource _dataSource;
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly IAppPreferences _appPreferences;
	private readonly IDisplayRequestManager _displayRequestManager;

	[ObservableProperty]
	private StopwatchViewModel? _selectedStopwatch;

	public MainViewModel(
		INavigationService navigationService,
		ITimerFactory timerFactory,
		IDataSource dataSource,
		IWindowShellProvider windowShellProvider,
		IAppPreferences appPreferences,
		IDisplayRequestManager displayRequestManager) : base(navigationService)
	{
		_timerFactory = timerFactory;
		_dataSource = dataSource;
		_windowShellProvider = windowShellProvider;
		_appPreferences = appPreferences;
		_displayRequestManager = displayRequestManager;
		ReloadStopwatch();
	}

	public override void ViewLoaded()
	{
		ReloadStopwatch();
	}

	public override void ViewUnloaded()
	{
		SelectedStopwatch?.Dispose();
		SelectedStopwatch = null!;
	}

	[MemberNotNull(nameof(Stopwatch))]
	private void ReloadStopwatch()
	{
		var stopwatch = _dataSource.GetOrCreateFirst();
		SelectedStopwatch = new(stopwatch, _dataSource, _timerFactory, _appPreferences, _displayRequestManager);
	}

	[RelayCommand]
	public void GoToSettings() => NavigationService.Navigate<SettingsViewModel>();

	[RelayCommand]
	public void ToggleCompactOverlay()
	{
		var newPresenterKind = IsCompactOverlay ? Microsoft.UI.Windowing.AppWindowPresenterKind.Default : Microsoft.UI.Windowing.AppWindowPresenterKind.CompactOverlay;
		_windowShellProvider.Window.AppWindow.SetPresenter(newPresenterKind);

		UpdatePresenterButtons();
	}

	[RelayCommand]
	public void ToggleFullScreen()
	{
		var newPresenterKind = IsFullScreen ? Microsoft.UI.Windowing.AppWindowPresenterKind.Default : Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen;
		_windowShellProvider.Window.AppWindow.SetPresenter(newPresenterKind);

		UpdatePresenterButtons();
	}

	private void UpdatePresenterButtons()
	{
		OnPropertyChanged(nameof(IsFullScreen));
		OnPropertyChanged(nameof(IsCompactOverlay));
		OnPropertyChanged(nameof(ShowCompactOverlayButton));
		OnPropertyChanged(nameof(ShowFullScreenButton));
	}

	public bool IsFullScreen => _windowShellProvider.Window.AppWindow.Presenter is FullScreenPresenter;

	public bool IsCompactOverlay => _windowShellProvider.Window.AppWindow.Presenter is CompactOverlayPresenter;

	public bool ShowCompactOverlayButton => !IsFullScreen;

	public bool ShowFullScreenButton => !IsCompactOverlay;
}
