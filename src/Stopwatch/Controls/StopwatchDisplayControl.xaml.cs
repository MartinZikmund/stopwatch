using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stopwatch.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Stopwatch.Controls;

public sealed partial class StopwatchDisplayControl : UserControl
{
	public StopwatchDisplayControl()
	{
		this.InitializeComponent();
		this.Loaded += StopwatchDisplayControl_Loaded;
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

	public bool IsCompactOverlay
	{
		get => (bool)GetValue(IsCompactOverlayProperty);
		set => SetValue(IsCompactOverlayProperty, value);
	}

	public static DependencyProperty IsCompactOverlayProperty { get; } =
		DependencyProperty.Register(
			nameof(IsCompactOverlay),
			typeof(bool),
			typeof(StopwatchDisplayControl),
			new PropertyMetadata(false, OnIsCompactOverlayChanged));

	private static void OnIsCompactOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (StopwatchDisplayControl)d;
		var isCompact = (bool)e.NewValue;
		VisualStateManager.GoToState(control, isCompact ? "CompactOverlayMode" : "NormalMode", true);
	}
}
