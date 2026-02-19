namespace Stopwatch.Services.Store;

public record RevenueCatConfig
{
	public string IOSApiKey { get; init; } = string.Empty;
	public string AndroidApiKey { get; init; } = string.Empty;
	public string EntitlementId { get; init; } = string.Empty;
	public string IOSProProductId { get; init; } = string.Empty;
	public string AndroidProProductId { get; init; } = string.Empty;
}
