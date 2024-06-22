using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using Stopwatch.Model;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public class StopwatchViewModel : ObservableObject
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;
	private readonly ITimerFactory _timerProvider;
	private readonly StopwatchService _stopwatchService;
	private readonly DispatcherQueueTimer _timer;

	public StopwatchViewModel(StopwatchModel stopwatch, IDataSource dataSource, ITimerFactory timerProvider)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
		Laps = new(_stopwatch.Laps);
		_timerProvider = timerProvider;
		_stopwatchService = new StopwatchService(stopwatch, dataSource);
		_timer = timerProvider.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(16);
		_timer.Tick += (sender, e) => OnPropertyChanged("");
		if (_stopwatchService.IsRunning)
		{
			_timer.Start();
		}
	}

	public void Start()
	{
		_stopwatchService.Start();
		_timer.Start();
		OnPropertyChanged("");
	}

	public void Stop()
	{
		_stopwatchService.Stop();
		_timer.Stop();
		OnPropertyChanged("");
	}

	public string CurrentTime => _stopwatchService.CurrentTime.ToString(@"hh\:mm\:ss\.");

	public string CurrentTimeMilliseconds => _stopwatchService.CurrentTime.Milliseconds.ToString("D3");

	public bool IsRunning => _stopwatchService.IsRunning;

	public bool IsZero => _stopwatchService.CurrentTime == TimeSpan.Zero;

	public LapsObservableCollection Laps { get; }

	public void Reset()
	{
		_stopwatchService.Reset();
		Laps.Clear();
		OnPropertyChanged("");
	}

	internal void Lap()
	{
		var lapTime = _stopwatchService.AddLap();
		Laps.AddLap(lapTime);
		OnPropertyChanged(nameof(Laps));
	}
}
