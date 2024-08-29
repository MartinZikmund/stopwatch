using Stopwatch.Services;
using MZikmund.Toolkit.WinUI.Infrastructure;
using Stopwatch.Infrastructure;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;
using Stopwatch.ViewModels;
using Windows.Foundation.Metadata;

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

		Loading += WindowShell_Loading;
	}

	private void WindowShell_Loading(FrameworkElement sender, object args)
	{
		((XamlRootProvider)ServiceProvider.GetRequiredService<IXamlRootProvider>()).XamlRoot = XamlRoot ?? throw new InvalidOperationException("XamlRoot must be set");
	}

	public IServiceProvider ServiceProvider => _windowScope.ServiceProvider;

	public WindowShellViewModel ViewModel { get; }

	public Frame RootFrame => InnerFrame;

	public bool HasCustomTitleBar { get; private set; }

	private void CustomizeWindow()
	{
		if (ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Window", "ExtendsContentIntoTitleBar"))
		{
#if !HAS_UNO
			_associatedWindow.ExtendsContentIntoTitleBar = true;
			// TODO: The title bar grid will need to be resized along with TabBar
			// _associatedWindow.SetTitleBar(TitleBarGrid);
			HasCustomTitleBar = true;
#endif
		}
		if (ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Window", "SystemBackdrop"))
		{
			_associatedWindow.SystemBackdrop = new MicaBackdrop();
			Background = null;
		}
	}
}
