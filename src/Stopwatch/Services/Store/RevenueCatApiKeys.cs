#if __IOS__ || __ANDROID__
namespace Stopwatch.Services.Store;

/// <summary>
/// Platform-specific RevenueCat API keys.
/// These are public API keys that are safe to embed in the app binary.
/// </summary>
internal static class RevenueCatApiKeys
{
#if __IOS__
	private const string ApiKey = "appl_YOUR_IOS_REVENUECAT_PUBLIC_API_KEY";
#elif __ANDROID__
	private const string ApiKey = "goog_YOUR_ANDROID_REVENUECAT_PUBLIC_API_KEY";
#endif

	public static string GetApiKey() => ApiKey;
}
#endif
