using CommunityToolkit.Mvvm.ComponentModel;
using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public partial class MainViewModel : PageViewModel
{
	public MainViewModel(INavigationService navigationService) : base(navigationService)
	{
	}
}
