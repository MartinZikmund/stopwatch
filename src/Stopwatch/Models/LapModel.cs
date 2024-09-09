namespace Stopwatch.Models;

public class LapModel
{
	public LapModel()
	{
	}

	public LapModel(TimeSpan time)
	{
		TotalTime = time;
	}

	public TimeSpan TotalTime { get; set; }

	public string Note { get; set; } = "";
}
