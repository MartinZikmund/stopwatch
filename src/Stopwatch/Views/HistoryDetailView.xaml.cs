using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class HistoryDetailView : HistoryDetailViewBase
{
	public HistoryDetailView()
	{
		this.InitializeComponent();
	}
}

public partial class HistoryDetailViewBase : PageBase<HistoryDetailViewModel>
{
}
