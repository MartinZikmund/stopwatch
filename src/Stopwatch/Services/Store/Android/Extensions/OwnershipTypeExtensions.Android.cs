using Uno.RevenueCat.InAppBilling.Enums;
using OwnershipTypeNative = Com.Revenuecat.Purchases.OwnershipType;

namespace Uno.RevenueCat.InAppBilling.Platforms.Android.Extensions;
internal static class OwnershipTypeExtensions
{
    internal static OwnershipType ToDtoOwnershipType(this OwnershipTypeNative ownershipType)
    {
        if (ownershipType == OwnershipTypeNative.FamilyShared) return OwnershipType.FamilyShared;
        if (ownershipType == OwnershipTypeNative.Purchased) return OwnershipType.Purchased;
        return OwnershipType.Unknown;
    }
}
