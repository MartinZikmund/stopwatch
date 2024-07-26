using CommunityToolkit.WinUI.Converters;
using Microsoft.UI;
using MZikmund.Toolkit.WinUI.Infrastructure;
using Stopwatch.Core.Services;
using Stopwatch.Dialogs;
using Stopwatch.Services.Data;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Theming;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Stopwatch.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
	private readonly IAppPreferences _appSettings;
	private readonly IThemeManager _themeManager;
	private readonly IImagePickerService _imagePickerService;
	private readonly IXamlRootProvider _xamlRootProvider;
	private readonly IDataSource _dataSource;

	private readonly UISettings _uiSettings = new();

	private double _backgroundImageOpacityPercent;

	[ObservableProperty]
	private Uri? _lastBackgroundImageUri;

	[ObservableProperty]
	private Uri? _backgroundImageUri;

	[ObservableProperty]
	private Color _backgroundColor;

	public SettingsViewModel(
		INavigationService navigationService,
		IAppPreferences appSettings,
		IThemeManager themeManager,
		IImagePickerService imagePickerService,
		IXamlRootProvider xamlRootProvider,
		IDataSource dataSource) : base(navigationService)
	{
		_appSettings = appSettings;
		_themeManager = themeManager;
		_imagePickerService = imagePickerService;
		_xamlRootProvider = xamlRootProvider;
		_dataSource = dataSource;

		var stopwatch = _dataSource.GetOrCreateFirst();
		BackgroundImageUri = stopwatch.BackgroundImageUri is not null ? new(stopwatch.BackgroundImageUri) : null;
		BackgroundImageOpacityPercent = stopwatch.BackgroundImageOpacity * 100;
		BackgroundColor = ColorHelper.ToColor(stopwatch.BackgroundColor);
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

	public double BackgroundImageOpacityPercent
	{
		get => _backgroundImageOpacityPercent;
		set
		{
			if (_backgroundImageOpacityPercent != value)
			{
				_backgroundImageOpacityPercent = value;
				SaveChanges();
				OnPropertyChanged(nameof(BackgroundImageOpacity));
			}
		}
	}

	public double BackgroundImageOpacity => BackgroundImageOpacityPercent / 100;

	public bool IsBackgroundImageSet => BackgroundImageUri is not null;

	public bool IsBackgroundColorSet => BackgroundColor != Colors.Transparent;

	[RelayCommand]
	private async Task PickBackgroundImageAsync()
	{
		IsWorking = true;
		try
		{

			if (await _imagePickerService.PickAsync() is { } imageUri)
			{
				BackgroundImageUri = imageUri;
				OnPropertyChanged(nameof(IsBackgroundImageSet));
			}

			SaveChanges();
		}
		finally
		{
			IsWorking = false;
		}
	}

	[RelayCommand]
	private async Task PickBackgroundColor()
	{
		IsWorking = true;
		var pickerDialog = new ColorPickerDialog
		{
			XamlRoot = _xamlRootProvider.XamlRoot,
			SelectedColor = IsBackgroundColorSet ? BackgroundColor : _uiSettings.GetColorValue(UIColorType.Accent),
		};

		if (await pickerDialog.ShowAsync() == ContentDialogResult.Primary)
		{
			BackgroundColor = pickerDialog.SelectedColor;
			OnPropertyChanged(nameof(IsBackgroundColorSet));
			SaveChanges();
		}
		IsWorking = false;
	}

	[RelayCommand]
	private void RemoveBackgroundImage()
	{
		BackgroundImageUri = null;
		OnPropertyChanged(nameof(IsBackgroundImageSet));
		SaveChanges();
	}

	[RelayCommand]
	private void RemoveBackgroundColor()
	{
		BackgroundColor = Colors.Transparent;
		OnPropertyChanged(nameof(IsBackgroundColorSet));
		SaveChanges();
	}

	private void SaveChanges()
	{
		var stopwatch = _dataSource.GetOrCreateFirst();
		stopwatch.BackgroundImageUri = BackgroundImageUri?.ToString();
		stopwatch.BackgroundImageOpacity = BackgroundImageOpacityPercent / 100;
		stopwatch.BackgroundColor = ColorHelper.ToHex(BackgroundColor);
		_dataSource.Update(stopwatch);
	}
}
