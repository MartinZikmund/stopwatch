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
		StopwatchTabView.SizeChanged += StopwatchTabView_SizeChanged;
		this.Loaded += MainView_Loaded;
		this.Unloaded += MainView_Unloaded;
		TabViewContainer.RegisterPropertyChangedCallback(
			FrameworkElement.VisibilityProperty,
			(s, e) => UpdateTitleBarMetrics()
		);
		this.DataContextChanged += MainView_DataContextChanged;
	}

	private TeachingTip? _renameTeachingTip;
	private TeachingTip? _lapsTeachingTip;
	private TeachingTip? _exportTeachingTip;

	private void MainView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
	{
		if (ViewModel is not null)
		{
			ViewModel.TriggerTeachingTips = () => ShowTeachingTips();
		}
	}

	private void ShowTeachingTips()
	{
		// Start with the tabs teaching tip
		TabsTeachingTip.Target = TabListButton.Visibility == Visibility.Visible ? TabListButton : StopwatchTabView.FindDescendant("AddButton");
		TabsTeachingTip.IsOpen = true;
	}

	private void ShowRenameTeachingTip()
	{
		if (_renameTeachingTip == null)
		{
			_renameTeachingTip = CreateTeachingTip(
				"TeachingTip_RenameStopwatch_Title",
				"TeachingTip_RenameStopwatch_Subtitle",
				HandleRenameTeachingTipDismissal);
		}

		// Find the StopwatchNameTextBox inside StopwatchDisplayControl
		var stopwatchDisplay = this.FindDescendant<Controls.StopwatchDisplayControl>();
		if (stopwatchDisplay != null)
		{
			var target = stopwatchDisplay.FindDescendant<TextBox>("StopwatchNameTextBox");
			if (target != null)
			{
				_renameTeachingTip.Target = target;
				_renameTeachingTip.IsOpen = true;
			}
		}
	}

	private void ShowLapsTeachingTip()
	{
		if (_lapsTeachingTip == null)
		{
			_lapsTeachingTip = CreateTeachingTip(
				"TeachingTip_Laps_Title",
				"TeachingTip_Laps_Subtitle",
				HandleLapsTeachingTipDismissal);
		}

		// Find the LapButton inside StopwatchDisplayControl
		var stopwatchDisplay = this.FindDescendant<Controls.StopwatchDisplayControl>();
		if (stopwatchDisplay != null)
		{
			var target = stopwatchDisplay.FindDescendant<Button>("LapButton");
			if (target != null)
			{
				_lapsTeachingTip.Target = target;
				_lapsTeachingTip.IsOpen = true;
			}
		}
	}

	private void ShowExportTeachingTip()
	{
		if (_exportTeachingTip == null)
		{
			_exportTeachingTip = CreateTeachingTip(
				"TeachingTip_Export_Title",
				"TeachingTip_Export_Subtitle",
				HandleExportTeachingTipDismissal);
		}

		// Find the LapsExpander inside StopwatchDisplayControl
		var stopwatchDisplay = this.FindDescendant<Controls.StopwatchDisplayControl>();
		if (stopwatchDisplay != null)
		{
			var target = stopwatchDisplay.FindDescendant<Expander>("LapsExpander");
			if (target != null)
			{
				_exportTeachingTip.Target = target;
				_exportTeachingTip.IsOpen = true;
			}
		}
	}

	private TeachingTip CreateTeachingTip(string titleKey, string subtitleKey, Action<TeachingTip> dismissalHandler)
	{
		var teachingTip = new TeachingTip
		{
			Title = Localizer.Instance.GetString(titleKey),
			Subtitle = Localizer.Instance.GetString(subtitleKey)
		};

		var actionButton = new TextBlock { Text = Localizer.Instance.GetString("GotIt") };
		teachingTip.ActionButtonContent = actionButton;

		teachingTip.ActionButtonClick += (sender, args) =>
		{
			sender.IsOpen = false;
			dismissalHandler(sender);
		};

		teachingTip.CloseButtonClick += (sender, args) =>
		{
			sender.IsOpen = false;
			dismissalHandler(sender);
		};

		// Add to the visual tree
		((Grid)Content).Children.Add(teachingTip);

		return teachingTip;
	}

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

	private void TabsTeachingTip_ActionButtonClick(TeachingTip sender, object args)
	{
		sender.IsOpen = false;
		HandleTabsTeachingTipDismissal();
	}

	private void TabsTeachingTip_CloseButtonClick(TeachingTip sender, object args)
	{
		sender.IsOpen = false;
		HandleTabsTeachingTipDismissal();
	}

	private void HandleTabsTeachingTipDismissal()
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenTabsTeachingTip = true;

			// Show next teaching tip if this is first time
			if (!appPreferences.HasSeenRenameTeachingTip)
			{
				ShowRenameTeachingTip();
			}
		}
	}

	private void HandleRenameTeachingTipDismissal(TeachingTip sender)
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenRenameTeachingTip = true;

			// Show next teaching tip if this is first time
			if (!appPreferences.HasSeenLapsTeachingTip)
			{
				ShowLapsTeachingTip();
			}
		}
	}

	private void HandleLapsTeachingTipDismissal(TeachingTip sender)
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenLapsTeachingTip = true;

			// Show next teaching tip if this is first time
			if (!appPreferences.HasSeenExportTeachingTip)
			{
				ShowExportTeachingTip();
			}
		}
	}

	private void HandleExportTeachingTipDismissal(TeachingTip sender)
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenExportTeachingTip = true;
			appPreferences.FirstStart = false; // Mark that first start is complete
		}
	}
}

public partial class MainViewBase : PageBase<MainViewModel>
{
}
