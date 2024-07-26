using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Windows.UI;

namespace Stopwatch.Model;

public class StopwatchModel
{
	public int Id { get; set; }

    public DateTimeOffset? LastStartTime { get; set; }

    public TimeSpan PausedElapsedTime { get; set; }

	public TimeSpan[] Laps { get; set; } = Array.Empty<TimeSpan>();

	public string? BackgroundImageUri { get; set; }

	public double BackgroundImageOpacity { get; set; } = 0.8;

	public string BackgroundColor { get; set; } = ColorHelper.ToHex(Colors.Transparent);
}
