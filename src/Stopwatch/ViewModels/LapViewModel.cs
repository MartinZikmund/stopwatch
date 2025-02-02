using Stopwatch.Models;

namespace Stopwatch.ViewModels;

public partial class LapViewModel : ObservableObject
{
	private readonly StopwatchViewModel _owner;

	public LapViewModel(StopwatchViewModel owner, LapModel lap, int order, TimeSpan lapTime)
	{
		_owner = owner;
		Lap = lap;
		Order = order;
		LapTime = lapTime;
	}

	public LapModel Lap { get; }

	public TimeSpan LapTime { get; }

	[ObservableProperty]
	public partial int Order { get; set; }

	[ObservableProperty]
	public partial bool IsFastest { get; set; }

	[ObservableProperty]
	public partial bool IsSlowest { get; set; }

	public TimeSpan TotalTime => Lap.TotalTime;

	public string Note
	{
		get => Lap.Note;
		set
		{
			Lap.Note = value;
			OnPropertyChanged();
			_owner.OnLapUpdated();
		}
	}

	public string TimeString => LapTime.ToString(@"hh\:mm\:ss\.ff");

	public string TotalTimeString => TotalTime.ToString(@"hh\:mm\:ss\.ff");

	[RelayCommand]
	public async Task RequestDeleteAsync() => await _owner.RequestDeleteLapAsync(this);
}
