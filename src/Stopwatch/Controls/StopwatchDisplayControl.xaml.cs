using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stopwatch.Services.Settings;
using Stopwatch.ViewModels;
using Stopwatch.Extensions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Stopwatch.Controls;
public sealed partial class StopwatchDisplayControl : UserControl
{
	public StopwatchDisplayControl()
	{
		this.InitializeComponent();
		this.Loaded += StopwatchDisplayControl_Loaded;
		this.DataContextChanged += StopwatchDisplayControl_DataContextChanged;
	}

	private void StopwatchDisplayControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
	{
		SetupTeachingTipActions();
	}

	private void SetupTeachingTipActions()
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var mainViewModel = FindMainViewModel();
			if (mainViewModel is not null)
			{
				mainViewModel.ShowRenameTeachingTip = () => RenameTeachingTip.IsOpen = true;
				mainViewModel.ShowLapsTeachingTip = () => LapsTeachingTip.IsOpen = true;
				mainViewModel.ShowExportTeachingTip = () => ExportTeachingTip.IsOpen = true;
			}
		}
	}

	private MainViewModel? FindMainViewModel()
	{
		// Find the MainView in the visual tree to get its ViewModel
		var current = this.Parent;
		while (current != null)
		{
			if (current is MainView mainView)
			{
				return mainView.ViewModel;
			}
			current = (current as FrameworkElement)?.Parent;
		}
		return null;
	}

	private void StopwatchDisplayControl_Loaded(object sender, RoutedEventArgs e)
	{
		StartStopButton.Focus(FocusState.Programmatic);
	}

	public StopwatchViewModel Stopwatch
	{
		get => (StopwatchViewModel)GetValue(StopwatchProperty);
		set => SetValue(StopwatchProperty, value);
	}

	public static DependencyProperty StopwatchProperty { get; } =
		DependencyProperty.Register(
			nameof(Stopwatch),
			typeof(StopwatchViewModel),
			typeof(StopwatchDisplayControl),
			new PropertyMetadata(null));

	public MainViewModel MainViewModel
	{
		get => (MainViewModel)GetValue(MainViewModelProperty);
		set => SetValue(MainViewModelProperty, value);
	}

	public static DependencyProperty MainViewModelProperty { get; } =
		DependencyProperty.Register(
			nameof(MainViewModel),
			typeof(MainViewModel),
			typeof(StopwatchDisplayControl),
			new PropertyMetadata(null));

	private void RenameTeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenRenameTeachingTip = true;

			// Show next teaching tip if this is first time
			if (!appPreferences.HasSeenLapsTeachingTip)
			{
				var mainViewModel = FindMainViewModel();
				mainViewModel?.ShowLapsTeachingTip?.Invoke();
			}
		}
	}

	private void LapsTeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenLapsTeachingTip = true;

			// Show next teaching tip if this is first time
			if (!appPreferences.HasSeenExportTeachingTip)
			{
				var mainViewModel = FindMainViewModel();
				mainViewModel?.ShowExportTeachingTip?.Invoke();
			}
		}
	}

	private void ExportTeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
	{
		if (this.GetServiceProvider() is { } serviceProvider)
		{
			var appPreferences = serviceProvider.GetRequiredService<IAppPreferences>();
			appPreferences.HasSeenExportTeachingTip = true;
			appPreferences.FirstStart = false; // Mark that first start is complete
		}
	}
}
