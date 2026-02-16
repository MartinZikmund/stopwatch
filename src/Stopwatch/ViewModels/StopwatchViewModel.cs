using System.Diagnostics;
using System.Text;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using Stopwatch.Extensions;
using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Timer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;

namespace Stopwatch.ViewModels;

public partial class StopwatchViewModel : ObservableObject
{
	private readonly StopwatchModel _stopwatch;
	private readonly IConfirmationDialogService _confirmationDialogService;
	private readonly IDataSource _dataSource;
	private readonly IHistoryService _historyService;
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly StopwatchService _stopwatchService;

	public StopwatchViewModel(
		StopwatchModel stopwatch,
		IDataSource dataSource,
		IAppPreferences appPreferences,
		IHistoryService historyService,
		IConfirmationDialogService confirmationDialogService,
		IWindowShellProvider windowShellProvider)
	{
		_stopwatch = stopwatch;
		_confirmationDialogService = confirmationDialogService;
		_dataSource = dataSource;
		_windowShellProvider = windowShellProvider;
		Laps = new(this, stopwatch);
		IsLapsListExpanded = Laps.Count > 0;
		_historyService = historyService;
		_stopwatchService = new StopwatchService(stopwatch, dataSource, appPreferences);
	}

	public int Id => _stopwatch.Id;

	public StopwatchModel Stopwatch => _stopwatch;

	[ObservableProperty]
	public partial LapsObservableCollection Laps { get; private set; }

	[ObservableProperty]
	public partial bool IsLapsListExpanded { get; set; }

	[ObservableProperty]
	public partial bool IsPoppedOut { get; set; }

	public Color BackgroundColor => ColorHelper.ToColor(_stopwatch.BackgroundColor);

	public Uri? BackgroundImageUri => _stopwatch.BackgroundImageUri is not null ? new(_stopwatch.BackgroundImageUri) : null;

	public double BackgroundImageOpacity => _stopwatch.BackgroundImageOpacity;

	public string CurrentTime => _stopwatchService.CurrentTime.ToStopwatchString(false);

	public string CurrentTimeFull => _stopwatchService.CurrentTime.ToStopwatchString(true);

	public string CurrentTimeMilliseconds => _stopwatchService.CurrentTime.ToFractionsOfSecondsString();

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

	private bool CanLap() => !IsZero;

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

	[RelayCommand]
	public async Task ExportToJsonAsync()
	{
		try
		{
			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			savePicker.FileTypeChoices.Add("JSON files", new List<string> { ".json" });
			savePicker.SuggestedFileName = $"{Name}_export_{DateTime.Now:yyyyMMdd_HHmmss}";

			var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_windowShellProvider.Window);
			WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

			var file = await savePicker.PickSaveFileAsync();
			if (file != null)
			{
				var jsonContent = _stopwatch.ToJson();
				await FileIO.WriteTextAsync(file, jsonContent);
			}
		}
		catch (Exception)
		{
		}
	}

	[RelayCommand]
	public async Task ExportToCsvAsync()
	{
		try
		{
			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			savePicker.FileTypeChoices.Add("CSV files", new List<string> { ".csv" });
			savePicker.SuggestedFileName = $"{Name}_laps_{DateTime.Now:yyyyMMdd_HHmmss}";

			var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_windowShellProvider.Window);
			WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

			var file = await savePicker.PickSaveFileAsync();
			if (file != null)
			{
				var csvContent = _stopwatch.LapsToCsv();
				await FileIO.WriteTextAsync(file, csvContent);
			}
		}
		catch (Exception)
		{
		}
	}

	internal void OnLapUpdated() => _dataSource.Stopwatches.Update(_stopwatch);

	internal async Task RequestDeleteLapAsync(LapViewModel lapViewModel)
	{
		var result = await _confirmationDialogService.ShowAsync(Localizer.Instance.GetString("DeleteLapDialogTitle"), Localizer.Instance.GetString("DeleteLapDialogText"));
		if (result != ConfirmationResult.Confirmed)
		{
			return;
		}

		_stopwatchService.RemoveLap(lapViewModel.Lap);
		Laps = new(this, _stopwatch);
	}
}
