namespace Stopwatch.ViewModels;

public partial class LapViewModel : ObservableObject
{
	[ObservableProperty]
	private bool _isFastest;

	[ObservableProperty]
	private bool _isSlowest;

	public LapViewModel(int order, TimeSpan lapTime)
	{
		Order = order;
		LapTime = lapTime;
	}

	public int Order { get; }

	public TimeSpan LapTime { get; }

	/// <summary>
	/// Output in the format 00:33:23.23
	/// </summary>
	public string LapTimeString => LapTime.ToString(@"hh\:mm\:ss\.ff");
}
