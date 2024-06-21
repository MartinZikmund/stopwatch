using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public partial class MainViewModel : PageViewModel
{
	private readonly ITimerFactory _timerFactory;
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly IAppPreferences _appPreferences;

	public MainViewModel(INavigationService navigationService, ITimerFactory timerFactory, IWindowShellProvider windowShellProvider, IAppPreferences appPreferences) : base(navigationService)
	{
		_appPreferences = appPreferences ?? throw new ArgumentNullException(nameof(appPreferences));
		Stopwatch = new(_appPreferences.CurrentStopwatch ?? new(), timerFactory);
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
	}

	[RelayCommand(CanExecute = nameof(CanLap))]
	public void Lap()
	{
		Stopwatch.Lap();
	}

	private bool CanLap() => Stopwatch.IsRunning;

	[RelayCommand]
	public void Reset()
	{
		Stopwatch.Reset();
	}

	[RelayCommand]
	public void GoToSettings() => NavigationService.Navigate<SettingsViewModel>();

	[RelayCommand]
	public void CompactOverlay()
	{
		_windowShellProvider.Window.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.CompactOverlay);
	}

	[RelayCommand]
	public void FullScreen()
	{
		_windowShellProvider.Window.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
	}
}
