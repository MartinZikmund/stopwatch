#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public abstract partial class PageViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;

    protected PageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public bool CanGoBack => _navigationService.CanGoBack;

    [RelayCommand]
    public void GoBack() => _navigationService.GoBack();

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
