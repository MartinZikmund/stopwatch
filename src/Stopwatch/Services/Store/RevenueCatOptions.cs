namespace Stopwatch.Services.Store;

public class RevenueCatOptions
{
	public const string SectionName = "RevenueCat";

	public string IOSApiKey { get; set; } = string.Empty;
	public string AndroidApiKey { get; set; } = string.Empty;
	public string EntitlementId { get; set; } = string.Empty;
	public string IOSProProductId { get; set; } = string.Empty;
	public string AndroidProProductId { get; set; } = string.Empty;
}
