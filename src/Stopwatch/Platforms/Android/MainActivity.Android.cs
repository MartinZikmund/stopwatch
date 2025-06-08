using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Stopwatch.Services.Navigation;

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
		// Try to handle back navigation through the app's navigation system
		if (App.Host?.Services?.GetService<INavigationService>() is { } navigationService && navigationService.CanGoBack)
		{
			navigationService.GoBack();
		}
		// If there's nowhere to go back in the app, don't close the app - just ignore the back button
		// This prevents the hardware back button from closing the app unexpectedly
	}
}
