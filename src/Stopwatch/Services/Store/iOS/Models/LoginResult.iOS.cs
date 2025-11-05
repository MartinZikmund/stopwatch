using Maui.RevenueCat.iOS;

namespace Uno.RevenueCat.InAppBilling.Platforms.iOS.Models;

public sealed class LoginResult
{
    public RCCustomerInfo CustomerInfo { get; }
    public bool Created { get; }

    public LoginResult(RCCustomerInfo customerInfo, bool created)
    {
        CustomerInfo = customerInfo;
        Created = created;
    }
}
