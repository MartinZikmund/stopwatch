using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Dialogs;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Store;
using Stopwatch.Services.Theming;
using Stopwatch.Services.Timer;
using Uno.Disposables;
using Uno.Extensions;

namespace Stopwatch.ViewModels;

public partial class MainViewModel : PageViewModel
{
	private readonly ITimerFactory _timerFactory;
	private readonly IDataSource _dataSource;
	private readonly IHistoryService _historyService;
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly IAppPreferences _appPreferences;
	private readonly IThemeManager _themeManager;
	private readonly IStoreService _storeService;
	private readonly IDialogService _dialogService;
	private readonly IConfirmationDialogService _confirmationDialogService;
	private readonly IDisplayRequestManager _displayRequestManager;
	private readonly DispatcherQueueTimer _timer;
	private readonly SerialDisposable _displayRequestDisposable = new();

	[ObservableProperty]
	private StopwatchViewModel? _selectedStopwatch;

	[ObservableProperty]
	private bool _hasProLicense = true;

	public MainViewModel(
		INavigationService navigationService,
		ITimerFactory timerFactory,
		IDataSource dataSource,
		IHistoryService historyService,
		IWindowShellProvider windowShellProvider,
		IAppPreferences appPreferences,
		IThemeManager themeManager,
		IStoreService storeService,
		IDialogService dialogService,
		IConfirmationDialogService confirmationDialogService,
		IDisplayRequestManager displayRequestManager) : base(navigationService)
	{
		_timerFactory = timerFactory;
		_dataSource = dataSource;
		_historyService = historyService;
		_windowShellProvider = windowShellProvider;
		_appPreferences = appPreferences;
		_themeManager = themeManager;
		_storeService = storeService;
		_dialogService = dialogService;
		_confirmationDialogService = confirmationDialogService;
		_displayRequestManager = displayRequestManager;

		_timer = timerFactory.Create();
		_timer.Interval = TimeSpan.FromMilliseconds(50);
		_timer.Tick += (sender, e) => OnTick();
	}

	private void OnTick()
	{
		SelectedStopwatch?.OnTick();
	}

	public ObservableCollection<StopwatchViewModel> Stopwatches { get; } = new();

	public override async void ViewNavigatedTo(object? parameter)
	{
		ReloadStopwatches();
		HasProLicense = await _storeService.HasProAsync();
	}

	public override void ViewLoaded()
	{
		_timer.Start();
	}

	public override void ViewUnloaded()
	{
		_timer.Stop();
	}

	[MemberNotNull(nameof(SelectedStopwatch))]
	private void ReloadStopwatches()
	{
		var stopwatches = _dataSource.Stopwatches.GetAll();

		if (stopwatches.Length == 0)
		{
			_dataSource.Stopwatches.Add(new StopwatchModel(GetNextStopwatchName()));
			stopwatches = _dataSource.Stopwatches.GetAll();
		}

		for (int i = Stopwatches.Count - 1; i >= 0; i--)
		{
			var existingStopwatch = Stopwatches[i];
			if (_dataSource.Stopwatches.Get(existingStopwatch.Id) is not { } updatedStopwatch)
			{
				Stopwatches.RemoveAt(i);
			}
			else
			{
				bool wasSelected = Stopwatches[i] == SelectedStopwatch;
				var replacementStopwatch = new StopwatchViewModel(updatedStopwatch, _dataSource, _appPreferences, _historyService);
				Stopwatches[i] = replacementStopwatch;
				if (wasSelected)
				{
					SelectedStopwatch = replacementStopwatch;
				}
			}
		}

		// Add missing stopwatches
		foreach (var stopwatch in stopwatches)
		{
			if (Stopwatches.All(s => s.Id != stopwatch.Id))
			{
				Stopwatches.Add(new StopwatchViewModel(stopwatch, _dataSource, _appPreferences, _historyService));
			}
		}

		SelectedStopwatch ??= Stopwatches.First();
	}

	partial void OnSelectedStopwatchChanged(StopwatchViewModel? value)
	{
		if (value is not null)
		{
			_themeManager.SetTheme(value.Stopwatch.Theme);
		}
	}

