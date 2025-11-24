using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using MZikmund.Toolkit.WinUI.Infrastructure;
using Stopwatch.Core.Services;
using Stopwatch.Dialogs;
using Stopwatch.Models;
using Stopwatch.Services;
using Stopwatch.Services.Data;
using Stopwatch.Services.Dialogs;
using Stopwatch.Services.Localization;
using Stopwatch.Services.Navigation;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Store;
using Stopwatch.Services.Theming;
using Windows.Services.Store;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Stopwatch.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
	private readonly IAppPreferences _appSettings;
	private readonly IThemeManager _themeManager;
	private readonly IImagePickerService _imagePickerService;
	private readonly IXamlRootProvider _xamlRootProvider;
	private readonly IStoreService _storeService;
	private readonly IDialogService _dialogService;
	private readonly IDataSource _dataSource;

	private readonly UISettings _uiSettings = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundImageOpacity))]
	public partial double BackgroundImageOpacityPercent { get; set; }

	private StopwatchModel _stopwatch;

	[ObservableProperty]
	public partial ElementTheme Theme { get; set; }

	[ObservableProperty]
	public partial Uri? LastBackgroundImageUri { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsBackgroundImageSet))]
	public partial Uri? BackgroundImageUri { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsBackgroundColorSet))]
	public partial Color BackgroundColor { get; set; }

	[ObservableProperty]
	public partial bool HasProLicense { get; set; }

	private bool _isInitializing = false;

	public SettingsViewModel(
		INavigationService navigationService,
		IAppPreferences appSettings,
		IThemeManager themeManager,
		IImagePickerService imagePickerService,
		IXamlRootProvider xamlRootProvider,
		IStoreService storeService,
		IDialogService dialogService,
		IDataSource dataSource) : base(navigationService)
	{
		_appSettings = appSettings;
		_themeManager = themeManager;
		_imagePickerService = imagePickerService;
		_xamlRootProvider = xamlRootProvider;
		_storeService = storeService;
		_dialogService = dialogService;
		_dataSource = dataSource;
	}

	public override async void ViewNavigatedTo(object? parameter)
	{
		base.ViewNavigatedTo(parameter);
		try
		{
			_isInitializing = true;
			HasProLicense = await _storeService.HasProAsync();

			if (parameter is int stopwatchId)
			{
				if (_dataSource.Stopwatches.Get(stopwatchId) is not { } stopwatch)
				{
					throw new InvalidOperationException("Stopwatch with ID " + stopwatchId + " does not exist.");
				}

				_stopwatch = stopwatch;
				Theme = _stopwatch.Theme;

				BackgroundImageUri = _stopwatch.BackgroundImageUri is not null ? new(_stopwatch.BackgroundImageUri) : null;
				BackgroundImageOpacityPercent = _stopwatch.BackgroundImageOpacity * 100;
				BackgroundColor = ColorHelper.ToColor(_stopwatch.BackgroundColor);
			}
		}
		finally
		{
			_isInitializing = false;
		}
	}

	public override void GoBack()
	{
		SaveChanges();

		base.GoBack();
	}

	public ElementTheme[] ThemeOptions { get; } = [ElementTheme.Default, ElementTheme.Light, ElementTheme.Dark];

	partial void OnThemeChanged(ElementTheme value)
	{
		_themeManager.SetTheme(Theme);
		SaveChanges();
	}

	public bool KeepScreenOn
	{
		get => _appSettings.KeepScreenOn;
		set
		{
			if (_appSettings.KeepScreenOn != value)
			{
				_appSettings.KeepScreenOn = value;
				OnPropertyChanged();
			}
		}
	}

	partial void OnBackgroundImageOpacityPercentChanged(double value) => SaveChanges();

	public double BackgroundImageOpacity => BackgroundImageOpacityPercent / 100;

	public bool IsBackgroundImageSet => BackgroundImageUri is not null;

	public bool IsBackgroundColorSet => BackgroundColor != Colors.Transparent;

	public string PackageVersionString => Package.Current.Id.Version.ToFormattedString();

	[RelayCommand]
	private async Task ReviewAppAsync() => await StoreContext.GetDefault().RequestRateAndReviewAppAsync();

	[RelayCommand]
	private async Task PickBackgroundImageAsync()
	{
		if (!HasProLicense)
		{
			var proOnlyFeatureDialog = new ProOnlyFeatureDialog();
			await _dialogService.ShowAsync(proOnlyFeatureDialog);
			return;
		}

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

#if DEBUG
	[RelayCommand]
	public void AddTestData()
	{
		_dataSource.Stopwatches.Add(new StopwatchModel() { Name = "Many hours", PausedElapsedTime = TimeSpan.FromHours(234.32) });
	}
#endif

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

	[RelayCommand]
	private async Task RestorePurchasesAsync()
	{
		IsWorking = true;
		try
		{
			var result = await _storeService.TryRestorePurchasesAsync();

			if (result)
			{
				await _dialogService.ShowAsync(
					Localizer.Instance.GetString("RestorePurchases"),
					Localizer.Instance.GetString("PurchasesRestored"));
				HasProLicense = true;
			}
		}
		finally
		{
			IsWorking = false;
		}
	}

	[RelayCommand]
	private async Task ShowThirdPartySoftwareAsync()
	{
		var dialog = new ThirdPartySoftwareDialog
		{
			XamlRoot = _xamlRootProvider.XamlRoot
		};
		await dialog.ShowAsync();
	}

	[RelayCommand]
	private void ShowOnboardingTips()
	{
		SaveChanges();
		// Navigate to MainView with a special parameter to trigger teaching tips
		NavigationService.Navigate<MainViewModel>("ShowTeachingTips");
	}

	private void SaveChanges()
	{
		if (_isInitializing)
		{
			return;
		}

		_stopwatch.Theme = Theme;
		_stopwatch.BackgroundImageUri = BackgroundImageUri?.ToString();
		_stopwatch.BackgroundImageOpacity = BackgroundImageOpacity;
		_stopwatch.BackgroundColor = ColorHelper.ToHex(BackgroundColor);
		_dataSource.Stopwatches.Update(_stopwatch);
	}
}
