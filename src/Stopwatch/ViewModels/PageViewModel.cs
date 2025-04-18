#nullable enable

using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public abstract partial class PageViewModel : ObservableRecipient
{
	protected PageViewModel(INavigationService navigationService)
	{
		NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
	}

	public bool IsDebug =>
#if DEBUG
		true;
#else
		false;
#endif

	public INavigationService NavigationService { get; }

	public bool CanGoBack => NavigationService.CanGoBack;

	[RelayCommand]
	public virtual void GoBack() => NavigationService.GoBack();

	[ObservableProperty]
	public partial string Title { get; set; } = "";

	[ObservableProperty]
	public partial bool IsWorking { get; set; }

	public virtual void ViewCreated() { }

	public virtual void ViewLoading() { }

	public virtual void ViewLoaded() { }

	public virtual void ViewUnloaded() { }

	public void ViewNavigatedToInternal(object? parameter)
	{
		OnPropertyChanged(nameof(CanGoBack));
		ViewNavigatedTo(parameter);
	}

	public virtual void ViewNavigatedTo(object? parameter) { }
}
