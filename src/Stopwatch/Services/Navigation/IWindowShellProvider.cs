using Microsoft.UI.Dispatching;
using Stopwatch.Infrastructure;

namespace Stopwatch.Services.Navigation;

public interface IWindowShellProvider
{
	Window Window { get; }

	WindowShell Shell { get; }

	DispatcherQueue DispatcherQueue { get; }

	Frame RootFrame { get; }
}
