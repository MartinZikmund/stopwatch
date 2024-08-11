using Microsoft.UI;
using Stopwatch.Services.Navigation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Stopwatch.Services.Theming;

public class ThemeManager : IThemeManager
{
	private readonly IWindowShellProvider _windowShellProvider;
	private readonly UISettings _uiSettings = new();

	public ThemeManager(IWindowShellProvider windowShellProvider)
	{
		_windowShellProvider = windowShellProvider;
		_uiSettings.ColorValuesChanged += OnColorValuesChanged;
	}

	public void SetTheme(ElementTheme theme)
	{
		GetRootElement().RequestedTheme = theme;
		UpdateTitleBarTheming();
	}

	public ElementTheme CurrentTheme => GetRootElement().RequestedTheme;

	public ApplicationTheme ActualTheme => CurrentTheme switch
	{
		ElementTheme.Default => Application.Current.RequestedTheme,
		ElementTheme.Light => ApplicationTheme.Light,
		ElementTheme.Dark => ApplicationTheme.Dark,
		_ => throw new ArgumentOutOfRangeException()
	};

	private FrameworkElement GetRootElement()
	{
		var rootElement = _windowShellProvider.Shell;
		if (rootElement == null)
		{
			throw new InvalidOperationException("Root element of the window is not a FrameworkElement");
		}

		return rootElement;
	}

	private void UpdateTitleBarTheming()
	{
#pragma warning disable CS8618
#pragma warning disable Uno0001
		var titleBar = _windowShellProvider.Window.AppWindow.TitleBar;
		titleBar.BackgroundColor = Colors.Transparent;
		titleBar.ButtonBackgroundColor = Colors.Transparent;
		titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
		if (ActualTheme == ApplicationTheme.Dark)
		{
			titleBar.ButtonForegroundColor = Colors.White;
			titleBar.ButtonInactiveForegroundColor = Colors.Gray;
			titleBar.ButtonHoverBackgroundColor = Color.FromArgb(100, 100, 100, 100);
			titleBar.ButtonHoverForegroundColor = Colors.White;
			titleBar.ButtonPressedBackgroundColor = Color.FromArgb(200, 100, 100, 100);
			titleBar.ButtonPressedForegroundColor = Colors.White;
		}
		else
		{
			titleBar.ButtonForegroundColor = Colors.Black;
			titleBar.ButtonInactiveForegroundColor = Colors.Gray;
			titleBar.ButtonHoverBackgroundColor = Color.FromArgb(100, 200, 200, 200);
			titleBar.ButtonHoverForegroundColor = Colors.Black;
			titleBar.ButtonPressedBackgroundColor = Color.FromArgb(200, 200, 200, 200);
			titleBar.ButtonPressedForegroundColor = Colors.Black;
		}
#pragma warning restore Uno0001
#pragma warning restore CS8618
	}

	private void OnColorValuesChanged(UISettings sender, object args)
	{
		_windowShellProvider.DispatcherQueue.TryEnqueue(() =>
		{
			UpdateTitleBarTheming();
		});
	}
}
