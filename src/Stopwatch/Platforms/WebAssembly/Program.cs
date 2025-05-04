using Stopwatch;
using Uno.UI.Hosting;

App.InitializeLogging();

var host = UnoPlatformHostBuilder.Create()
	.UseWebAssembly()
	.Build();
await host.RunAsync();
