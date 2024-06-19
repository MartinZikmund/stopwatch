using Microsoft.UI.Dispatching;
using Stopwatch.Services.Navigation;

namespace Stopwatch.Services.Timer;

public class TimerFactory : ITimerFactory
{
	private readonly IWindowShellProvider _provider;

	public TimerFactory(IWindowShellProvider provider)
	{
		_provider = provider;
	}

	public DispatcherQueueTimer Create() => _provider.DispatcherQueue.CreateTimer();
}
