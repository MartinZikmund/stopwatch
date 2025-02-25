using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Stopwatch.Extensions;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.ViewModels;
using Windows.Foundation.Metadata;

namespace Stopwatch.Views;

public sealed partial class MainView : MainViewBase
{
	private DispatcherQueueTimer _fadeOutTimer;
	private WindowShell _shell;
	private Window _window;

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

		if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "OpacityTransition"))
		{
			ControlButtonsPanel.OpacityTransition = new ScalarTransition() { Duration = TimeSpan.FromMilliseconds(200) };
		}
		StopwatchTabView.SizeChanged += StopwatchTabView_SizeChanged;
		this.Loaded += MainView_Loaded;
		this.Unloaded += MainView_Unloaded;
	}

	private void StopwatchTabView_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (_window is null)
		{
			return;
		}

		DraggableArea.Width = _window.Bounds.Width - TabViewContainer.Padding.Left - StopwatchTabView.ActualWidth + FooterArea.ActualWidth;
	}

	private void UpdateTitleBarMetrics()
	{
		if (_window is null)
		{
			return;
		}

		var rightInset = _window.AppWindow.TitleBar.RightInset / XamlRoot.RasterizationScale;

		TabViewContainer.Width = _window.Bounds.Width - Math.Max(rightInset, 0);
	}

	private void MainView_Loaded(object sender, RoutedEventArgs e)
	{
		StartAutoHide();

		if (this.GetServiceProvider() is not { } serviceProvider)
		{
			throw new InvalidOperationException("Service provider is not available");
		}

		_window = serviceProvider.GetRequiredService<IWindowShellProvider>().Window;
		_window.SizeChanged += OnWindowSizeChanged;

		_shell = serviceProvider.GetRequiredService<IWindowShellProvider>().Shell;
		_shell.SetTitleBar(DraggableArea);

		UpdateTitleBarMetrics();
	}

	private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs args) => UpdateTitleBarMetrics();

	private void MainView_Unloaded(object sender, RoutedEventArgs e)
	{
		_window.SizeChanged -= OnWindowSizeChanged;

		_shell.SetTitleBar(null);
		_shell = null;
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

	private async void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
	{
		if (args.Item is StopwatchViewModel stopwatchViewModel && ViewModel is not null)
		{
			await ViewModel.CloseStopwatchAsync(stopwatchViewModel);
		}
	}
}

public partial class MainViewBase : PageBase<MainViewModel>
{
}
