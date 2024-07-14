using Microsoft.UI.Dispatching;
using Stopwatch.Extensions;
using Stopwatch.Services.Settings;
using Stopwatch.ViewModels;
using Windows.Foundation.Metadata;

namespace Stopwatch.Views;

public sealed partial class MainView : MainViewBase
{
	private DispatcherQueueTimer _fadeOutTimer;

	public MainView()
	{
		this.InitializeComponent();

		_fadeOutTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
		_fadeOutTimer.Interval = TimeSpan.FromSeconds(3);
		_fadeOutTimer.Tick += (sender, e) =>
		{
			_fadeOutTimer.Stop();
			ControlButtonsPanel.Opacity = 0;
		};
		_fadeOutTimer.Start();

		if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "OpacityTransition"))
		{
			ControlButtonsPanel.OpacityTransition = new ScalarTransition() { Duration = TimeSpan.FromMilliseconds(200) };
		}

		this.Loaded += MainView_Loaded;
	}

	private void MainView_Loaded(object sender, RoutedEventArgs e)
	{
		StartAutoHide();
	}

	private void RootGridPointerEvent(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		_fadeOutTimer.Stop();
		ControlButtonsPanel.Opacity = 1;
		StartAutoHide();
	}

	private void StartAutoHide()
	{
		var serviceProvider = this.GetServiceProvider();
		if (serviceProvider is null)
		{
			return;
		}

		var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
		if (appPreferences.AutoHideButtons)
		{
			_fadeOutTimer.Start();
		}
	}
}

public partial class MainViewBase : PageBase<MainViewModel>
{
}
