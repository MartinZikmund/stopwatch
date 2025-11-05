using Uno.RevenueCat.InAppBilling.Enums;

namespace Uno.RevenueCat.InAppBilling.Models;

public sealed record PurchaseResultDto
{
    public bool IsSuccess { get; set; }
    public bool IsError => !(ErrorStatus is null);
    public PurchaseErrorStatus? ErrorStatus { get; set; }
    public StoreTransactionDto? Transaction { get; set; }
    public CustomerInfoDto? CustomerInfo { get; set; }
}
