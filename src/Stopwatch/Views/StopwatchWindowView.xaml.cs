using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class StopwatchWindowView : StopwatchWindowViewBase
{
	public StopwatchWindowView()
	{
		this.InitializeComponent();
	}
}

public partial class StopwatchWindowViewBase : PageBase<StopwatchWindowViewModel>
{
}
