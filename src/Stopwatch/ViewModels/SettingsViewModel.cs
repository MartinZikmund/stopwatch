using Microsoft.UI;
using Stopwatch.Core.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;
using Windows.UI;
using ColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace Stopwatch.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
	private readonly IAppPreferences _appSettings;
	private readonly IThemeManager _themeManager;
	private readonly IImagePickerService _imagePickerService;
	private readonly IDataSource _dataSource;

	[ObservableProperty]
	private Uri? _lastBackgroundImageUri;

	[ObservableProperty]
	private Uri? _backgroundImageUri;

	public SettingsViewModel(
		INavigationService navigationService,
		IAppPreferences appSettings,
		IThemeManager themeManager,
		IImagePickerService imagePickerService,
		IDataSource dataSource) : base(navigationService)
	{
		_appSettings = appSettings;
		_themeManager = themeManager;
		_imagePickerService = imagePickerService;
		_dataSource = dataSource;
	}

	public override void GoBack()
	{
		SaveChanges();

		base.GoBack();
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

	[RelayCommand]
	private async Task PickBackgroundImageAsync()
	{
		IsWorking = true;
		BackgroundImageUri = (await _imagePickerService.PickAsync()) ?? LastBackgroundImageUri;
		SaveChanges();
		IsWorking = false;
	}

	private void SaveChanges()
	{
		var stopwatch = _dataSource.GetOrCreateFirst();
		stopwatch.BackgroundImageUri = BackgroundImageUri?.ToString();
		_dataSource.Update(stopwatch);
	}
}