	[RelayCommand]
	public void GoToSettings()
	{
		if (SelectedStopwatch is null)
		{
			throw new InvalidOperationException("A stopwatch needs to be selected");
		}

		NavigationService.Navigate<SettingsViewModel>(SelectedStopwatch.Id);
	}

	[RelayCommand]
	public void GoToHistory() => NavigationService.Navigate<HistoryViewModel>();

	[RelayCommand]
	public void GoToGetPro() => NavigationService.Navigate<GetProViewModel>();

	[RelayCommand]
	public void ToggleCompactOverlay()
	{
		var newPresenterKind = IsCompactOverlay ? Microsoft.UI.Windowing.AppWindowPresenterKind.Default : Microsoft.UI.Windowing.AppWindowPresenterKind.CompactOverlay;
		_windowShellProvider.Window.AppWindow.SetPresenter(newPresenterKind);

		UpdatePresenterButtons();
	}

	[RelayCommand]
	public void ToggleFullScreen()
	{
		var newPresenterKind = IsFullScreen ? Microsoft.UI.Windowing.AppWindowPresenterKind.Default : Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen;
		_windowShellProvider.Window.AppWindow.SetPresenter(newPresenterKind);

		UpdatePresenterButtons();
	}

	[RelayCommand]
	public void AddStopwatch()
	{
		if (!HasProLicense && Stopwatches.Count > 1)
		{
			var proOnlyFeatureDialog = new ProOnlyFeatureDialog();
			_dialogService.ShowAsync(proOnlyFeatureDialog);
			return;
		}

		var newStopwatch = new StopwatchModel(GetNextStopwatchName());
		_dataSource.Stopwatches.Add(newStopwatch);
		Stopwatches.Add(new StopwatchViewModel(newStopwatch, _dataSource, _appPreferences, _historyService));
		SelectedStopwatch = Stopwatches.Last();
	}

	[RelayCommand]
	public async Task CloseStopwatchAsync(StopwatchViewModel viewModel)
	{
		if (viewModel.IsRunning)
		{
			// Confirm closing
			var result = await _confirmationDialogService.ShowAsync(
				string.Format(Localizer.Instance.GetString("CloseStopwatchDialogTitle"), viewModel.Name),
				string.Format(Localizer.Instance.GetString("CloseStopwatchDialogText"), viewModel.Name));

			if (result != ConfirmationResult.Confirmed)
			{
				return;
			}
		}

		var wasSelected = SelectedStopwatch == viewModel;

		if (_historyService.CanSave(viewModel.Stopwatch))
		{
			_historyService.Save(viewModel.Stopwatch);
		}
		_dataSource.Stopwatches.Delete(viewModel.Id);
		var index = Stopwatches.IndexOf(viewModel);
		Stopwatches.Remove(viewModel);

		if (Stopwatches.Count == 0)
		{
			AddStopwatch();
		}

		if (wasSelected)
		{
			if (index < Stopwatches.Count)
			{
				SelectedStopwatch = Stopwatches[index];
			}
			else
			{
				SelectedStopwatch = Stopwatches.Last();
			}
		}
	}

	private void UpdatePresenterButtons()
	{
		OnPropertyChanged(nameof(IsFullScreen));
		OnPropertyChanged(nameof(IsCompactOverlay));
		OnPropertyChanged(nameof(ShowCompactOverlayButton));
		OnPropertyChanged(nameof(ShowFullScreenButton));
	}

	public bool IsFullScreen => _windowShellProvider.Window.AppWindow.Presenter is FullScreenPresenter;

	public bool IsCompactOverlay => _windowShellProvider.Window.AppWindow.Presenter is CompactOverlayPresenter;

	public bool ShowCompactOverlayButton => !IsFullScreen;

	public bool ShowFullScreenButton => !IsCompactOverlay;

	private string GetNextStopwatchName()
	{
		if (Stopwatches.Count == 0)
		{
			return Localizer.Instance.GetString("StopwatchLabelDefault");
		}

		var i = 2;
		while (true)
		{
			var name = string.Format(Localizer.Instance.GetString("StopwatchLabelNumbered"), i);
			if (Stopwatches.All(s => s.Name != name))
			{
				return name;
			}
			i++;
		}
	}
}
