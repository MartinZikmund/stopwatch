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
			Add(new LapViewModel(Count + 1, lap));
		}
		UpdateExtremes();
	}

	public void AddLap(TimeSpan lap)
	{
		Add(new LapViewModel(Count + 1, lap));
		UpdateExtremes();
	}

	private void UpdateExtremes()
	{
		if (Count >= 2)
		{
			var fastest = this.OrderBy(l => l.LapTime).FirstOrDefault();
			var slowest = this.OrderByDescending(l => l.LapTime).FirstOrDefault();

			foreach (var lap in this)
			{
				lap.IsFastest = lap == fastest;
				lap.IsSlowest = lap == slowest;
			}
		}
	}

	public TimeSpan? AverageLap => Count == 0 ? null : TimeSpan.FromTicks((long)this.Select(l => l.LapTime.Ticks).Average());
}
