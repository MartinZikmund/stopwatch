using Stopwatch.Model;
using Stopwatch.Services.Data;
using Uno.Disposables;

namespace Stopwatch.Services;

public class StopwatchService
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;
	private readonly IDisplayRequestManager _displayRequestManager;
	private readonly SerialDisposable _displayRequestDisposable = new();

	public StopwatchService(StopwatchModel stopwatch, IDataSource dataSource, IDisplayRequestManager displayRequestManager)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
		_displayRequestManager = displayRequestManager;
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
		_displayRequestDisposable.Disposable = _displayRequestManager.RequestActive();
	}

	public void Stop()
	{
		if (!IsRunning)
		{
			return;
		}

		_stopwatch.LastStartTime = null;
		_stopwatch.PausedElapsedTime = _stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime!.Value);
		_dataSource.Update(_stopwatch);
		_displayRequestDisposable.Disposable = null;
	}

	public void Reset()
	{
		_stopwatch.LastStartTime = null;
		_stopwatch.Laps = Array.Empty<TimeSpan>();
		_stopwatch.PausedElapsedTime = TimeSpan.Zero;
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
