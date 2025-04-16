using Uno.UI.Runtime.Skia.WebAssembly.Browser;

namespace Stopwatch;

public class Program
{
	public static async Task Main(string[] args)
	{
		App.InitializeLogging();

		var host = new WebAssemblyBrowserHost(() => new App());
		await host.Run();
	}
}
