namespace Stopwatch.Helpers;

internal static class LayoutConstants
{
	public const double CompactLandscapeMaxHeight = 450;

	public static bool IsCompactLandscape(double width, double height) =>
		width > height && height < CompactLandscapeMaxHeight;
}
