using Microsoft.UI;

namespace Stopwatch.Models;

public class HistoryEntryModel : IId
{
	public HistoryEntryModel()
	{
	}

	public HistoryEntryModel(
		string icon,
		string name,
		DateTimeOffset initialStartTime,
		TimeSpan pausedElapsedTime,
		LapModel[] laps,
		string? backgroundImageUri,
		double backgroundImageOpacity,
		string backgroundColor)
	{
		Icon = icon;
		Name = name;
		InitialStartTime = initialStartTime;
		PausedElapsedTime = pausedElapsedTime;
		Laps = laps;
		BackgroundImageUri = backgroundImageUri;
		BackgroundImageOpacity = backgroundImageOpacity;
		BackgroundColor = backgroundColor;
	}

	public string Id { get; set; }

	public string Icon { get; set; }

	public string Name { get; set; }

	public DateTimeOffset? InitialStartTime { get; set; }

	public TimeSpan PausedElapsedTime { get; set; }

	public LapModel[] Laps { get; set; } = Array.Empty<LapModel>();

	public string? BackgroundImageUri { get; set; }

	public double BackgroundImageOpacity { get; set; } = 0.8;

	public string BackgroundColor { get; set; } = ColorHelper.ToHex(Colors.Transparent);
}
