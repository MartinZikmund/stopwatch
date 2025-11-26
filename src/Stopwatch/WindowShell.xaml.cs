using Stopwatch.Services;
using MZikmund.Toolkit.WinUI.Infrastructure;
using Stopwatch.Infrastructure;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;
using Stopwatch.ViewModels;
using Windows.Foundation.Metadata;
using Microsoft.UI.Windowing;

namespace Stopwatch;

public sealed partial class WindowShell : Page, IWindowShell
{
	private readonly IServiceScope _windowScope;
	private readonly Window _associatedWindow;
	private bool _isWindowClosed;

	public WindowShell(IServiceProvider serviceProvider, Window associatedWindow)
	{
		InitializeComponent();

		_windowScope = serviceProvider.CreateScope();
		var windowShellProvider = (WindowShellProvider)ServiceProvider.GetRequiredService<IWindowShellProvider>();
		windowShellProvider.SetShell(this, associatedWindow);

		var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
		navigationService.Initialize();
		navigationService.RegisterViewsFromAssembly(typeof(Stopwatch.App).Assembly);
		ServiceProvider.GetRequiredService<IDialogService>().RegisterDialogsFromAssembly(typeof(Stopwatch.App).Assembly);

		var settings = ServiceProvider.GetRequiredService<IAppPreferences>();
		var themeService = ServiceProvider.GetRequiredService<IThemeManager>();
		themeService.SetTheme(settings.Theme);

		//_uiSettings.ColorValuesChanged += ColorValuesChanged;
		_associatedWindow = associatedWindow;
		_associatedWindow.Closed += OnWindowClosed;
		CustomizeWindow();

		ViewModel = ServiceProvider.GetRequiredService<WindowShellViewModel>();
		ViewModel.PropertyChanged += ViewModel_PropertyChanged;

		Loading += WindowShell_Loading;

		UpdateWindowTitle();
	}

	private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(WindowShellViewModel.Title))
		{
			UpdateWindowTitle();
		}
	}

	private void UpdateWindowTitle()
	{
		if (ViewModel.Title != null && !_isWindowClosed)
		{
			_associatedWindow.Title = ViewModel.Title;
		}
	}

	private void OnWindowClosed(object sender, WindowEventArgs args) => _isWindowClosed = true;

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
		if (AppWindowTitleBar.IsCustomizationSupported())
		{
			_associatedWindow.ExtendsContentIntoTitleBar = true;
			_associatedWindow.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
			// TODO: The title bar grid will need to be resized along with TabBar
			_associatedWindow.SetTitleBar(TitleBarGrid);
			HasCustomTitleBar = true;
		}
		if (ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.Window", "SystemBackdrop"))
		{
			_associatedWindow.SystemBackdrop = new MicaBackdrop();
			Background = null;
		}
	}

	public void SetTitleBar(UIElement? titleBar)
	{
		if (!_isWindowClosed)
		{
			_associatedWindow.SetTitleBar(titleBar ?? TitleBarGrid);
		}
	}
}
