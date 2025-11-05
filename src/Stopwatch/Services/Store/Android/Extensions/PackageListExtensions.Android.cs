using Com.Revenuecat.Purchases;
using Uno.RevenueCat.InAppBilling.Extensions;
using Uno.RevenueCat.InAppBilling.Models;

namespace Uno.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class PackageListExtensions
{
    internal static List<PackageDto> ToPackageDtoList(this IList<Package> packages)
    {
        var packageDtos = new List<PackageDto>();

        foreach (var package in packages)
        {
            var currencyCode = package.Product.Price.CurrencyCode;
            var price = Convert.ToDecimal(package.Product.Price.AmountMicros * Math.Pow(10, -6));

            var packageDto = new PackageDto()
            {
                OfferingIdentifier = package.PresentedOfferingContext.OfferingIdentifier,
                Identifier = package.Identifier,
                Product = new ProductDto()
                {
                    Pricing = new PricingDto
                    {
                        CurrencyCode = currencyCode,
                        Price = price,
                        PriceMicros = package.Product.Price.AmountMicros,
                        PriceLocalized = PackageDtoExtensions.GetLocalizedPrice(currencyCode, price)
                    },
                    Sku = package.Product.Sku,
                    SubscriptionPeriod = package.Product.Period?.ToString() ?? string.Empty,
                }
            };

            packageDtos.Add(packageDto);
        }

        return packageDtos;
    }
}
