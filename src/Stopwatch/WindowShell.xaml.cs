using Windows.Foundation.Metadata;
using Stopwatch.Infrastructure;
using Stopwatch.Services.Navigation;
using Stopwatch.ViewModels;
using MZikmund.Services.Dialogs;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;

namespace Stopwatch;

public sealed partial class WindowShell : Page, IWindowShell
{
    private readonly IServiceScope _windowScope;
    private readonly Window _associatedWindow;

    public WindowShell(IServiceProvider serviceProvider, Window associatedWindow)
    {
        InitializeComponent();

        _windowScope = serviceProvider.CreateScope();
        var windowShellProvider = (WindowShellProvider)ServiceProvider.GetRequiredService<IWindowShellProvider>();
        windowShellProvider.SetShell(this, associatedWindow);
        ServiceProvider.GetRequiredService<INavigationService>().RegisterViewsFromAssembly(typeof(Stopwatch.App).Assembly);
        ServiceProvider.GetRequiredService<IDialogService>().RegisterDialogsFromAssembly(typeof(Stopwatch.App).Assembly);

		var settings = ServiceProvider.GetRequiredService<IAppPreferences>();
		var themeService = ServiceProvider.GetRequiredService<IThemeManager>();
		themeService.SetTheme(settings.Theme);

		ViewModel = ServiceProvider.GetRequiredService<WindowShellViewModel>();

        //_uiSettings.ColorValuesChanged += ColorValuesChanged;
        _associatedWindow = associatedWindow;
        CustomizeWindow();

        //Loaded += WindowShell_Loaded;
    }

    public IServiceProvider ServiceProvider => _windowScope.ServiceProvider;

    private void WindowShell_Loaded(object sender, RoutedEventArgs e)
    {
        SetTitlebarColors();
    }

    public WindowShellViewModel ViewModel { get; }

    public Frame RootFrame => InnerFrame;

    public bool HasCustomTitleBar { get; private set; }

    private void CustomizeWindow()
    {
        if (ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Window", "ExtendsContentIntoTitleBar"))
        {
#if !HAS_UNO
            _associatedWindow.ExtendsContentIntoTitleBar = true;
            _associatedWindow.SetTitleBar(TitleBarGrid);
            HasCustomTitleBar = true;
#endif
        }
        if (ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Window", "SystemBackdrop"))
        {
            _associatedWindow.SystemBackdrop = new MicaBackdrop();
            Background = null;
        }
    }

    //	private async void ColorValuesChanged(UISettings sender, object args)
    //	{
    //		await Dispatcher.RunAsync(
    //			CoreDispatcherPriority.Normal,
    //			SetTitlebarColors);
    //	}

    private void SetTitlebarColors()
    {

        //TODO:Titlebar colors
        //#pragma warning disable CS8618
        //#pragma warning disable Uno0001
        //		var brandColor = ColorResources.BrandColor;
        //		var titleBar = ApplicationView.GetForCurrentView().TitleBar;
        //		titleBar.BackgroundColor = brandColor;
        //		titleBar.ButtonBackgroundColor = Colors.Transparent;
        //		titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        //		if (Ioc.Default.GetRequiredService<IThemeManager>().CurrentTheme == AppTheme.Dark)
        //		{
        //			titleBar.ButtonForegroundColor = Colors.White;
        //			titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        //			titleBar.ButtonHoverBackgroundColor = Color.FromArgb(100, 100, 100, 100);
        //			titleBar.ButtonHoverForegroundColor = Colors.White;
        //			titleBar.ButtonPressedBackgroundColor = Color.FromArgb(200, 100, 100, 100);
        //			titleBar.ButtonPressedForegroundColor = Colors.White;
        //		}
        //		else
        //		{
        //			titleBar.ButtonForegroundColor = Colors.Black;
        //			titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        //			titleBar.ButtonHoverBackgroundColor = Color.FromArgb(100, 200, 200, 200);
        //			titleBar.ButtonHoverForegroundColor = Colors.Black;
        //			titleBar.ButtonPressedBackgroundColor = Color.FromArgb(200, 200, 200, 200);
        //			titleBar.ButtonPressedForegroundColor = Colors.Black;
        //		}
        //#pragma warning restore Uno0001
        //#pragma warning restore CS8618
    }
}
