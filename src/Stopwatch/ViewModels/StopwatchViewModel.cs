using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Timer;
using Windows.UI;

namespace Stopwatch.ViewModels;

public partial class StopwatchViewModel : ObservableObject
{
	private readonly StopwatchModel _stopwatch;
	private readonly IDataSource _dataSource;
	private readonly IHistoryService _historyService;
	private readonly StopwatchService _stopwatchService;

	public StopwatchViewModel(
		StopwatchModel stopwatch,
		IDataSource dataSource,
		IAppPreferences appPreferences,
		IHistoryService historyService)
	{
		_stopwatch = stopwatch;
		_dataSource = dataSource;
		Laps = new(this, stopwatch);
		IsLapsListExpanded = Laps.Count > 0;
		_historyService = historyService;
		_stopwatchService = new StopwatchService(stopwatch, dataSource, appPreferences);
	}

	public int Id => _stopwatch.Id;

	public StopwatchModel Stopwatch => _stopwatch;

	public LapsObservableCollection Laps { get; }

	[ObservableProperty]
	public partial bool IsLapsListExpanded { get; set; }

	public Color BackgroundColor => ColorHelper.ToColor(_stopwatch.BackgroundColor);

	public Uri? BackgroundImageUri => _stopwatch.BackgroundImageUri is not null ? new(_stopwatch.BackgroundImageUri) : null;

	public double BackgroundImageOpacity => _stopwatch.BackgroundImageOpacity;

	public string CurrentTime => _stopwatchService.CurrentTime.ToString(@"hh\:mm\:ss\.");

	public string CurrentTimeFull => _stopwatchService.CurrentTime.ToString(@"hh\:mm\:ss\.ff");

	public string CurrentTimeMilliseconds => (_stopwatchService.CurrentTime.Milliseconds / 10).ToString("D2");

	public bool IsRunning => _stopwatchService.IsRunning;

	public bool IsZero => _stopwatchService.CurrentTime == TimeSpan.Zero;

	public string Icon
	{
		get => _stopwatchService.Icon;
		set
		{
			_stopwatchService.Icon = value;
			OnPropertyChanged();
		}
	}

	public string Name
	{
		get => _stopwatchService.Name;
		set
		{
			_stopwatchService.Name = value;
			OnPropertyChanged();
		}
	}

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
		IsLapsListExpanded = true;
	}

	[RelayCommand(CanExecute = nameof(CanReset))]
	public void Reset()
	{
		if (_stopwatch.InitialStartTime is not null &&
			(_stopwatch.Laps.Any() || !IsRunning))
		{
			_historyService.Save(_stopwatch);
		}

		_stopwatchService.Reset();
		IsLapsListExpanded = false;
		Laps.Clear();
		OnTick();

		LapCommand?.NotifyCanExecuteChanged();
		ResetCommand?.NotifyCanExecuteChanged();
	}

	public void OnTick()
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
		OnTick();
	}

	private void Stop()
	{
		_stopwatchService.Stop();
		OnTick();
	}

	internal void OnLapUpdated() => _dataSource.Stopwatches.Update(_stopwatch);
}
