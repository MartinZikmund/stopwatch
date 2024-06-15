using CommunityToolkit.Mvvm.ComponentModel;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public partial class MainViewModel : PageViewModel
{
	private readonly ITimerFactory _timerFactory;
	private readonly IWindowShellProvider _windowShellProvider;

	public MainViewModel(INavigationService navigationService, ITimerFactory timerFactory, IWindowShellProvider windowShellProvider) : base(navigationService)
	{
		Stopwatch = new StopwatchViewModel(new Model.StopwatchModel(), timerFactory);
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
