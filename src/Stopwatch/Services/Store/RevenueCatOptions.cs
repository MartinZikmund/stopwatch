namespace Stopwatch.Services.Store;

public class RevenueCatOptions
{
	public const string SectionName = "RevenueCat";

	public string iOSApiKey { get; set; } = string.Empty;
	public string AndroidApiKey { get; set; } = string.Empty;
}
