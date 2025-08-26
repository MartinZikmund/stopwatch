#pragma warning disable Uno0001

using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Windows.ApplicationModel.Store;
using Windows.Services.Store;
using WinRT.Interop;

namespace Stopwatch.Services.Store;

public class StoreService : IStoreService
{
	private const string StopwatchProId = "9PJRM3NWXGBN";

	private readonly IWindowShellProvider _shellProvider;
	private readonly IDialogService _dialogService;
	private readonly StoreContext _storeContext;

	private bool? _hasPro = null;

	public StoreService(IWindowShellProvider shellProvider, IDialogService dialogService)
	{
		_shellProvider = shellProvider ?? throw new ArgumentNullException(nameof(shellProvider));
		_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
		_storeContext = StoreContext.GetDefault();
	}

	public async Task<string?> GetPriceAsync()
	{
		var context = GetStoreContext();
		var listingsQueryResult = await context.GetStoreProductsAsync([ProductType.Durable.ToString("g")], [StopwatchProId]);

		if (listingsQueryResult.ExtendedError != null)
		{
			// The user may be offline or there might be some other server failure.
			return null;
		}

		var product = listingsQueryResult.Products.Values.FirstOrDefault();
		if (product is null)
		{
			return null;
		}

		var price = product.Price.FormattedPrice;
		return price;
	}

	public async Task<bool> HasProAsync()
	{
		if (_hasPro is null)
		{
			var context = GetStoreContext();
			var result = await context.GetAppLicenseAsync();
			_hasPro = result.AddOnLicenses.Any(license => license.Value.SkuStoreId == StopwatchProId);
		}

		return _hasPro.Value;
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		var context = GetStoreContext();
		var purchaseResult = await context.RequestPurchaseAsync(StopwatchProId);

		// Capture the error message for the operation, if any.
		string extendedError = string.Empty;
		if (purchaseResult.ExtendedError != null)
		{
			extendedError = purchaseResult.ExtendedError.Message;
		}

		var result = false;
		switch (purchaseResult.Status)
		{
			case StorePurchaseStatus.AlreadyPurchased:
			case StorePurchaseStatus.Succeeded:
				result = true;
				break;
			case StorePurchaseStatus.NetworkError:
				await ShowError("PurchaseNetworkError");
				break;
			case StorePurchaseStatus.ServerError:
				await ShowError("PurchaseServerError");
				break;
			default:
				await ShowError("PurchaseUnknownError", extendedError);
				break;
		}

		if (result)
		{
			_hasPro = true;
		}

		return result;
	}

	private async Task ShowError(string errorResourceId, string additionalInformation = "")
	{
		var content = Localizer.Instance.GetString(errorResourceId);
		if (!string.IsNullOrEmpty(additionalInformation))
		{
			content += $"{Environment.NewLine}{additionalInformation}";
		}

		await _dialogService.ShowAsync(Localizer.Instance.GetString("StoreError"), content);
	}

	private StoreContext GetStoreContext()
	{
		var hWnd = WindowNative.GetWindowHandle(_shellProvider.Window);
		InitializeWithWindow.Initialize(_storeContext, hWnd);
		return _storeContext;
	}
}
