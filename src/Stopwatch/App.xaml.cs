using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI;
using Microsoft.Toolkit.Uwp.Helpers;
using MZikmund.Toolkit.WinUI.Infrastructure;
using MZikmund.Toolkit.WinUI.Services;
using Stopwatch.Core.Services;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Data.LiteDb;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Store;
using Stopwatch.Services.Theming;
using Stopwatch.Services.Timer;
using Stopwatch.ViewModels;

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
		MainWindow.EnableHotReload();
#endif

		Host = builder.Build();
		Ioc.Default.ConfigureServices(Host.Services);
		await (Host.Services.GetRequiredService<IDataSource>()).InitializeAsync();
		Host.Services.GetRequiredService<SystemInformation>().TrackAppUse(args.UWPLaunchActivatedEventArgs);

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
		services.AddSingleton<IDataSource, LiteDbDataSource>();
		services.AddSingleton<IHistoryService, HistoryService>();
		services.AddSingleton<IDisplayRequestManager, DisplayRequestManager>();
		services.AddSingleton<IPreferences, Preferences>();
		services.AddSingleton<IAppPreferences, AppPreferences>();
		services.AddSingleton<SystemInformation>();

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
#if DEBUG
		services.AddScoped<IStoreService, FakeStoreService>();
#else
		services.AddScoped<IStoreService, StoreService>();
#endif

		services.AddScoped<IXamlRootProvider, XamlRootProvider>();
	}
}
