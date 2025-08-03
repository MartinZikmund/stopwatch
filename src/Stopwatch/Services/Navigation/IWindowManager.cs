using Stopwatch.ViewModels;

namespace Stopwatch.Services.Navigation;

public interface IWindowManager
{
	/// <summary>
	/// Opens a new window with the specified stopwatch
	/// </summary>
	/// <param name="stopwatchViewModel">The stopwatch to display in the new window</param>
	/// <returns>A task representing the asynchronous operation</returns>
	Task OpenStopwatchInNewWindowAsync(StopwatchViewModel stopwatchViewModel);
}