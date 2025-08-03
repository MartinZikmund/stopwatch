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

				// Navigate to the main view with the specific stopwatch ID
				var navigationService = newWindowShell.ServiceProvider.GetRequiredService<INavigationService>();
				navigationService.Navigate<MainViewModel>(stopwatchViewModel.Id);

				// Activate the new window
				newWindow.Activate();
			}
			catch (Exception ex)
			{
				// Log error (in a real app, you'd use proper logging)
				System.Diagnostics.Debug.WriteLine($"Error opening new window: {ex.Message}");
			}
		});
	}
}