using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Export;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public partial class HistoryDetailViewModel : PageViewModel
{
	private readonly IHistoryService _historyService;
	private readonly IExportService _exportService;
	private readonly IDataSource _dataSource;
	private readonly IConfirmationDialogService _confirmationDialogService;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(LapViewModels))]
	public partial HistoryEntryModel? HistoryEntry { get; set; }

	public HistoryLapViewModel[] LapViewModels =>
		HistoryEntry?.Laps != null
			? HistoryLapViewModel.CreateFromLaps(HistoryEntry.Laps)
			: Array.Empty<HistoryLapViewModel>();

	public HistoryDetailViewModel(
		INavigationService navigationService,
		IHistoryService historyService,
		IExportService exportService,
		IDataSource dataSource,
		IConfirmationDialogService confirmationDialogService) : base(navigationService)
	{
		_historyService = historyService;
		_exportService = exportService;
		_dataSource = dataSource;
		_confirmationDialogService = confirmationDialogService;
	}

	public override void ViewNavigatedTo(object? parameter)
	{
		base.ViewNavigatedTo(parameter);

		if (parameter is int historyEntryId)
		{
			HistoryEntry = _historyService.GetAll()
				.FirstOrDefault(h => h.Id == historyEntryId);
		}
	}

	[RelayCommand]
	public async Task DeleteAsync()
	{
		if (HistoryEntry == null)
		{
			return;
		}

		var dialogName = "DeleteHistoryEntryDialog";
		var result = await _confirmationDialogService.ShowAsync(
			Localizer.Instance.GetString($"{dialogName}Title"),
			Localizer.Instance.GetString($"{dialogName}Text"));

		if (result == ConfirmationResult.Confirmed)
		{
			_historyService.Delete(HistoryEntry);
			NavigationService.GoBack();
		}
	}

	[RelayCommand]
	public async Task RestoreAsync()
	{
		if (HistoryEntry == null)
		{
			return;
		}

		var newStopwatch = StopwatchModel.FromHistoryEntry(HistoryEntry);
		_dataSource.Stopwatches.Add(newStopwatch);
		var id = newStopwatch.Id;

		NavigationService.Navigate<MainViewModel>(id);
		await Task.CompletedTask;
	}

	[RelayCommand]
	public async Task ExportToJsonAsync()
	{
		if (HistoryEntry == null)
		{
			return;
		}

		await _exportService.ExportToJsonAsync(
			HistoryEntry,
			$"{HistoryEntry.Name}_export_{DateTime.Now:yyyyMMdd_HHmmss}");
	}

	[RelayCommand]
	public async Task ExportToCsvAsync()
	{
		if (HistoryEntry == null)
		{
			return;
		}

		await _exportService.ExportToCsvAsync(
			HistoryEntry,
			$"{HistoryEntry.Name}_laps_{DateTime.Now:yyyyMMdd_HHmmss}");
	}

	[RelayCommand]
	public async Task ExportToXmlAsync()
	{
		if (HistoryEntry == null)
		{
			return;
		}

		await _exportService.ExportToXmlAsync(
			HistoryEntry,
			$"{HistoryEntry.Name}_export_{DateTime.Now:yyyyMMdd_HHmmss}");
	}

	[RelayCommand]
	public async Task ExportToExcelAsync()
	{
		if (HistoryEntry == null)
		{
			return;
		}

		await _exportService.ExportToExcelAsync(
			HistoryEntry,
			$"{HistoryEntry.Name}_laps_{DateTime.Now:yyyyMMdd_HHmmss}");
	}
}
