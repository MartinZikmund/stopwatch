using Stopwatch.Model;
using Stopwatch.Services.Data;
using Stopwatch.Services.Settings;
using Uno.Disposables;

namespace Stopwatch.Services;

public class StopwatchService : IDisposable
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;
	private readonly IAppPreferences _appPreferences;
	private readonly IDisplayRequestManager _displayRequestManager;
	private readonly SerialDisposable _displayRequestDisposable = new();

	public StopwatchService(StopwatchModel stopwatch, IDataSource dataSource, IAppPreferences appPreferences, IDisplayRequestManager displayRequestManager)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
		_appPreferences = appPreferences;
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

		if (_appPreferences.KeepScreenOn)
		{
			_displayRequestDisposable.Disposable = _displayRequestManager.RequestActive();
		}
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

	public void Dispose()
	{
		_displayRequestDisposable.Disposable = null;
	}
}
