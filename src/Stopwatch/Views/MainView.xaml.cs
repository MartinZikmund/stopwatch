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
	private AppWindow _appWindow;
	private WindowShell _shell;

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

		this.Loaded += MainView_Loaded;
		this.Unloaded += MainView_Unloaded;
		StopwatchTabView.SizeChanged += OnTabViewSizeChanged;
	}

	private void OnTabViewSizeChanged(object sender, SizeChangedEventArgs e) => UpdateTitleBarMetrics();

	private void UpdateTitleBarMetrics()
	{
		if (_appWindow is null)
		{
			return;
		}

		StopwatchTabView.Visibility = _appWindow.Presenter is OverlappedPresenter ? Visibility.Visible : Visibility.Collapsed;

		StopwatchTabView.Margin = new Thickness(0, 0, _appWindow.TitleBar.RightInset + 16, 0);

		DraggableArea.Margin = new Thickness(StopwatchTabView.ActualWidth, 0, 0, 0);
	}

	private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args) => UpdateTitleBarMetrics();

	private void MainView_Loaded(object sender, RoutedEventArgs e)
	{
		StartAutoHide();

		_appWindow = this.GetServiceProvider().GetRequiredService<IWindowShellProvider>().Window.AppWindow;
		_appWindow.Changed += OnAppWindowChanged;

		_shell = this.GetServiceProvider().GetRequiredService<IWindowShellProvider>().Shell;
		_shell.SetTitleBar(DraggableArea);

		UpdateTitleBarMetrics();
	}
	
	private void MainView_Unloaded(object sender, RoutedEventArgs e)
	{
		_appWindow.Changed -= OnAppWindowChanged;
		_appWindow = null;

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
