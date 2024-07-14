namespace Stopwatch.ViewModels;

public partial class LapViewModel : ObservableObject
{
	[ObservableProperty]
	private bool _isFastest;

	[ObservableProperty]
	private bool _isSlowest;

	public LapViewModel(int order, TimeSpan time, TimeSpan totalTime)
	{
		Order = order;
		Time = time;
		TotalTime = totalTime;
	}

	public int Order { get; }

	public TimeSpan Time { get; }

	public TimeSpan TotalTime { get; }

	public string TimeString => Time.ToString(@"hh\:mm\:ss\.ff");

	public string TotalTimeString => TotalTime.ToString(@"hh\:mm\:ss\.ff");
}
