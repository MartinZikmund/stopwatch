using Stopwatch.Services.Navigation;

namespace Stopwatch.Services.Theming;

public class ThemeManager : IThemeManager
{
	private readonly IWindowShellProvider _windowShellProvider;

	public ThemeManager(IWindowShellProvider windowShellProvider)
	{
		_windowShellProvider = windowShellProvider;
	}

	public void SetTheme(ElementTheme theme) => GetRootElement().RequestedTheme = theme;

	public ElementTheme CurrentTheme => GetRootElement().RequestedTheme;

	private FrameworkElement GetRootElement()
	{
		var rootElement = _windowShellProvider.Shell;
		if (rootElement == null)
		{
			throw new InvalidOperationException("Root element of the window is not a FrameworkElement");
		}

		return rootElement;
	}
}
