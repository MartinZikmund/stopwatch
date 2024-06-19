using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;

namespace Stopwatch.ViewModels;

public class SettingsViewModel : PageViewModel
{
	private readonly IAppPreferences _appSettings;
	private readonly IThemeManager _themeManager;

	public SettingsViewModel(INavigationService navigationService, IAppPreferences appSettings, IThemeManager themeManager) : base(navigationService)
    {
		_appSettings = appSettings;
		_themeManager = themeManager;
	}

	public AppTheme[] ThemeOptions { get; } = [AppTheme.Light, AppTheme.Dark, AppTheme.System];

	public AppTheme SelectedTheme
	{
		get => _appSettings.Theme;
		set
		{
			if (_appSettings.Theme != value)
			{
				_appSettings.Theme = value;
				_themeManager.SetTheme(SelectedTheme);
				OnPropertyChanged();
			}
		}
	}
}
