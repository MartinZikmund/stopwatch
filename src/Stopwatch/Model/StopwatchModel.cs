namespace Stopwatch.Model;

public class StopwatchModel
{
	public int Id { get; set; }

    public DateTimeOffset? LastStartTime { get; set; }

    public TimeSpan PausedElapsedTime { get; set; }

	public TimeSpan[] Laps { get; set; } = Array.Empty<TimeSpan>();
}
