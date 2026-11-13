using UIKit;
using Uno.UI.Hosting;

namespace Stopwatch.iOS;

public class EntryPoint
{
	// This is the main entry point of the application.
	public static void Main(string[] args)
	{
		App.InitializeLogging();

		// Uno 7 keeps LocalSettings in its own suite and does not bring over what earlier versions wrote.
		try
		{
			Uno.Storage.ApplicationDataMigrator.MigrateSettings();
		}
		catch (IOException)
		{
			// The old values stay where they are, so the next launch retries.
		}

		var host = UnoPlatformHostBuilder.Create()
			.App(() => new App())
			.UseAppleUIKit()
			.Build();

		host.Run();
	}
}
