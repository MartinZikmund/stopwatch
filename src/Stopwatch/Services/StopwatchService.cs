using Stopwatch.Models;
using Stopwatch.Services.Data;
using Stopwatch.Services.Settings;
using Uno.Disposables;

namespace Stopwatch.Services;

public class StopwatchService
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;
	private readonly IAppPreferences _appPreferences;

	public StopwatchService(StopwatchModel stopwatch, IDataSource dataSource, IAppPreferences appPreferences)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
		_appPreferences = appPreferences;
	}

	public TimeSpan CurrentTime => _stopwatch.LastStartTime is not null ?
		_stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime.Value) :
		_stopwatch.PausedElapsedTime;

	public bool IsRunning => _stopwatch.LastStartTime is not null;

	public string Name
	{
		get => _stopwatch.Name;
		set
		{
			_stopwatch.Name = value;
			_dataSource.Stopwatches.Update(_stopwatch);
		}
	}

	public string Icon
	{
		get => _stopwatch.Icon;
		set
		{
			_stopwatch.Icon = value;
			_dataSource.Stopwatches.Update(_stopwatch);
		}
	}

	public void Start()
	{
		if (IsRunning)
		{
			return;
		}

		_stopwatch.InitialStartTime = _stopwatch.InitialStartTime ?? DateTimeOffset.Now;
		_stopwatch.LastStartTime = DateTimeOffset.Now;
		_dataSource.Stopwatches.Update(_stopwatch);
	}

	public void Stop()
	{
		if (!IsRunning)
		{
			return;
		}

		_stopwatch.PausedElapsedTime = _stopwatch.PausedElapsedTime + (DateTimeOffset.UtcNow - _stopwatch.LastStartTime!.Value);
		_stopwatch.LastStartTime = null;
		_dataSource.Stopwatches.Update(_stopwatch);
	}

	public void Reset()
	{
		_stopwatch.LastStartTime = null;
		_stopwatch.Laps = Array.Empty<LapModel>();
		_stopwatch.PausedElapsedTime = TimeSpan.Zero;
		_dataSource.Stopwatches.Update(_stopwatch);
	}

	internal LapModel AddLap()
	{
		var lap = new LapModel(CurrentTime);
		_stopwatch.Laps = _stopwatch.Laps.Append(lap).ToArray();
		_dataSource.Stopwatches.Update(_stopwatch);
		return lap;
	}
}
