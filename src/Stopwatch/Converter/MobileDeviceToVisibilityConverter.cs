using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Stopwatch.Converter;

/// <summary>
/// Converter that returns Collapsed visibility when HasCustomTitleBar is true and Visible when false.
/// This is used to hide the app icon when a custom title bar is used to avoid visual clutter.
/// </summary>
public class MobileDeviceToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		// The value should be the HasCustomTitleBar boolean property
		if (value is bool hasCustomTitleBar)
		{
			// Hide icon when there's a custom title bar, show it when there's not
			return hasCustomTitleBar ? Visibility.Collapsed : Visibility.Visible;
		}
		
		// If we can't get a valid boolean, default to showing the icon
		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}