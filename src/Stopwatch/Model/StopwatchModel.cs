namespace Stopwatch.Model;

public class StopwatchModel
{
    public DateTimeOffset? LastStartTime { get; set; }

    public TimeSpan PausedElapsedTime { get; set; }

    public bool IsRunning => LastStartTime is not null;
}
