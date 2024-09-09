using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class GetPremiumView : GetPremiumViewBase
{
	public GetPremiumView()
	{
		this.InitializeComponent();
	}
}

public partial class GetPremiumViewBase : PageBase<GetProViewModel>
{
}
