using Stopwatch.Services;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Store;

namespace Stopwatch.ViewModels;

public partial class GetProViewModel : PageViewModel
{
	private readonly IStoreService _storeService;
	private readonly IDialogService _dialogService;

	[ObservableProperty]
	public partial string? CurrentPrice { get; set; }

	public GetProViewModel(INavigationService navigationService, IStoreService storeService, IDialogService dialogService) : base(navigationService)
	{
		_storeService = storeService;
		_dialogService = dialogService;
	}

	public override async void ViewNavigatedTo(object? parameter)
	{
		var price = await _storeService.GetPriceAsync();
		if (price is not null)
		{
			CurrentPrice = price;
		}
	}

	[RelayCommand]
	public async Task GetProAsync()
	{
		var result = await _storeService.TryPurchaseProAsync();

		if (result)
		{
			GoBack();
		}
	}

	[RelayCommand]
	public async Task RestorePurchasesAsync()
	{
		IsWorking = true;
		try
		{
			var result = await _storeService.TryRestorePurchasesAsync();

			if (result)
			{
				await _dialogService.ShowAsync(
					Localizer.Instance.GetString("RestorePurchases"),
					Localizer.Instance.GetString("PurchasesRestored"));
				GoBack();
			}
		}
		finally
		{
			IsWorking = false;
		}
	}
}
