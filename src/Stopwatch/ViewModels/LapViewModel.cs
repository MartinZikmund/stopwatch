namespace Stopwatch.ViewModels;

public class LapViewModel : ObservableObject
{
	private readonly TimeSpan _lapTime;

	public LapViewModel(int order, TimeSpan lapTime)
	{
		Order = order;
		_lapTime = lapTime;
	}

	public int Order { get; }

	/// <summary>
	/// Output in the format 00:33:23.23
	/// </summary>
	public string LapTime => _lapTime.ToString(@"hh\:mm\:ss\.ff");
}
