using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Stopwatch.Droid;
[Activity(
    MainLauncher = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
	internal static Activity? Current { get; private set; }

	protected override void OnCreate(Bundle? bundle)
	{
		global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);
		Current = this;
		base.OnCreate(bundle);
	}
}
