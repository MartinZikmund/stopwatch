using Stopwatch.Models;

namespace Stopwatch.ViewModels;

public partial class LapViewModel : ObservableObject
{
	private readonly StopwatchViewModel _owner;
	private readonly LapModel _lap;

	[ObservableProperty]
	private bool _isFastest;

	[ObservableProperty]
	private bool _isSlowest;

	public LapViewModel(StopwatchViewModel owner, LapModel lap, int order, TimeSpan lapTime)
	{
		_owner = owner;
		_lap = lap;
		Order = order;
		LapTime = lapTime;
	}

	public int Order { get; }

	public TimeSpan LapTime { get; }

	public TimeSpan TotalTime => _lap.TotalTime;

	public string Note
	{
		get => _lap.Note;
		set
		{
			_lap.Note = value;
			OnPropertyChanged();
			_owner.OnLapUpdated();
		}
	}

	public string TimeString => LapTime.ToString(@"hh\:mm\:ss\.ff");

	public string TotalTimeString => TotalTime.ToString(@"hh\:mm\:ss\.ff");
}
