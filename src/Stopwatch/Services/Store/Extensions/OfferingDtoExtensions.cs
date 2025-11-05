using Uno.RevenueCat.InAppBilling.Models;

namespace Uno.RevenueCat.InAppBilling.Extensions;

public static partial class OfferingDtoExtensions
{
    public static OfferingDto? GetCurrent(this List<OfferingDto> offerings)
    {
        return offerings.FirstOrDefault(x => x.IsCurrent);
    }
}
