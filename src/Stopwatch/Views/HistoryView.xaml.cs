using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class HistoryView : HistoryViewBase
{
	public HistoryView()
	{
		this.InitializeComponent();
	}

	private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
	{
		if (e.ClickedItem is HistoryEntryViewModel historyEntry)
		{
			await ViewModel.OpenAsync(historyEntry);
		}
	}
}

public partial class HistoryViewBase : PageBase<HistoryViewModel>
{
}
