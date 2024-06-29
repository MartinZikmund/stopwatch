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
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly IAppPreferences _appPreferences;

	public MainViewModel(
		INavigationService navigationService,
		ITimerFactory timerFactory,
		IDataSource dataSource,
		IWindowShellProvider windowShellProvider,
		IDisplayRequestManager displayRequestManager) : base(navigationService)
	{
		StopwatchModel stopwatch;
		if (dataSource.GetAll() is { Length: > 0 } array)
		{
			stopwatch = array[0];
		}
		else
		{
			stopwatch = new StopwatchModel();
			dataSource.Add(stopwatch);
		}
		Stopwatch = new(stopwatch, dataSource, timerFactory, displayRequestManager);
		_windowShellProvider = windowShellProvider;
	}

	public StopwatchViewModel Stopwatch { get; }

	[RelayCommand]
	public void StartStop()
	{
		if (Stopwatch.IsRunning)
		{
			Stopwatch.Stop();
		}
		else
		{
			Stopwatch.Start();
		}
		LapCommand?.NotifyCanExecuteChanged();
		ResetCommand?.NotifyCanExecuteChanged();
	}

	[RelayCommand(CanExecute = nameof(CanLap))]
	public void Lap()
	{
		Stopwatch.Lap();
	}

	private bool CanLap() => Stopwatch.IsRunning;

	[RelayCommand(CanExecute = nameof(CanReset))]
	public void Reset()
	{
		Stopwatch.Reset();
		LapCommand?.NotifyCanExecuteChanged();
		ResetCommand?.NotifyCanExecuteChanged();
	}

	private bool CanReset() => !Stopwatch.IsZero || Stopwatch.IsRunning;

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
