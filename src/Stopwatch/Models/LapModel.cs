namespace Stopwatch.Models;

public class LapModel
{
	public LapModel()
	{
	}

	public LapModel(TimeSpan time)
	{
		Time = time;
	}

	public TimeSpan Time { get; set; }

	public string Note { get; set; } = "";
}
