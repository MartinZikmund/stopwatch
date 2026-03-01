using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stopwatch.ViewModels;

namespace Stopwatch.Controls;

public sealed partial class LapsListControl : UserControl
{
	public LapsListControl()
	{
		this.InitializeComponent();
	}

	public LapsObservableCollection Laps
	{
		get => (LapsObservableCollection)GetValue(LapsProperty);
		set => SetValue(LapsProperty, value);
	}

	public static DependencyProperty LapsProperty { get; } =
		DependencyProperty.Register(
			nameof(Laps),
			typeof(LapsObservableCollection),
			typeof(LapsListControl),
			new PropertyMetadata(null));
}
