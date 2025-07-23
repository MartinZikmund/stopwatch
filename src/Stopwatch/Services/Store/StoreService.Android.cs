using Plugin.InAppBilling;

namespace Stopwatch.Services.Store;

public class StoreService : IStoreService
{
    private const string StopwatchProId = "stopwatch_pro"; // Update this to your actual product ID in Google Play
    private bool? _hasPro = null;

    public async Task<string?> GetPriceAsync()
    {
        try
        {
            var billing = CrossInAppBilling.Current;
            var connected = await billing.ConnectAsync();
            if (!connected)
                return null;

            var items = await billing.GetProductInfoAsync(ItemType.InAppPurchase, new[] { StopwatchProId });
            var product = items?.FirstOrDefault();
            await billing.DisconnectAsync();
            return product?.LocalizedPrice;
        }
        catch
        {
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
                var connected = await billing.ConnectAsync();
                if (!connected)
                {
                    _hasPro = false;
                    return false;
                }

                var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
                _hasPro = purchases?.Any(p => p.ProductId == StopwatchProId) == true;
                await billing.DisconnectAsync();
            }
            catch
            {
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
            var connected = await billing.ConnectAsync();
            if (!connected)
                return false;

            var purchase = await billing.PurchaseAsync(StopwatchProId, ItemType.InAppPurchase, "app_payload");
            await billing.DisconnectAsync();
            if (purchase != null && purchase.State == PurchaseState.Purchased)
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
    }
}
