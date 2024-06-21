using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using Stopwatch.Model;
using Stopwatch.Services;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public class StopwatchViewModel : ObservableObject
{
	private readonly StopwatchModel _stopwatch;
	private readonly ITimerFactory _timerProvider;
	private readonly StopwatchService _stopwatchService;
	private readonly DispatcherQueueTimer _timer;

	public StopwatchViewModel(StopwatchModel stopwatch, ITimerFactory timerProvider)
	{
		_stopwatch = stopwatch;
		Laps = new(_stopwatch.Laps);
		_timerProvider = timerProvider;
		_stopwatchService = new StopwatchService(stopwatch);
		_timer = timerProvider.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(16);
		_timer.Tick += (sender, e) => OnPropertyChanged("");
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

	public bool IsRunning => _stopwatch.IsRunning;

	public ObservableCollection<LapViewModel> Laps { get; }

	public void Reset()
	{
		_stopwatchService.Reset();
		OnPropertyChanged("");
	}

	internal void Lap()
	{
		_stopwatch.Laps.Add(_stopwatchService.CurrentTime);
		Laps.Add(new(_stopwatchService.CurrentTime));
		OnPropertyChanged(nameof(Laps));
	}
}
