#if __IOS__
using Plugin.InAppBilling;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;

namespace Stopwatch.Services.Store;

public class StoreService : IStoreService
{
	private const string StopwatchProProductId = "dev.mzikmund.stopwatch.pro";

	private readonly IDialogService _dialogService;
	private bool? _hasPro = null;

	public StoreService(IDialogService dialogService)
	{
		_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
	}

	public async Task<string?> GetPriceAsync()
	{
		try
		{
			var billing = CrossInAppBilling.Current;
			await billing.ConnectAsync();

			var products = await billing.GetProductInfoAsync(ItemType.InAppPurchase, [StopwatchProProductId]);
			var product = products?.FirstOrDefault(p => p.ProductId == StopwatchProProductId);

			await billing.DisconnectAsync();

			return product?.LocalizedPrice;
		}
		catch (Exception ex)
		{
			// Log error or handle gracefully
			System.Diagnostics.Debug.WriteLine($"Error getting price: {ex.Message}");
			return null;
		}
	}

	public async Task<bool> HasProAsync()
	{
		if (_hasPro is null)
		{
			try
			{
				var billing = CrossInAppBilling.Current;
				await billing.ConnectAsync();

				var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
				_hasPro = purchases?.Any(p => p.ProductId == StopwatchProProductId && p.State == PurchaseState.Purchased) ?? false;

				await billing.DisconnectAsync();
			}
			catch (Exception ex)
			{
				// Log error or handle gracefully
				System.Diagnostics.Debug.WriteLine($"Error checking Pro status: {ex.Message}");
				_hasPro = false;
			}
		}

		return _hasPro.Value;
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		try
		{
			var billing = CrossInAppBilling.Current;
			await billing.ConnectAsync();

			var purchase = await billing.PurchaseAsync(StopwatchProProductId, ItemType.InAppPurchase);

			if (purchase == null)
			{
				// User cancelled
				await billing.DisconnectAsync();
				return false;
			}

			if (purchase.State == PurchaseState.Purchased)
			{
				// Verify the purchase if needed
				_hasPro = true;
				await billing.DisconnectAsync();
				return true;
			}
			else if (purchase.State == PurchaseState.Restored)
			{
				// Purchase was restored
				_hasPro = true;
				await billing.DisconnectAsync();
				return true;
			}
			else
			{
				await ShowError("PurchaseUnknownError", purchase.State.ToString());
				await billing.DisconnectAsync();
				return false;
			}
		}
		catch (InAppBillingPurchaseException purchaseEx)
		{
			// Handle specific purchase exceptions
			switch (purchaseEx.PurchaseError)
			{
				case PurchaseError.UserCancelled:
					// User cancelled, no error message needed
					break;
				case PurchaseError.PaymentInvalid:
				case PurchaseError.BillingUnavailable:
					await ShowError("PurchaseServerError", purchaseEx.Message);
					break;
				case PurchaseError.AlreadyOwned:
					_hasPro = true;
					return true;
				default:
					await ShowError("PurchaseUnknownError", purchaseEx.Message);
					break;
			}
			return false;
		}
		catch (Exception ex)
		{
			await ShowError("PurchaseUnknownError", ex.Message);
			return false;
		}
	}

	public async Task<bool> TryRestorePurchasesAsync()
	{
		try
		{
			var billing = CrossInAppBilling.Current;
			await billing.ConnectAsync();

			var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
			var hasPro = purchases?.Any(p => p.ProductId == StopwatchProProductId && p.State == PurchaseState.Purchased) ?? false;

			await billing.DisconnectAsync();

			if (hasPro)
			{
				_hasPro = true;
				return true;
			}
			else
			{
				// Show message that no purchases were found to restore
				await _dialogService.ShowAsync(
					Localizer.Instance.GetString("RestorePurchases"),
					Localizer.Instance.GetString("NoPurchasesToRestore"));
				return false;
			}
		}
		catch (Exception ex)
		{
			// Log error or handle gracefully
			System.Diagnostics.Debug.WriteLine($"Error restoring purchases: {ex.Message}");
			await ShowError("RestorePurchasesError", ex.Message);
			return false;
		}
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
}
#endif
