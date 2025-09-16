#if __ANDROID__ && USE_GOOGLE_BILLING
// Placeholder minimal implementation due to build environment limitations (Google Play Billing bindings unavailable).
// TODO: Integrate direct Google Play BillingClient implementation when Android dependencies can be restored.
namespace Stopwatch.Services.Store;
public class StoreService : IStoreService
{
	public Task<string?> GetPriceAsync() => Task.FromResult<string?>(null);
	public Task<bool> HasProAsync() => Task.FromResult(false);
	public Task<bool> TryPurchaseProAsync() => Task.FromResult(false);
}
#else
namespace Stopwatch.Services.Store; public class StoreService : IStoreService { public Task<string?> GetPriceAsync()=>Task.FromResult<string?>(null); public Task<bool> HasProAsync()=>Task.FromResult(false); public Task<bool> TryPurchaseProAsync()=>Task.FromResult(false);} 
#endif
