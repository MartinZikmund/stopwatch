using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Windows.UI;

namespace Stopwatch.Models;

public class StopwatchModel : IId
{
	public int Id { get; set; }

	public string Icon { get; set; }

	public string Name { get; set; }

	public DateTimeOffset? InitialStartTime { get; set; }

    public DateTimeOffset? LastStartTime { get; set; }

    public TimeSpan PausedElapsedTime { get; set; }

	public LapModel[] Laps { get; set; } = Array.Empty<LapModel>();

	public string? BackgroundImageUri { get; set; }

	public double BackgroundImageOpacity { get; set; } = 0.8;

	public string BackgroundColor { get; set; } = ColorHelper.ToHex(Colors.Transparent);
}
