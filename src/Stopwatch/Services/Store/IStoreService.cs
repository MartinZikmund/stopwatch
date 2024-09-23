namespace Stopwatch.Services.Store;

public interface IStoreService
{
	Task<bool> HasProAsync();

	Task<bool> TryPurchaseProAsync();
}
