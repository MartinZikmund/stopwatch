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

	public LapViewModel(StopwatchViewModel owner, LapModel lap, int order, TimeSpan time)
	{
		_owner = owner;
		_lap = lap;
		Order = order;
		Time = time;
	}

	public int Order { get; }

	public TimeSpan Time { get; }

	public TimeSpan TotalTime => _lap.Time;

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

	public string TimeString => Time.ToString(@"hh\:mm\:ss\.ff");

	public string TotalTimeString => TotalTime.ToString(@"hh\:mm\:ss\.ff");
}
