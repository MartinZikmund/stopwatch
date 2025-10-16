namespace Stopwatch.Services.Store;

public interface IStoreService
{
	Task<string?> GetPriceAsync();

	Task<bool> HasProAsync();

	Task<bool> TryPurchaseProAsync();

	Task<bool> TryRestorePurchasesAsync();
}
