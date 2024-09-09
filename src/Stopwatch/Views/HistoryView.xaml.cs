using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class HistoryView : HistoryViewBase
{
	public HistoryView()
	{
		this.InitializeComponent();
	}
}

public partial class HistoryViewBase : PageBase<HistoryViewModel>
{
}
