#if __ANDROID__ && USE_GOOGLE_BILLING_REAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Stopwatch.Droid;

namespace Stopwatch.Services.Store;

/// <summary>
/// Real Google Play Billing implementation. Enable by adding USE_GOOGLE_BILLING_REAL to DefineConstants for Android.
/// WARNING: Consider implementing backend receipt validation before shipping.
/// </summary>
public partial class StoreService : Java.Lang.Object, IPurchasesUpdatedListener, IStoreService
{
	private const string ProProductId = "stopwatch_pro"; // TODO: replace with actual product id from Play Console

	private BillingClient? _billingClient;
	private string? _cachedPrice;
	private bool? _hasPro;

	private BillingClient Billing => _billingClient ??= BillingClient
		.NewBuilder((MainActivity.Current as Context) ?? global::Android.App.Application.Context)
		.EnablePendingPurchases()
		.SetListener(this)
		.Build();

	private Task<bool> EnsureConnectedAsync()
	{
		var tcs = new TaskCompletionSource<bool>();
		if (Billing.IsReady)
		{
			tcs.TrySetResult(true);
			return tcs.Task;
		}
		Billing.StartConnection(new BillingStateListener(
			result => tcs.TrySetResult(result.ResponseCode == BillingResponseCode.Ok),
			() => tcs.TrySetResult(false)));
		return tcs.Task;
	}

	public async Task<string?> GetPriceAsync()
	{
		if (_cachedPrice is not null) return _cachedPrice;
		if (!await EnsureConnectedAsync().ConfigureAwait(false)) return null;
		var (res, details) = await QueryProductDetailsAsync(ProProductId).ConfigureAwait(false);
		if (res.ResponseCode != BillingResponseCode.Ok) return null;
		_cachedPrice = details?.FirstOrDefault()?.OneTimePurchaseOfferDetails?.FormattedPrice;
		return _cachedPrice;
	}

	public async Task<bool> HasProAsync()
	{
		if (_hasPro is not null) return _hasPro.Value;
		if (!await EnsureConnectedAsync().ConfigureAwait(false)) return (_hasPro = false).Value;
		var (result, purchases) = await QueryPurchasesAsync().ConfigureAwait(false);
		_hasPro = result.ResponseCode == BillingResponseCode.Ok && purchases?.Any(p => p.Products.Contains(ProProductId)) == true;
		return _hasPro.Value;
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		if (await HasProAsync().ConfigureAwait(false)) return true;
		if (!await EnsureConnectedAsync().ConfigureAwait(false)) return false;
		var (res, detailsList) = await QueryProductDetailsAsync(ProProductId).ConfigureAwait(false);
		if (res.ResponseCode != BillingResponseCode.Ok) return false;
		var details = detailsList?.FirstOrDefault();
		if (details is null) return false;
		var pdList = new Java.Util.ArrayList();
		pdList.Add(BillingFlowParams.ProductDetailsParams.NewBuilder().SetProductDetails(details).Build());
		var flow = BillingFlowParams.NewBuilder().SetProductDetailsParamsList(pdList).Build();
		var act = MainActivity.Current;
		if (act is null) return false;
		var launch = Billing.LaunchBillingFlow(act, flow);
		return launch.ResponseCode == BillingResponseCode.Ok; // final confirmation via purchase update
	}

	public void OnPurchasesUpdated(BillingResult? billingResult, IList<Purchase>? purchases)
	{
		if (billingResult?.ResponseCode == BillingResponseCode.Ok && purchases != null && purchases.Any(p => p.Products.Contains(ProProductId)))
		{
			_hasPro = true;
		}
	}

	private Task<(BillingResult result, IList<ProductDetails>? details)> QueryProductDetailsAsync(string productId)
	{
		var tcs = new TaskCompletionSource<(BillingResult, IList<ProductDetails>?)>();
		var list = new Java.Util.ArrayList();
		list.Add(QueryProductDetailsParams.Product.NewBuilder().SetProductId(productId).SetProductType(BillingClient.ProductTypeInapp).Build());
		var paramsObj = QueryProductDetailsParams.NewBuilder().SetProductList(list).Build();
		Billing.QueryProductDetailsAsync(paramsObj, new ProductDetailsListener((r, d) => tcs.TrySetResult((r, d))));
		return tcs.Task;
	}

	private Task<(BillingResult result, IList<Purchase>? purchases)> QueryPurchasesAsync()
	{
		var tcs = new TaskCompletionSource<(BillingResult, IList<Purchase>?)>();
		var queryParams = QueryPurchasesParams.NewBuilder().SetProductType(BillingClient.ProductTypeInapp).Build();
		Billing.QueryPurchasesAsync(queryParams, new PurchasesListener((r, p) => tcs.TrySetResult((r, p))));
		return tcs.Task;
	}

	private sealed class BillingStateListener : Java.Lang.Object, IBillingClientStateListener
	{
		private readonly Action<BillingResult> _onSetup; private readonly Action _onDisc;
		public BillingStateListener(Action<BillingResult> onSetup, Action onDisc){_onSetup=onSetup; _onDisc=onDisc;}
		public void OnBillingSetupFinished(BillingResult billingResult)=>_onSetup(billingResult);
		public void OnBillingServiceDisconnected()=>_onDisc();
	}
	private sealed class ProductDetailsListener : Java.Lang.Object, IProductDetailsResponseListener
	{
		private readonly Action<BillingResult, IList<ProductDetails>?> _cb; public ProductDetailsListener(Action<BillingResult, IList<ProductDetails>?> cb)=>_cb=cb; public void OnProductDetailsResponse(BillingResult billingResult, IList<ProductDetails>? productDetails)=>_cb(billingResult, productDetails);
	}
	private sealed class PurchasesListener : Java.Lang.Object, IPurchasesResponseListener
	{
		private readonly Action<BillingResult, IList<Purchase>?> _cb; public PurchasesListener(Action<BillingResult, IList<Purchase>?> cb)=>_cb=cb; public void OnQueryPurchasesResponse(BillingResult billingResult, IList<Purchase>? purchases)=>_cb(billingResult, purchases);
	}
}
#endif
