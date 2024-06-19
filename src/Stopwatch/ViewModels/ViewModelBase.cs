#nullable enable

using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public abstract partial class PageViewModel : ObservableRecipient
{
	protected PageViewModel(INavigationService navigationService)
	{
		NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
	}

	public INavigationService NavigationService { get; }

	public bool CanGoBack => NavigationService.CanGoBack;

	[RelayCommand]
	public void GoBack() => NavigationService.GoBack();

	[ObservableProperty]
	private string _title = "";

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
