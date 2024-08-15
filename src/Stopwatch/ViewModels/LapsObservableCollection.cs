#nullable enable

using System.Collections.ObjectModel;
using Stopwatch.Models;

namespace Stopwatch.ViewModels;

public class LapsObservableCollection : ObservableCollection<LapViewModel>
{
	private readonly StopwatchViewModel _owner;

	public LapsObservableCollection(StopwatchViewModel owner, StopwatchModel stopwatch)
	{
		_owner = owner;
		foreach (var lap in stopwatch.Laps)
		{
			AddLapInner(lap);
		}

		UpdateExtremes();
	}

	public void AddLap(LapModel lap)
	{
		AddLapInner(lap);
		UpdateExtremes();
	}

	private void AddLapInner(LapModel lap)
	{
		var lastTotalTime = Count == 0 ? TimeSpan.Zero : this[0].TotalTime;
		var diff = lap.Time - lastTotalTime;
		Insert(0, new LapViewModel(_owner, lap, Count + 1, diff));
	}

	private void UpdateExtremes()
	{
		if (Count >= 2)
		{
			var fastest = this.OrderBy(l => l.Time).FirstOrDefault();
			var slowest = this.OrderByDescending(l => l.Time).FirstOrDefault();

			foreach (var lap in this)
			{
				lap.IsFastest = lap == fastest;
				lap.IsSlowest = lap == slowest;
			}
		}
	}

	public TimeSpan? AverageLap => Count == 0 ? null : TimeSpan.FromTicks((long)this.Select(l => l.Time.Ticks).Average());
}
