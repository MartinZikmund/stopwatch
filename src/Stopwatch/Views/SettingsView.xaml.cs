using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class SettingsView : SettingsViewBase
{
	public SettingsView()
	{
		this.InitializeComponent();
	}
}

public partial class SettingsViewBase : PageBase<SettingsViewModel>
{
}
