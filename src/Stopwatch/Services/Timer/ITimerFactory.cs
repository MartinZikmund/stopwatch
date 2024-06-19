using Microsoft.UI.Dispatching;

namespace Stopwatch.Services.Timer;

public interface ITimerFactory
{
	DispatcherQueueTimer Create();
}
