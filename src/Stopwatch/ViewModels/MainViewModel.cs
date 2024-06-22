using Stopwatch.Model;
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
		IWindowShellProvider windowShellProvider) : base(navigationService)
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
		Stopwatch = new(stopwatch, dataSource, timerFactory);
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
