using Stopwatch.ViewModels;

namespace Stopwatch.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OnboardingView : OnboardingViewBase
{
    public OnboardingView()
    {
        this.InitializeComponent();
    }
}

public partial class OnboardingViewBase : PageBase<OnboardingViewModel>
{ 
}
