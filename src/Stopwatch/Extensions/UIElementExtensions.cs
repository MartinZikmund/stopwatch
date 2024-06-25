namespace Stopwatch.Extensions;

public static class UIElementExtensions
{
	public static IServiceProvider? GetServiceProvider(this UIElement element)
	{
		if (element.XamlRoot?.Content is WindowShell windowShell)
		{
			return windowShell.ServiceProvider;
		}

		return null;
	}
}
