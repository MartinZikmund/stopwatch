#if __IOS__
using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using StoreKit;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;

namespace Stopwatch.Services.Store;

public class StoreService : NSObject, IStoreService, ISKProductsRequestDelegate, ISKPaymentTransactionObserver
{
    private const string StopwatchProId = "stopwatch_pro"; // Replace with your actual product ID

    private readonly IDialogService _dialogService;
    private TaskCompletionSource<string?>? _priceTcs;
    private TaskCompletionSource<bool>? _purchaseTcs;
    private bool? _hasPro = null;
    private SKProduct? _proProduct;

    public StoreService(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        SKPaymentQueue.DefaultQueue.AddTransactionObserver(this);
    }

    public async Task<string?> GetPriceAsync()
    {
        if (_proProduct != null)
            return _proProduct.LocalizedPrice();

        _priceTcs = new TaskCompletionSource<string?>();
        var request = new SKProductsRequest(new NSSet(StopwatchProId));
        request.Delegate = this;
        request.Start();
        return await _priceTcs.Task;
    }

    public async Task<bool> HasProAsync()
    {
        if (_hasPro.HasValue)
            return _hasPro.Value;

        // Check receipt for purchase
        var receiptUrl = NSBundle.MainBundle.AppStoreReceiptUrl;
        if (receiptUrl != null && NSFileManager.DefaultManager.FileExists(receiptUrl.Path))
        {
            // In production, validate receipt with Apple server
            // For now, just check if file exists
            _hasPro = true;
        }
        else
        {
            _hasPro = false;
        }
        return _hasPro.Value;
    }

    public async Task<bool> TryPurchaseProAsync()
    {
        if (!SKPaymentQueue.CanMakePayments)
        {
            await ShowError("PurchaseNotAllowed");
            return false;
        }

        if (_proProduct == null)
        {
            await GetPriceAsync();
            if (_proProduct == null)
            {
                await ShowError("PurchaseProductUnavailable");
                return false;
            }
        }

        _purchaseTcs = new TaskCompletionSource<bool>();
        var payment = SKPayment.CreateFrom(_proProduct);
        SKPaymentQueue.DefaultQueue.AddPayment(payment);
        return await _purchaseTcs.Task;
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

    // ISKProductsRequestDelegate
    public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
    {
        _proProduct = response.Products.FirstOrDefault(p => p.ProductIdentifier == StopwatchProId);
        _priceTcs?.TrySetResult(_proProduct?.LocalizedPrice());
    }

    public override void RequestFailed(SKRequest request, NSError error)
    {
        _priceTcs?.TrySetResult(null);
    }

    // ISKPaymentTransactionObserver
    public void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
    {
        foreach (var transaction in transactions)
        {
            switch (transaction.TransactionState)
            {
                case SKPaymentTransactionState.Purchased:
                case SKPaymentTransactionState.Restored:
                    _hasPro = true;
                    _purchaseTcs?.TrySetResult(true);
                    queue.FinishTransaction(transaction);
                    break;
                case SKPaymentTransactionState.Failed:
                    _purchaseTcs?.TrySetResult(false);
                    queue.FinishTransaction(transaction);
                    break;
            }
        }
    }

    public void RemovedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions) { }
    public void RestoreCompletedTransactionsFinished(SKPaymentQueue queue) { }
    public void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error) { }
}

public static class SKProductExtensions
{
    public static string? LocalizedPrice(this SKProduct product)
    {
        if (product == null) return null;
        var formatter = new NSNumberFormatter
        {
            FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
            NumberStyle = NSNumberFormatterStyle.Currency,
            Locale = product.PriceLocale
        };
        return formatter.StringFromNumber(product.Price);
    }
}
#endif
