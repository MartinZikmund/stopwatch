using Stopwatch.Model;

namespace Stopwatch.Services;

public class StopwatchService
{
	private readonly StopwatchModel _stopwatch;

	public StopwatchService(StopwatchModel stopwatch)
	{
		_stopwatch = stopwatch;
	}

	public TimeSpan CurrentTime => _stopwatch.LastStartTime is not null ?
		_stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime.Value) :
		_stopwatch.PausedElapsedTime;

	public void Start()
	{
		if (_stopwatch.IsRunning)
		{
			return;
		}

		_stopwatch.LastStartTime = DateTimeOffset.Now;
	}

	public void Stop()
	{
		if (!_stopwatch.IsRunning)
		{
			return;
		}

		_stopwatch.PausedElapsedTime = _stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime!.Value);
		_stopwatch.LastStartTime = null;
	}

	public void Reset()
	{
		_stopwatch.LastStartTime = null;
		_stopwatch.PausedElapsedTime = TimeSpan.Zero;
		_stopwatch.Laps.Clear();
	}
}
