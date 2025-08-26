#if __ANDROID__
using System;
using System.Linq;
using Plugin.InAppBilling;

namespace Stopwatch.Services.Store;

/// <summary>
/// Google Play implementation of the StoreService using Plugin.InAppBilling.
/// </summary>
public class StoreService : IStoreService
{
	// TODO: Replace with the actual product id configured in Google Play Console.
	private const string ProProductId = "stopwatch_pro"; // in-app (non-consumable) product id

	private string? _cachedPrice;
	private bool? _hasPro;

	public async Task<string?> GetPriceAsync()
	{
		if (_cachedPrice is not null)
		{
			return _cachedPrice;
		}

		var billing = CrossInAppBilling.Current;
		try
		{
			var connected = await billing.ConnectAsync();
			if (!connected)
			{
				return null; // offline / billing not available
			}

			var info = await billing.GetProductInfoAsync(ItemType.InAppPurchase, new[] { ProProductId });
			var product = info?.FirstOrDefault();
			_cachedPrice = product?.LocalizedPrice;
			return _cachedPrice;
		}
		catch
		{
			return null;
		}
		finally
		{
			await billing.DisconnectAsync();
		}
	}

	public async Task<bool> HasProAsync()
	{
		if (_hasPro is not null)
		{
			return _hasPro.Value;
		}

		var billing = CrossInAppBilling.Current;
		try
		{
			var connected = await billing.ConnectAsync();
			if (!connected)
			{
				_hasPro = false;
				return false;
			}

			var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
			_hasPro = purchases?.Any(p => string.Equals(p.ProductId, ProProductId, StringComparison.OrdinalIgnoreCase)) == true;
			return _hasPro.Value;
		}
		catch
		{
			_hasPro = false;
			return false;
		}
		finally
		{
			await billing.DisconnectAsync();
		}
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		// If already purchased, short-circuit.
		if (await HasProAsync())
		{
			return true;
		}

		var billing = CrossInAppBilling.Current;
		try
		{
			var connected = await billing.ConnectAsync();
			if (!connected)
			{
				return false;
			}

			InAppBillingPurchase? purchase = null;
			try
			{
				purchase = await billing.PurchaseAsync(ProProductId, ItemType.InAppPurchase, "");
			}
			catch
			{
				// ignored - we'll verify ownership below
			}

			if (purchase is not null && string.Equals(purchase.ProductId, ProProductId, StringComparison.OrdinalIgnoreCase))
			{
				_hasPro = true;
				return true;
			}

			// Fallback: re-check ownership (covers AlreadyOwned scenarios depending on plugin version)
			var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
			if (purchases?.Any(p => string.Equals(p.ProductId, ProProductId, StringComparison.OrdinalIgnoreCase)) == true)
			{
				_hasPro = true;
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
		finally
		{
			await billing.DisconnectAsync();
		}
	}
}
#endif
