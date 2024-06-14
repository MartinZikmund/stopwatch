namespace Stopwatch.Services.Settings;

public class AppSettings : IAppSettings
{
	private readonly ISettingsService _settingsService;

	public AppSettings(ISettingsService settingsService)
	{
		_settingsService = settingsService;
	}

	private const string DataVersionKey = "AppDataVersion";

	public int DataVersion
	{
		get => _settingsService.GetSetting(DataVersionKey, () => 0);
		set => _settingsService.SetSetting(DataVersionKey, value);
	}

	private const string FirstStartKey = "AppFirstStart";

	public bool FirstStart
	{
		get => _settingsService.GetSetting(FirstStartKey, () => true);
		set => _settingsService.SetSetting(FirstStartKey, value);
	}

	private const string LaunchCountKey = "AppLaunchCount";

	public int LaunchCount
	{
		get => _settingsService.GetSetting(LaunchCountKey, () => 0);
		set => _settingsService.SetSetting(LaunchCountKey, value);
	}

	private const string OfferUserRatingKey = "OfferUserRating";

	public bool OfferUserRating
	{
		get => _settingsService.GetSetting(OfferUserRatingKey, () => true, true);
		set => _settingsService.SetSetting(OfferUserRatingKey, value, true);
	}

	private const string AppThemeKey = "AppTheme";

	public AppTheme Theme
	{
		get => _settingsService.GetSetting(AppThemeKey, () => AppTheme.System, true);
		set => _settingsService.SetSetting(AppThemeKey, value, true);
	}

    private const string CurrentStopwatchTimeKey = "CurrentStopwatchTime";

    public TimeSpan CurrentStopwatchTime
    {
        get => _settingsService.GetSetting(CurrentStopwatchTimeKey, () => TimeSpan.Zero);
        set => _settingsService.SetSetting(CurrentStopwatchTimeKey, value);
    }
}
