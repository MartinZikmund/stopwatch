using System.Collections.ObjectModel;
using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

public partial class HistoryViewModel : PageViewModel
{
	private readonly IHistoryService _historyService;
	private readonly IConfirmationDialogService _confirmationDialogService;
	[ObservableProperty]
	public partial ObservableCollection<HistoryEntryViewModel> HistoryEntries { get; set; } = new();

	public HistoryViewModel(INavigationService navigationService, IHistoryService historyService, IConfirmationDialogService confirmationDialogService) : base(navigationService)
	{
		_historyService = historyService;
		_confirmationDialogService = confirmationDialogService;
	}

	public override void ViewNavigatedTo(object? parameter)
	{
		var items = _historyService.GetAll();
		HistoryEntries = new(items.Select(i => new HistoryEntryViewModel(this, i)));
	}

	internal async Task DeleteAsync(HistoryEntryViewModel historyEntryViewModel)
	{
		var dialogName = "DeleteHistoryEntryDialog";
		var result = await _confirmationDialogService.ShowAsync(
			Localizer.Instance.GetString($"{dialogName}Title"),
			Localizer.Instance.GetString($"{dialogName}Text"));

		if (result == ConfirmationResult.Confirmed)
		{
			_historyService.Delete(historyEntryViewModel.Stopwatch);
			HistoryEntries.Remove(historyEntryViewModel);
		}
	}

	internal async Task OpenAsync(HistoryEntryViewModel historyEntryViewModel)
	{
		NavigationService.Navigate<HistoryDetailViewModel>(historyEntryViewModel.Stopwatch.Id);
		await Task.CompletedTask;
	}

	[RelayCommand]
	internal async Task ClearAllAsync()
	{
		var dialogName = "ClearHistoryDialog";
		var result = await _confirmationDialogService.ShowAsync(
			Localizer.Instance.GetString($"{dialogName}Title"),
			Localizer.Instance.GetString($"{dialogName}Text"));

		if (result == ConfirmationResult.Confirmed)
		{
			_historyService.Clear();
			HistoryEntries.Clear();
		}
	}
}
