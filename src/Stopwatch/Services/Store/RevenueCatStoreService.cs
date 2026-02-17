#if __IOS__ || __ANDROID__
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Services;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;

namespace Stopwatch.Services.Store;

public class RevenueCatStoreService : IStoreService
{
	private const string ProEntitlementIdentifier = "stopwatch_pro";
	private const string StopwatchProProductId = "dev.mzikmund.stopwatch.pro";

	private readonly IRevenueCatBilling _billing;
	private readonly IDialogService _dialogService;
	private readonly IAppPreferences _appPreferences;
	private readonly RevenueCatOptions _options;
	private bool? _hasPro = null;
	private bool _isInitialized = false;

	public RevenueCatStoreService(
		IRevenueCatBilling billing,
		IDialogService dialogService,
		IAppPreferences appPreferences,
		RevenueCatOptions options)
	{
		_billing = billing ?? throw new ArgumentNullException(nameof(billing));
		_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
		_appPreferences = appPreferences ?? throw new ArgumentNullException(nameof(appPreferences));
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}

	private void EnsureInitialized()
	{
		if (_isInitialized)
		{
			return;
		}

#if __IOS__
		var apiKey = _options.iOSApiKey;
#elif __ANDROID__
		var apiKey = _options.AndroidApiKey;
#endif
		_billing.Initialize(apiKey);
		_isInitialized = true;
	}

	public async Task<string?> GetPriceAsync()
	{
		try
		{
			EnsureInitialized();

			var offerings = await _billing.GetOfferingsAsync();
			var currentOffering = offerings.FirstOrDefault(o => o.IsCurrent);
			if (currentOffering == null)
			{
				return null;
			}

			// Find the Pro package by product ID
			var proPackage = currentOffering.AvailablePackages
				.FirstOrDefault(p => p.Product.Sku == StopwatchProProductId);

			return proPackage?.Product.Pricing?.PriceLocalized;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error getting price: {ex.Message}");
			return null;
		}
	}

	public async Task<bool> HasProAsync()
	{
		if (_hasPro.HasValue)
		{
			return _hasPro.Value;
		}

		// First check local cache for offline scenarios
		if (_appPreferences.HasPro)
		{
			_hasPro = true;
			return true;
		}

		try
		{
			EnsureInitialized();

			var customerInfo = await _billing.GetCustomerInfoAsync();
			if (customerInfo != null)
			{
				var proEntitlement = customerInfo.Entitlements
					.FirstOrDefault(e => e.Identifier == ProEntitlementIdentifier);

				_hasPro = proEntitlement?.IsActive ?? false;

				// Cache locally for offline access
				if (_hasPro.Value)
				{
					_appPreferences.HasPro = true;
				}

				return _hasPro.Value;
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error checking pro status: {ex.Message}");
		}

		// Fall back to cached preference
		_hasPro = _appPreferences.HasPro;
		return _hasPro.Value;
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		try
		{
			EnsureInitialized();

			var offerings = await _billing.GetOfferingsAsync();
			var currentOffering = offerings.FirstOrDefault(o => o.IsCurrent);
			if (currentOffering == null)
			{
				await ShowError("PurchaseServerError", "No offerings available");
				return false;
			}

			var proPackage = currentOffering.AvailablePackages
				.FirstOrDefault(p => p.Product.Sku == StopwatchProProductId);

			if (proPackage == null)
			{
				await ShowError("PurchaseUnknownError", "Product not found");
				return false;
			}

			var purchaseResult = await _billing.PurchaseProductAsync(proPackage);

			if (purchaseResult.IsSuccess)
			{
				_hasPro = true;
				_appPreferences.HasPro = true;
				return true;
			}

			// Handle specific error cases
			switch (purchaseResult.ErrorStatus)
			{
				case PurchaseErrorStatus.PurchaseCancelledError:
					// User cancelled, no error message needed
					return false;

				case PurchaseErrorStatus.ProductAlreadyPurchasedError:
					_hasPro = true;
					_appPreferences.HasPro = true;
					return true;

				case PurchaseErrorStatus.NetworkError:
					await ShowError("PurchaseNetworkError");
					break;

				case PurchaseErrorStatus.StoreProblemError:
				case PurchaseErrorStatus.InvalidReceiptError:
					await ShowError("PurchaseServerError");
					break;

				default:
					await ShowError("PurchaseUnknownError", purchaseResult.ErrorStatus?.ToString() ?? "");
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
			EnsureInitialized();

			var customerInfo = await _billing.RestoreTransactionsAsync();

			if (customerInfo != null)
			{
				var proEntitlement = customerInfo.Entitlements
					.FirstOrDefault(e => e.Identifier == ProEntitlementIdentifier);

				if (proEntitlement?.IsActive == true)
				{
					_hasPro = true;
					_appPreferences.HasPro = true;
					return true;
				}
			}

			// Show message that no purchases were found
			await _dialogService.ShowAsync(
				Localizer.Instance.GetString("RestorePurchases"),
				Localizer.Instance.GetString("NoPurchasesToRestore"));
			return false;
		}
		catch (Exception ex)
		{
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
