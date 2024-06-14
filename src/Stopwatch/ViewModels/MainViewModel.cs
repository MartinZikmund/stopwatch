using CommunityToolkit.Mvvm.ComponentModel;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public partial class MainViewModel : PageViewModel
{
	private readonly ITimerFactory _timerFactory;

	public MainViewModel(INavigationService navigationService, ITimerFactory timerFactory) : base(navigationService)
	{
		Stopwatch = new StopwatchViewModel(new Model.StopwatchModel(), timerFactory);
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
}
