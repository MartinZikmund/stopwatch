using System.ComponentModel;
using Microsoft.UI.Dispatching;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Dialogs;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public partial class StopwatchWindowViewModel : PageViewModel
{
	private readonly ITimerFactory _timerFactory;
	private readonly IDataSource _dataSource;
	private readonly IHistoryService _historyService;
	private readonly IAppPreferences _appPreferences;
	private readonly IConfirmationDialogService _confirmationDialogService;
	private readonly DispatcherQueueTimer _timer;

	[ObservableProperty]
	public partial StopwatchViewModel? Stopwatch { get; set; }

	public StopwatchWindowViewModel(
		INavigationService navigationService,
		ITimerFactory timerFactory,
		IDataSource dataSource,
		IHistoryService historyService,
		IAppPreferences appPreferences,
		IConfirmationDialogService confirmationDialogService) : base(navigationService)
	{
		_timerFactory = timerFactory;
		_dataSource = dataSource;
		_historyService = historyService;
		_appPreferences = appPreferences;
		_confirmationDialogService = confirmationDialogService;

		_timer = timerFactory.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(50);
		_timer.Tick += (sender, e) => OnTick();
	}

	private void OnTick()
	{
		Stopwatch?.OnTick();
	}

	public override void ViewNavigatedTo(object? parameter)
	{
		// Load the specific stopwatch by ID
		if (parameter is int stopwatchId)
		{
			var stopwatchModel = _dataSource.Stopwatches.Get(stopwatchId);
			if (stopwatchModel != null)
			{
				Stopwatch = new StopwatchViewModel(stopwatchModel, _dataSource, _appPreferences, _historyService, _confirmationDialogService);
				Stopwatch.PropertyChanged += OnStopwatchPropertyChanged;
				UpdateTitle();
			}
		}
	}

	private void OnStopwatchPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(StopwatchViewModel.Name))
		{
			UpdateTitle();
		}
	}

	private void UpdateTitle()
	{
		if (Stopwatch != null)
		{
			Title = $"Fluent Stopwatch - {Stopwatch.Name}";
		}
	}

	public override void ViewLoaded()
	{
		_timer.Start();
	}

	public override void ViewUnloaded()
	{
		_timer.Stop();
		
		if (Stopwatch != null)
		{
			Stopwatch.PropertyChanged -= OnStopwatchPropertyChanged;
		}
	}
}
