using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public class SettingsViewModel : PageViewModel
{
    public SettingsViewModel(INavigationService navigationService) : base(navigationService)
    {
    }
}
