using Microsoft.UI.Xaml;
using Stopwatch.ViewModels;

namespace Stopwatch.Services.Navigation;

public class WindowManager : IWindowManager
{
	private readonly IServiceProvider _serviceProvider;

	public WindowManager(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task OpenStopwatchInNewWindowAsync(StopwatchViewModel stopwatchViewModel)
	{
		// Check if multiple windows are supported on this platform
		if (!IsMultiWindowSupported())
		{
			return;
		}

		// Get the current window provider to access the dispatcher queue
		var mainWindowProvider = _serviceProvider.GetRequiredService<IWindowShellProvider>();
		
		// Ensure we're on the UI thread
		await mainWindowProvider.DispatcherQueue.EnqueueAsync(() =>
		{
			try
			{
				// Create a new window
				var newWindow = new Window();
				newWindow.Title = $"Fluent Stopwatch - {stopwatchViewModel.Name}";

				// Create a new window shell with a new service scope
				var newWindowShell = new WindowShell(_serviceProvider, newWindow);
				newWindow.Content = newWindowShell;

				// Navigate to the stopwatch window view with the specific stopwatch ID
				var navigationService = newWindowShell.ServiceProvider.GetRequiredService<INavigationService>();
				navigationService.Navigate<StopwatchWindowViewModel>(stopwatchViewModel.Id);

				// Activate the new window
				newWindow.Activate();
			}
			catch (Exception ex)
			{
				// Log error (in a real app, you'd use proper logging)
				System.Diagnostics.Debug.WriteLine($"Error opening new window: {ex.Message}");
				
				// TODO: Show user-friendly error message via dialog service
			}
		});
	}

	private static bool IsMultiWindowSupported()
	{
#if HAS_UNO && (__ANDROID__ || __IOS__ || __WASM__)
		// Multiple windows are not supported on mobile platforms and WASM
		return false;
#else
		// On Windows and other desktop platforms, multiple windows are supported
		return true;
#endif
	}
}