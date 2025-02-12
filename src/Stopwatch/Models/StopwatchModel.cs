using System.Text;
using System.Text.Json;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Windows.UI;

namespace Stopwatch.Models;

public class StopwatchModel : IId
{
	public StopwatchModel()
	{		
	}

	public StopwatchModel(string name)
	{
		Name = name;	
	}

	public int Id { get; set; }

	public string Icon { get; set; }

	public string Name { get; set; }

	public DateTimeOffset? InitialStartTime { get; set; }

	public DateTimeOffset? LastStartTime { get; set; }

	public TimeSpan PausedElapsedTime { get; set; }

	public LapModel[] Laps { get; set; } = Array.Empty<LapModel>();

	public ElementTheme Theme { get; set; }

	public string? BackgroundImageUri { get; set; }

	public double BackgroundImageOpacity { get; set; } = 0.8;

	public string BackgroundColor { get; set; } = ColorHelper.ToHex(Colors.Transparent);

	public string LapsToCsv()
	{
		var csv = new StringBuilder();
		csv.AppendLine("LapTime,TotalTime,Note");

		for (var i = 0; i < Laps.Length; i++)
		{
			var lap = Laps[i];
			var previousLapTime = i > 0 ? Laps[i-1].TotalTime : TimeSpan.Zero;
			var lapTime = lap.TotalTime - previousLapTime;
			csv.AppendLine($"{lapTime},{lap.TotalTime},{lap.Note}");
		}

		return csv.ToString();
	}

	public string ToJson()
	{
		return JsonSerializer.Serialize(this);
	}

	public string ToXml()
	{
		var xml = new StringBuilder();
		xml.AppendLine("<Stopwatch>");
		xml.AppendLine($"<Name>{Name}</Name>");
		xml.AppendLine($"<InitialStartTime>{InitialStartTime}</InitialStartTime>");
		xml.AppendLine($"<LastStartTime>{LastStartTime}</LastStartTime>");
		xml.AppendLine($"<PausedElapsedTime>{PausedElapsedTime}</PausedElapsedTime>");
		xml.AppendLine($"<Laps>");
		for (var i = 0; i < Laps.Length; i++)
		{
			var lap = Laps[i];
			var previousLapTime = i > 0 ? Laps[i-1].TotalTime : TimeSpan.Zero;
			xml.AppendLine($"<Lap>");
			xml.AppendLine($"<LapTime>{lap.TotalTime - previousLapTime}</LapTime>");
			xml.AppendLine($"<TotalTime>{lap.TotalTime}</TotalTime>");
			xml.AppendLine($"<Note>{lap.Note}</Note>");
			xml.AppendLine("</Lap>");
		}
		xml.AppendLine("</Laps>");
		xml.AppendLine("</Stopwatch>");

		return xml.ToString();
	}
}
