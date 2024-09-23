#pragma warning disable Uno0001

using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Windows.Services.Store;
using WinRT.Interop;

namespace Stopwatch.Services.Store;

public class StoreService : IStoreService
{
	private const string StopwatchProId = "9PJRM3NWXGBN";

	private readonly IWindowShellProvider _shellProvider;
	private readonly IDialogService _dialogService;
	private readonly StoreContext _storeContext;

	public StoreService(IWindowShellProvider shellProvider, IDialogService dialogService)
	{
		_shellProvider = shellProvider ?? throw new ArgumentNullException(nameof(shellProvider));
		_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
		_storeContext = StoreContext.GetDefault();
	}

	public async Task<bool> HasProAsync()
	{
		var context = GetStoreContext();
		var result = await context.GetAppLicenseAsync();
		return result.AddOnLicenses.Any(license => license.Value.SkuStoreId == StopwatchProId);
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		var context = GetStoreContext();
		var result = await context.RequestPurchaseAsync(StopwatchProId);

		// Capture the error message for the operation, if any.
		string extendedError = string.Empty;
		if (result.ExtendedError != null)
		{
			extendedError = result.ExtendedError.Message;
		}

		switch (result.Status)
		{
			case StorePurchaseStatus.AlreadyPurchased:
				return true;

			case StorePurchaseStatus.Succeeded:
				return true;

			case StorePurchaseStatus.NotPurchased:
				return false;

			case StorePurchaseStatus.NetworkError:
				await ShowPurchaseErrorAsync("PurchaseNetworkError");
				return false;

			case StorePurchaseStatus.ServerError:
				await ShowPurchaseErrorAsync("PurchaseServerError");
				return false;

			default:
				await ShowPurchaseErrorAsync("PurchaseUnknownError", extendedError);
				return false;
		}
	}

	private async Task ShowPurchaseErrorAsync(string errorResourceId, string additionalInformation = "")
	{
		var content = Localizer.Instance.GetString(errorResourceId);
		if (!string.IsNullOrEmpty(additionalInformation))
		{
			content = string.Format(content, additionalInformation);
		}

		await _dialogService.ShowAsync(Localizer.Instance.GetString("PurchaseError"), content);
	}

	private StoreContext GetStoreContext()
	{
		var hWnd = WindowNative.GetWindowHandle(_shellProvider.Window);
		InitializeWithWindow.Initialize(_storeContext, hWnd);
		return _storeContext;
	}
}
