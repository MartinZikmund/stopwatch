using Stopwatch.Extensions;
using Stopwatch.Models;

namespace Stopwatch.ViewModels;

public class HistoryLapViewModel
{
	public HistoryLapViewModel(int lapNumber, TimeSpan lapTime, TimeSpan totalTime, string note)
	{
		LapNumber = lapNumber;
		LapTimeSpan = lapTime;
		TotalTimeSpan = totalTime;
		Note = note;
	}

	public int LapNumber { get; }

	public TimeSpan LapTimeSpan { get; }

	public TimeSpan TotalTimeSpan { get; }

	public string LapTime => LapTimeSpan.ToStopwatchString(true);

	public string TotalTime => TotalTimeSpan.ToStopwatchString(true);

	public string Note { get; }

	public static HistoryLapViewModel[] CreateFromLaps(LapModel[] laps)
	{
		if (laps == null || laps.Length == 0)
		{
			return Array.Empty<HistoryLapViewModel>();
		}

		var result = new HistoryLapViewModel[laps.Length];
		TimeSpan previousTime = TimeSpan.Zero;

		for (int i = 0; i < laps.Length; i++)
		{
			var lap = laps[i];
			var lapTime = lap.TotalTime - previousTime;
			result[i] = new HistoryLapViewModel(i + 1, lapTime, lap.TotalTime, lap.Note);
			previousTime = lap.TotalTime;
		}

		return result;
	}
}
