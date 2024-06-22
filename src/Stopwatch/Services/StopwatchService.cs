using Stopwatch.Model;
using Stopwatch.Services.Data;

namespace Stopwatch.Services;

public class StopwatchService
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;

	public StopwatchService(StopwatchModel stopwatch, IDataSource dataSource)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
	}

	public TimeSpan CurrentTime => _stopwatch.LastStartTime is not null ?
		_stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime.Value) :
		_stopwatch.PausedElapsedTime;

	public bool IsRunning => _stopwatch.LastStartTime is not null;

	public void Start()
	{
		if (IsRunning)
		{
			return;
		}

		_stopwatch.LastStartTime = DateTimeOffset.Now;
		_dataSource.Update(_stopwatch);
	}

	public void Stop()
	{
		if (!IsRunning)
		{
			return;
		}

		_stopwatch.PausedElapsedTime = _stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime!.Value);
		_stopwatch.LastStartTime = null;
		_dataSource.Update(_stopwatch);
	}

	public void Reset()
	{
		_stopwatch.LastStartTime = null;
		_stopwatch.PausedElapsedTime = TimeSpan.Zero;
		_stopwatch.Laps = Array.Empty<TimeSpan>();
		_dataSource.Update(_stopwatch);
	}

	internal TimeSpan AddLap()
	{
		var lap = CurrentTime;
		_stopwatch.Laps = _stopwatch.Laps.Append(lap).ToArray();
		_dataSource.Update(_stopwatch);
		return lap;
	}
}
