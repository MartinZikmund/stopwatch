#nullable enable

using System.Collections.ObjectModel;
using Stopwatch.Extensions;
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

	private void ResetOrders()
	{
		for (var i = Count - 1; i >= 0; i--)
		{
			this[i].Order = Count - i;
		}
	}

	public void AddLap(LapModel lap)
	{
		AddLapInner(lap);
		UpdateExtremes();
		UpdateAverageLap();
	}

	public void RemoveLap(LapModel lap)
	{
		var lapViewModel = this.FirstOrDefault(l => l.Lap == lap);
		if (lapViewModel is null)
		{
			return;
		}
		Remove(lapViewModel);
		UpdateExtremes();
		ResetOrders();
		UpdateAverageLap();
	}

	private void AddLapInner(LapModel lap)
	{
		var lastTotalTime = Count == 0 ? TimeSpan.Zero : this[0].TotalTime;
		var diff = lap.TotalTime - lastTotalTime;
		Insert(0, new LapViewModel(_owner, lap, Count + 1, diff));
	}

	private void UpdateExtremes()
	{
		if (Count >= 2)
		{
			var fastest = Enumerable.MinBy(this, l => l.LapTime);
			var slowest = Enumerable.MaxBy(this, l => l.LapTime);

			foreach (var lap in this)
			{
				lap.IsFastest = lap == fastest;
				lap.IsSlowest = lap == slowest;
			}
		}
	}

	public TimeSpan? AverageLap => Count == 0 ? null : TimeSpan.FromTicks((long)this.Select(l => l.LapTime.Ticks).Average());

	public string AverageLapString => AverageLap?.ToStopwatchString(true) ?? string.Empty;

	private void UpdateAverageLap()
	{
		OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(AverageLap)));
		OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(AverageLapString)));
	}
}
