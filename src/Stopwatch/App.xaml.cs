using CommunityToolkit.Mvvm.DependencyInjection;
using MZikmund.Services.Dialogs;
using MZikmund.Toolkit.WinUI.Services;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
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

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
				.UseLocalization()
				.ConfigureServices((context, services) => ConfigureServices(services))
            );
        MainWindow = builder.Window;
#if DEBUG
        MainWindow.EnableHotReload();
#endif

        Host = builder.Build();
        Ioc.Default.ConfigureServices(Host.Services);

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

    private void ConfigureServices(IServiceCollection services)
    {
		services.AddSingleton<IDataSource, LiteDbDataSource>();

        services.AddScoped<WindowShellViewModel>();
        services.AddScoped<SettingsViewModel>();
		services.AddScoped<MainViewModel>();
        services.AddScoped<OnboardingViewModel>();

        services.AddScoped<IDialogCoordinator, DialogCoordinator>();
        services.AddScoped<IFrameProvider, FrameProvider>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IWindowShellProvider, WindowShellProvider>();
		services.AddScoped<ITimerFactory, TimerFactory>();
		services.AddScoped<IThemeManager, ThemeManager>();
		services.AddScoped<IPreferences, Preferences>();
		services.AddScoped<IAppPreferences, AppPreferences>();
	}
}
