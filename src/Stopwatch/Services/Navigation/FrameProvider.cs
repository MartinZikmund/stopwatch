namespace Stopwatch.Services.Navigation;

public class FrameProvider : IFrameProvider
{
	private readonly IWindowShellProvider _windowShellProvider;

	public FrameProvider(IWindowShellProvider windowShellProvider)
	{
		_windowShellProvider = windowShellProvider;
	}

	public Frame GetForCurrentScope() => _windowShellProvider.RootFrame;
}
