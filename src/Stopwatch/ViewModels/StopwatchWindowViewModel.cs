using Microsoft.UI.Dispatching;
using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;
using Stopwatch.Services.Timer;

namespace Stopwatch.ViewModels;

public partial class StopwatchWindowViewModel : PageViewModel
{
	private readonly IDataSource _dataSource;
	private readonly IAppPreferences _appPreferences;
	private readonly IHistoryService _historyService;
	private readonly IConfirmationDialogService _confirmationDialogService;
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly IThemeManager _themeManager;
	private readonly ITimerFactory _timerFactory;
	private DispatcherQueueTimer? _timer;

	[ObservableProperty]
	public partial StopwatchViewModel? Stopwatch { get; set; }

	public StopwatchWindowViewModel(
		INavigationService navigationService,
		IDataSource dataSource,
		IAppPreferences appPreferences,
		IHistoryService historyService,
		IConfirmationDialogService confirmationDialogService,
		IWindowShellProvider windowShellProvider,
		IThemeManager themeManager,
		ITimerFactory timerFactory) : base(navigationService)
	{
		_dataSource = dataSource;
		_appPreferences = appPreferences;
		_historyService = historyService;
		_confirmationDialogService = confirmationDialogService;
		_windowShellProvider = windowShellProvider;
		_themeManager = themeManager;
		_timerFactory = timerFactory;
	}

	public override void ViewNavigatedTo(object? parameter)
	{
		if (parameter is int stopwatchId)
		{
			var stopwatchModel = _dataSource.Stopwatches.Get(stopwatchId);
			if (stopwatchModel is null)
			{
				return;
			}

			Stopwatch = new StopwatchViewModel(
				stopwatchModel,
				_dataSource,
				_appPreferences,
				_historyService,
				_confirmationDialogService,
				_windowShellProvider);

			Title = stopwatchModel.Name;
			_themeManager.SetTheme(stopwatchModel.Theme);
		}
	}

	public override void ViewLoaded()
	{
		_timer = _timerFactory.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(50);
		_timer.Tick += (sender, e) => OnTick();
		_timer.Start();
	}

	public override void ViewUnloaded()
	{
		_timer?.Stop();
	}

	private void OnTick()
	{
		Stopwatch?.OnTick();
	}
}
