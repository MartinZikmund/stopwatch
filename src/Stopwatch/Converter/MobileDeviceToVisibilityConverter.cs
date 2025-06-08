using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.System.Profile;

namespace Stopwatch.Converter;

/// <summary>
/// Converter that returns Collapsed visibility for mobile devices and Visible for other devices.
/// This is used to hide certain UI elements on mobile platforms where they should not be shown.
/// </summary>
public class MobileDeviceToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var deviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
		
		// Hide on mobile devices (Windows.Mobile for UWP, or Android/iOS for Uno)
		bool isMobile = deviceFamily.Contains("Mobile") || 
			            deviceFamily.Contains("Android") || 
			            deviceFamily.Contains("iOS");
		
		return isMobile ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}