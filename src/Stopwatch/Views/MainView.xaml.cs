using Stopwatch.ViewModels;

namespace Stopwatch.Views;

public sealed partial class MainView : MainViewBase
{
    public MainView()
    {
        this.InitializeComponent();
    }
}

public partial class MainViewBase : PageBase<MainViewModel>
{
}
