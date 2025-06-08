using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Stopwatch.Converter;

/// <summary>
/// Converter that returns Collapsed visibility for narrow screen widths and Visible for wider screens.
/// This is used to hide certain UI elements on smaller screens where they should not be shown.
/// </summary>
public class MobileDeviceToVisibilityConverter : IValueConverter
{
	// Breakpoint width below which the icon will be hidden
	private const double MobileBreakpoint = 768.0;

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		// The value should be the actual width of the container
		if (value is double width && width > 0)
		{
			return width < MobileBreakpoint ? Visibility.Collapsed : Visibility.Visible;
		}
		
		// If we can't get a valid width, default to showing the icon (desktop behavior)
		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}