namespace Stopwatch.Services.PopOut;

public interface IPopOutService
{
	void PopOut(int stopwatchId);

	void CloseAll();

	bool IsPoppedOut(int stopwatchId);

	event Action<int>? StopwatchReturned;
}
