using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public partial class WindowShellViewModel : ObservableObject
{
	private readonly IWindowShellProvider _provider;
	private readonly INavigationService _navigationService;

	[ObservableProperty]
	private bool _isLoading;

	[ObservableProperty]
	private string _loadingStatusMessage = "";

	public WindowShellViewModel(IWindowShellProvider provider, INavigationService navigationService)
	{
		_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
	}

	public string Title { get; set; } = "Stopwatch";

	public void BackRequested() => _navigationService.GoBack();
}
