using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;
using Windows.UI;
using ColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

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

	public ElementTheme[] ThemeOptions { get; } = [ElementTheme.Default, ElementTheme.Light, ElementTheme.Dark];

	public ElementTheme SelectedTheme
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

	public Color BackgroundColor
	{
		get => _appSettings.BackgroundColor is not null ? ColorHelper.ToColor(_appSettings.BackgroundColor) : Colors.Transparent;
		set
		{
			if (_appSettings.BackgroundColor != ColorHelper.ToHex(value))
			{
				_appSettings.BackgroundColor = ColorHelper.ToHex(value);
				OnPropertyChanged();
			}
		}
	}
}
