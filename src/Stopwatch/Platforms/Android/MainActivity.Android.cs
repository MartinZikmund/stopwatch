using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;

namespace Stopwatch.Droid;
[Activity(
    MainLauncher = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
	protected override void OnCreate(Bundle? bundle)
	{
		global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

		base.OnCreate(bundle);
	}

	public override void OnBackPressed()
	{
		// Use Uno Platform's NavigationManagerPreview API for better integration
		if (Microsoft.UI.Xaml.Navigation.NavigationManagerPreview.GetForCurrentView() is { } navigationManager)
		{
			if (navigationManager.CanGoBack)
			{
				navigationManager.GoBack();
				return;
			}
		}
		
		// If there's nowhere to go back in the app, don't close the app - just ignore the back button
		// This prevents the hardware back button from closing the app unexpectedly
	}
}
