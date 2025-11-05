using System.Collections;
using Uno.RevenueCat.InAppBilling.Extensions;
using Org.Json;

namespace Uno.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class IDictonaryExtensions
{
    internal static string? ToJson<T, U>(this IDictionary<T, U> dictionary)
    {
        return dictionary.IsNullOrEmpty()
            ? null
            : new JSONObject((IDictionary)dictionary).ToString();
    }
}