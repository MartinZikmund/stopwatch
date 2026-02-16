using Stopwatch.Services.Navigation;
using Stopwatch.ViewModels;

namespace Stopwatch.Services.PopOut;

public class PopOutService : IPopOutService
{
	private readonly IServiceProvider _hostServices;
	private readonly Dictionary<int, PopOutWindowInfo> _windows = new();

	public PopOutService(IServiceProvider hostServices)
	{
		_hostServices = hostServices;
	}

	public event Action<int>? StopwatchReturned;

	public void PopOut(int stopwatchId)
	{
		if (_windows.ContainsKey(stopwatchId))
		{
			return;
		}

		var window = new Window();
		var windowShell = new WindowShell(_hostServices, window);
		window.Content = windowShell;

		windowShell.ServiceProvider.GetRequiredService<INavigationService>().Navigate<StopwatchWindowViewModel>(stopwatchId);

		_windows[stopwatchId] = new PopOutWindowInfo(stopwatchId, window, windowShell);

		window.Closed += (sender, args) => OnSecondaryWindowClosed(stopwatchId);

		window.Activate();
	}

	public void CloseAll()
	{
		foreach (var info in _windows.Values.ToArray())
		{
			info.Window.Close();
		}

		_windows.Clear();
	}

	public bool IsPoppedOut(int stopwatchId) => _windows.ContainsKey(stopwatchId);

	private void OnSecondaryWindowClosed(int stopwatchId)
	{
		_windows.Remove(stopwatchId);
		StopwatchReturned?.Invoke(stopwatchId);
	}

	private sealed record PopOutWindowInfo(int StopwatchId, Window Window, WindowShell WindowShell);
}
