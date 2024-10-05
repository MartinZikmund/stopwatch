using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class GetProView : GetProViewBase
{
	public GetProView()
	{
		this.InitializeComponent();
	}
}

public partial class GetProViewBase : PageBase<GetProViewModel>
{
}
