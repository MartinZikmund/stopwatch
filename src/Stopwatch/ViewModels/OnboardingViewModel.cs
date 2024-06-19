using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public partial class OnboardingViewModel : PageViewModel
{
	private readonly INavigationService _navigationService;

	public OnboardingViewModel(INavigationService navigationService) : base(navigationService)
	{
		_navigationService = navigationService;
	}

	[RelayCommand]
	public void GetStarted()
	{
		_navigationService.Navigate<MainViewModel>();
	}
}
