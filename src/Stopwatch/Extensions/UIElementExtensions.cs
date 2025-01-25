using CommunityToolkit.WinUI;

namespace Stopwatch.Extensions;

public static class UIElementExtensions
{
	public static IServiceProvider? GetServiceProvider(this UIElement element)
	{
		if (element.XamlRoot?.Content?.FindDescendantOrSelf<WindowShell>() is not { } windowShell)
		{
			return null;
		}

		return windowShell.ServiceProvider;
	}
}
