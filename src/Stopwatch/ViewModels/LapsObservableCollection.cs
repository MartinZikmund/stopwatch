using System.Collections.ObjectModel;

namespace Stopwatch.ViewModels;

public class LapsObservableCollection : ObservableCollection<LapViewModel>
{
	public LapsObservableCollection()
	{
	}

	public LapsObservableCollection(IEnumerable<TimeSpan> laps)
	{
		foreach (var lap in laps)
		{
			AddLapInner(lap);
		}

		UpdateExtremes();
	}

	public void AddLap(TimeSpan lapTime)
	{
		AddLapInner(lapTime);
		UpdateExtremes();
	}

	private void AddLapInner(TimeSpan lapTime)
	{
		var lastTotalTime = Count == 0 ? TimeSpan.Zero : this[0].TotalTime;
		var diff = lapTime - lastTotalTime;
		Insert(0, new LapViewModel(Count + 1, diff, lapTime));
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
