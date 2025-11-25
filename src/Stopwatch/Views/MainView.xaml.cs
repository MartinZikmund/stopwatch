using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stopwatch.Extensions;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Localization;
using Stopwatch.ViewModels;
using Windows.Foundation.Metadata;
using CommunityToolkit.WinUI;
using Uno.Disposables;

namespace Stopwatch.Views;

public sealed partial class MainView : MainViewBase
{
	private readonly SerialDisposable _teachingTipTriggerDisposable = new();
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
		StopwatchTabView.SizeChanged += StopwatchTabView_SizeChanged;
		this.Loaded += MainView_Loaded;
		this.Unloaded += MainView_Unloaded;
		TabViewContainer.RegisterPropertyChangedCallback(
			FrameworkElement.VisibilityProperty,
			(s, e) => UpdateTitleBarMetrics()
		);
		this.DataContextChanged += MainView_DataContextChanged;
	}

	private void MainView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
	{
		_teachingTipTriggerDisposable.Disposable = null;
		if (ViewModel is not null)
		{
			ViewModel.TriggerTeachingTips = ShowTeachingTips;
			_teachingTipTriggerDisposable.Disposable = Disposable.Create(() =>
			{
				ViewModel.TriggerTeachingTips = null;
			});
		}
	}

	private async void ShowTeachingTips()
	{           
		// Delay to make the initial teaching tip more noticeable
		await Task.Delay(200);
		ShowTabsTeachingTip();
		var serviceProvider = this.GetServiceProvider();
		if (serviceProvider is null)
		{
			return;
		}

		var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
		appPreferences.HasSeenOnboardingTips = true;
	}

	private void ShowTabsTeachingTip() =>
		ShowTeachingTip(
			"TeachingTip_AddTabs_Title",
			"TeachingTip_AddTabs_Subtitle",
			TabListButton.Visibility == Visibility.Visible ? TabListButton : StopwatchTabView.FindDescendant("AddButton"),
			ShowRenameTeachingTip);

	private void ShowRenameTeachingTip() =>
		ShowTeachingTip(
			"TeachingTip_RenameStopwatch_Title",
			"TeachingTip_RenameStopwatch_Subtitle",
			this.FindDescendant<Controls.StopwatchDisplayControl>()?.FindDescendant("StopwatchNameTextBox"),
			ShowLapsTeachingTip);

	private void ShowLapsTeachingTip() =>
		ShowTeachingTip(
			"TeachingTip_Laps_Title",
			"TeachingTip_Laps_Subtitle",
			this.FindDescendant<Controls.StopwatchDisplayControl>()?.FindDescendant("LapButton"),
			ShowExportTeachingTip);

	private void ShowExportTeachingTip() =>
		ShowTeachingTip(
			"TeachingTip_Export_Title",
			"TeachingTip_Export_Subtitle",
			this.FindDescendant<Controls.StopwatchDisplayControl>()?.FindDescendant("LapsExpander"),
			null);

	private void ShowTeachingTip(string titleKey, string subtitleKey, FrameworkElement? target, Action? dismissalAction)
	{
		var teachingTip = Resources["OnboardingTeachingTip"] as TeachingTip;
		if (teachingTip is null)
		{
			throw new InvalidOperationException("TeachingTip resource not found.");
		}

		teachingTip.Title = Localizer.Instance.GetString(titleKey);
		teachingTip.Subtitle = Localizer.Instance.GetString(subtitleKey);

		void OnTeachingTipClosed(TeachingTip sender, TeachingTipClosedEventArgs args)
		{
			sender.Closed -= OnTeachingTipClosed;
			dismissalAction?.Invoke();
		}

		teachingTip.Closed += OnTeachingTipClosed;

		if (target is not null)
		{
			teachingTip.Target = target;
		}
		else
		{
			teachingTip.Target = this;
		}

		teachingTip.IsOpen = true;
	}

	private void OnTeachingTipClick(TeachingTip sender, object args) => sender.IsOpen = false;

	private void StopwatchTabView_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (XamlRoot is null)
		{
			return;
		}

		DraggableArea.Width = XamlRoot.Size.Width - TabViewContainer.Padding.Left - StopwatchTabView.ActualWidth + FooterArea.ActualWidth;
	}

	private void UpdateTitleBarMetrics()
	{
		if (XamlRoot is null || _appWindow is null || TabViewContainer.Visibility == Visibility.Collapsed)
		{
#if HAS_UNO
			TitleBarArea.Visibility = Visibility.Collapsed;
#endif
			return;
		}

#if HAS_UNO
		TitleBarArea.Visibility = Visibility.Visible;
		TabViewContainer.Width = XamlRoot.Size.Width;
		DraggableArea.Visibility = Visibility.Collapsed;
#else
		var rightInset = _appWindow.TitleBar.RightInset / XamlRoot.RasterizationScale;

		TabViewContainer.Width = XamlRoot.Size.Width - Math.Max(rightInset, 0);
#endif
	}

	private void MainView_Loaded(object sender, RoutedEventArgs e)
	{
		StartAutoHide();

		if (this.GetServiceProvider() is not { } serviceProvider)
		{
			throw new InvalidOperationException("Service provider is not available");
		}

		XamlRoot.Changed += XamlRoot_Changed;

		_appWindow = serviceProvider.GetRequiredService<IWindowShellProvider>().Window.AppWindow;

		_shell = serviceProvider.GetRequiredService<IWindowShellProvider>().Shell;
		_shell.SetTitleBar(DraggableArea);

		UpdateTitleBarMetrics();
	}

	private void XamlRoot_Changed(XamlRoot sender, XamlRootChangedEventArgs args) => UpdateTitleBarMetrics();

	private void MainView_Unloaded(object sender, RoutedEventArgs e)
	{
		XamlRoot.Changed -= XamlRoot_Changed;
		_teachingTipTriggerDisposable.Disposable = null;

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

	private async void CloseTabClick(object sender, RoutedEventArgs e)
	{
		var button = (Button)sender;
		if (button.CommandParameter is StopwatchViewModel stopwatchViewModel && ViewModel is not null)
		{
			await ViewModel.CloseStopwatchAsync(stopwatchViewModel);
		}
	}
}

public partial class MainViewBase : PageBase<MainViewModel>
{
}
