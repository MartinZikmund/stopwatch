using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI;
using MZikmund.Toolkit.WinUI.Infrastructure;
using MZikmund.Toolkit.WinUI.Services;
using Stopwatch.Core.Services;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Store;
using Stopwatch.Services.Theming;
using Stopwatch.Services.Timer;
using Stopwatch.ViewModels;
using Stopwatch.Views;


#if __IOS__ || __ANDROID__
using Stopwatch.Services.Data.Files;
#else
using Stopwatch.Services.Data.LiteDb;
#endif

namespace Stopwatch;
public partial class App : Application
{
	/// <summary>
	/// Initializes the singleton application object. This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();
	}

	protected Window? MainWindow { get; private set; }

	internal static IHost? Host { get; private set; }

#if !HAS_UNO
	private Microsoft.Windows.AppLifecycle.AppInstance _currentInstance;
#endif

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
#if !HAS_UNO
		// If this is the first instance launched, then register it as the "main" instance.
		// If this isn't the first instance launched, then "main" will already be registered,
		// so retrieve it.
		_currentInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("fluentstopwatch");
		_currentInstance.Activated += OnInstanceActivated;

		// If the instance that's executing the OnLaunched handler right now
		// isn't the "main" instance.
		if (!_currentInstance.IsCurrent)
		{
			// Redirect the activation (and args) to the "main" instance, and exit.
			var activationArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
			await _currentInstance.RedirectActivationToAsync(activationArgs);
			this.Exit();
			return;
		}
#endif

		var builder = this.CreateBuilder(args)
			.Configure(host => host
#if DEBUG
				// Switch to Development environment when running in DEBUG
				.UseEnvironment(Environments.Development)
#endif
				.UseLocalization()
				.UseDefaultServiceProvider((context, options) =>
				{
					options.ValidateScopes = true;
					options.ValidateOnBuild = true;
				})
				.ConfigureServices((context, services) => ConfigureServices(services))
			);
		MainWindow = builder.Window;

#if DEBUG
		MainWindow.UseStudio();
#endif

		Host = builder.Build();
		Ioc.Default.ConfigureServices(Host.Services);
		await (Host.Services.GetRequiredService<IDataSource>()).InitializeAsync();
		var preferences = Host.Services.GetRequiredService<IAppPreferences>();
		preferences.FirstStart = false;
		preferences.LaunchCount++;

		// Do not repeat app initialization when the Window already has content,
		// just ensure that the window is active
		if (MainWindow.Content is not WindowShell windowShell)
		{
			// Create a Frame to act as the navigation context and navigate to the first page
			windowShell = new WindowShell(Host.Services, MainWindow);

			// Place the frame in the current Window
			MainWindow.Content = windowShell;
		}

		if (windowShell.RootFrame.Content is null)
		{
			// When the navigation stack isn't restored navigate to the first page,
			// configuring the new page by passing required information as a navigation
			// parameter
			windowShell.ServiceProvider.GetRequiredService<INavigationService>().Navigate<MainViewModel>(args.Arguments);
		}

		// Ensure the current window is active
		MainWindow.Activate();
	}

#if !HAS_UNO
	private async void OnInstanceActivated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
	{
		await MainWindow.DispatcherQueue.EnqueueAsync(() =>
		{
			MainWindow.Activate();
		});
	}
#endif

	private void ConfigureServices(IServiceCollection services)
	{
#if __IOS__ || __ANDROID__
		services.AddSingleton<IDataSource, FileDataSource>();
#else
		services.AddSingleton<IDataSource, LiteDbDataSource>();
#endif
		services.AddSingleton<IHistoryService, HistoryService>();
		services.AddSingleton<IDisplayRequestManager, DisplayRequestManager>();
		services.AddSingleton<IPreferences, Preferences>();
		services.AddSingleton<IAppPreferences, AppPreferences>();

		services.AddScoped<WindowShellViewModel>();
		services.AddScoped<SettingsViewModel>();
		services.AddScoped<MainViewModel>();
		services.AddScoped<OnboardingViewModel>();
		services.AddScoped<HistoryViewModel>();
		services.AddScoped<GetProViewModel>();

		services.AddScoped<IDialogCoordinator, DialogCoordinator>();
		services.AddScoped<IConfirmationDialogService, ConfirmationDialogService>();
		services.AddScoped<IFrameProvider, FrameProvider>();
		services.AddScoped<IImagePickerService, ImagePickerService>();
		services.AddScoped<INavigationService, NavigationService>();
		services.AddScoped<IDialogService, DialogService>();
		services.AddScoped<IWindowShellProvider, WindowShellProvider>();
		services.AddScoped<ITimerFactory, TimerFactory>();
		services.AddScoped<IThemeManager, ThemeManager>();
#if __IOS__
		services.AddScoped<IStoreService, StoreService>();
#elif HAS_UNO
		services.AddScoped<IStoreService, ProStoreService>();
#elif DEBUG
		services.AddScoped<IStoreService, FakeStoreService>();
#else
		services.AddScoped<IStoreService, StoreService>();
#endif

		services.AddScoped<IXamlRootProvider, XamlRootProvider>();
	}

	/// <summary>
	/// Configures global Uno Platform logging
	/// </summary>
	public static void InitializeLogging()
	{
#if DEBUG
		// Logging is disabled by default for release builds, as it incurs a significant
		// initialization cost from Microsoft.Extensions.Logging setup. If startup performance
		// is a concern for your application, keep this disabled. If you're running on the web or
		// desktop targets, you can use URL or command line parameters to enable it.
		//
		// For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

		var factory = LoggerFactory.Create(builder =>
		{
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#else
			builder.AddConsole();
#endif

			// Exclude logs below this level
			builder.SetMinimumLevel(LogLevel.Information);

			// Default filters for Uno Platform namespaces
			builder.AddFilter("Uno", LogLevel.Warning);
			builder.AddFilter("Windows", LogLevel.Warning);
			builder.AddFilter("Microsoft", LogLevel.Warning);

			// Generic Xaml events
			// builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

			// Layouter specific messages
			// builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

			// builder.AddFilter("Windows.Storage", LogLevel.Debug );

			// Binding related messages
			// builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
			// builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

			// Binder memory references tracking
			// builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

			// DevServer and HotReload related
			// builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

			// Debug JS interop
			// builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
		});

		global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
	}
}

