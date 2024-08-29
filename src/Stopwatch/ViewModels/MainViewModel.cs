using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Stopwatch.Models;
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
	private readonly IHistoryService _historyService;
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly IAppPreferences _appPreferences;
	private readonly IDisplayRequestManager _displayRequestManager;
	private readonly DispatcherQueueTimer _timer;

	[ObservableProperty]
	private StopwatchModel? _selectedStopwatch;

	[ObservableProperty]
	private StopwatchViewModel? _displayedStopwatch;

	public MainViewModel(
		INavigationService navigationService,
		ITimerFactory timerFactory,
		IDataSource dataSource,
		IHistoryService historyService,
		IWindowShellProvider windowShellProvider,
		IAppPreferences appPreferences,
		IDisplayRequestManager displayRequestManager) : base(navigationService)
	{
		_timerFactory = timerFactory;
		_dataSource = dataSource;
		_historyService = historyService;
		_windowShellProvider = windowShellProvider;
		_appPreferences = appPreferences;
		_displayRequestManager = displayRequestManager;

		_timer = timerFactory.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(50);
		_timer.Tick += (sender, e) => OnTick();
		_timer.Start();
	}

	private void OnTick()
	{
		DisplayedStopwatch?.OnTick();
	}

	private void OnStart()
	{
		if (_appPreferences.KeepScreenOn)
		{
			_displayRequestDisposable.Disposable = _displayRequestManager.RequestActive();
		}
	}

	private void OnStop()
	{
		_displayRequestDisposable.Disposable = null;
	}

	public ObservableCollection<StopwatchModel> Stopwatches { get; } = new();

	public override void ViewNavigatedTo(object? parameter)
	{
		ReloadStopwatches();
	}

	public override void ViewUnloaded()
	{
		DisplayedStopwatch?.Dispose();
		DisplayedStopwatch = null;
	}

	[MemberNotNull(nameof(DisplayedStopwatch))]
	private void ReloadStopwatches()
	{
		var stopwatches = _dataSource.Stopwatches.GetAll();
		
		// Merge with Stopwatches collection
		foreach (var stopwatch in stopwatches)
		{
			if (!Stopwatches.Any(s => s.Id == stopwatch.Id))
			{
				Stopwatches.Add(stopwatch);
			}
		}

		// Remove deleted stopwatches
		foreach (var stopwatch in Stopwatches.ToArray())
		{
			if (!stopwatches.Any(s => s.Id == stopwatch.Id))
			{
				Stopwatches.Remove(stopwatch);
			}
		}

		SelectedStopwatch ??= Stopwatches.FirstOrDefault();
		DisplayedStopwatch = new StopwatchViewModel(SelectedStopwatch!, _dataSource, _timerFactory, _appPreferences, _historyService, _displayRequestManager);
	}

	[RelayCommand]
	public void GoToSettings() => NavigationService.Navigate<SettingsViewModel>();

	[RelayCommand]
	public void GoToHistory() => NavigationService.Navigate<HistoryViewModel>();

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
