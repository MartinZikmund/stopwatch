using Stopwatch.Models;

namespace Stopwatch.ViewModels;

public partial class HistoryEntryViewModel : ObservableObject
{
	private readonly HistoryViewModel _owner;
	private readonly HistoryEntryModel _stopwatch;

	public HistoryEntryViewModel(HistoryViewModel owner, HistoryEntryModel stopwatch)
	{
		_owner = owner;
		_stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
	}

	public HistoryEntryModel Stopwatch => _stopwatch;

	[RelayCommand]
	public async Task DeleteAsync() => await _owner.DeleteAsync(this);

	[RelayCommand]
	public async Task OpenAsync() => await _owner.OpenAsync(this);
}
