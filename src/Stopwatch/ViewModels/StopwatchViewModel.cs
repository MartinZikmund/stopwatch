using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using Stopwatch.Model;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Timer;
using Windows.UI;

namespace Stopwatch.ViewModels;

public partial class StopwatchViewModel : ObservableObject, IDisposable
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;
	private readonly ITimerFactory _timerProvider;
	private readonly StopwatchService _stopwatchService;
	private readonly DispatcherQueueTimer _timer;

	public StopwatchViewModel(StopwatchModel stopwatch, IDataSource dataSource, ITimerFactory timerProvider, IAppPreferences appPreferences, IDisplayRequestManager displayRequestManager)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
		Laps = new(_stopwatch.Laps);
		_timerProvider = timerProvider;
		_stopwatchService = new StopwatchService(stopwatch, dataSource, appPreferences, displayRequestManager);
		_timer = timerProvider.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(50);
		_timer.Tick += (sender, e) => OnTimePropertiesChanged();
		if (_stopwatchService.IsRunning)
		{
			_timer.Start();
		}
	}

	public LapsObservableCollection Laps { get; }

	public Color BackgroundColor => ColorHelper.ToColor(_stopwatch.BackgroundColor);

	public Uri? BackgroundImageUri => _stopwatch.BackgroundImageUri is not null ? new(_stopwatch.BackgroundImageUri) : null;

	public double BackgroundImageOpacity => _stopwatch.BackgroundImageOpacity;

	public string CurrentTime => _stopwatchService.CurrentTime.ToString(@"hh\:mm\:ss\.");

	public string CurrentTimeFull => _stopwatchService.CurrentTime.ToString(@"hh\:mm\:ss\.ff");

	public string CurrentTimeMilliseconds => (_stopwatchService.CurrentTime.Milliseconds / 10).ToString("D2");

	public bool IsRunning => _stopwatchService.IsRunning;

	public bool IsZero => _stopwatchService.CurrentTime == TimeSpan.Zero;

	[RelayCommand]
	public void StartStop()
	{
		if (IsRunning)
		{
			Stop();
		}
		else
		{
			Start();
		}

		LapCommand?.NotifyCanExecuteChanged();
		ResetCommand?.NotifyCanExecuteChanged();
	}

	[RelayCommand(CanExecute = nameof(CanLap))]
	public void Lap()
	{
		var lapTime = _stopwatchService.AddLap();
		Laps.AddLap(lapTime);
	}

	[RelayCommand(CanExecute = nameof(CanReset))]
	public void Reset()
	{
		_stopwatchService.Reset();
		Laps.Clear();
		OnTimePropertiesChanged();

		LapCommand?.NotifyCanExecuteChanged();
		ResetCommand?.NotifyCanExecuteChanged();
	}

	public void Dispose()
	{
		_timer.Stop();
		_stopwatchService.Dispose();
	}

	private void OnTimePropertiesChanged()
	{
		OnPropertyChanged(nameof(CurrentTime));
		OnPropertyChanged(nameof(CurrentTimeFull));
		OnPropertyChanged(nameof(CurrentTimeMilliseconds));
		OnPropertyChanged(nameof(IsRunning));
		OnPropertyChanged(nameof(IsZero));
	}

	private bool CanLap() => IsRunning;

	private bool CanReset() => !IsZero || IsRunning;

	private void Start()
	{
		_stopwatchService.Start();
		_timer.Start();
		OnTimePropertiesChanged();
	}

	private void Stop()
	{
		_stopwatchService.Stop();
		_timer.Stop();
		OnTimePropertiesChanged();
	}
}
