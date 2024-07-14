using Stopwatch.Infrastructure;

namespace Stopwatch.Services.Navigation;

public interface IWindowShellProvider : IWindowShell
{
	Window Window { get; }

	FrameworkElement Shell { get; }
}
