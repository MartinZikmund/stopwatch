using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stopwatch.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Stopwatch.Controls;

public sealed partial class StopwatchDisplayControl : UserControl
{
	private bool _isLandscape;

	public StopwatchDisplayControl()
	{
		this.InitializeComponent();
		this.Loaded += StopwatchDisplayControl_Loaded;
		this.SizeChanged += StopwatchDisplayControl_SizeChanged;
	}

	private void StopwatchDisplayControl_Loaded(object sender, RoutedEventArgs e)
	{
		StartStopButton.Focus(FocusState.Programmatic);
	}

	private void StopwatchDisplayControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		var landscape = e.NewSize.Width > e.NewSize.Height && e.NewSize.Height < 450;
		if (landscape != _isLandscape)
		{
			_isLandscape = landscape;
			ApplyLayout(landscape);
		}
	}

	private void ApplyLayout(bool landscape)
	{
		if (landscape)
		{
			// Full-screen timer with laps as flyout button at bottom
			LapsRowDefinition.Height = new GridLength(0, GridUnitType.Auto);
			LapsColumnDefinition.Width = new GridLength(0);
			RootGrid.RowSpacing = 4;
			RootGrid.ColumnSpacing = 0;
			LapsExpander.Visibility = Visibility.Collapsed;
			LapsFlyoutButton.Visibility = Visibility.Visible;
			StopwatchNamePanel.Visibility = Visibility.Collapsed;
		}
		else
		{
			// Vertical stack: timer top, laps bottom
			LapsRowDefinition.Height = new GridLength(1, GridUnitType.Star);
			LapsColumnDefinition.Width = new GridLength(0);
			RootGrid.RowSpacing = 16;
			RootGrid.ColumnSpacing = 0;
			LapsExpander.Visibility = Visibility.Visible;
			LapsFlyoutButton.Visibility = Visibility.Collapsed;
			Grid.SetRow(LapsExpander, 1);
			Grid.SetColumn(LapsExpander, 0);
			LapsExpander.ExpandDirection = Microsoft.UI.Xaml.Controls.ExpandDirection.Up;
			LapsExpander.VerticalAlignment = VerticalAlignment.Bottom;
			StopwatchNamePanel.Visibility = Visibility.Visible;
		}
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
