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
	}

	public void AddLap(TimeSpan lap)
	{
		Add(new LapViewModel(Count + 1, lap));
	}
}
