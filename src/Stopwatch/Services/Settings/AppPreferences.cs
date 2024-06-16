using MZikmund.Services.Preferences;

namespace Stopwatch.Services.Settings;

public class AppPreferences : IAppPreferences
{
	private readonly IPreferencesService _preferencesService;

	public AppPreferences(IPreferencesService preferencesService)
	{
		_preferencesService = preferencesService;
	}

	private const string DataVersionKey = "AppDataVersion";

	public int DataVersion
	{
		get => _preferencesService.GetSetting(DataVersionKey, () => 0);
		set => _preferencesService.SetSetting(DataVersionKey, value);
	}

	private const string FirstStartKey = "AppFirstStart";

	public bool FirstStart
	{
		get => _preferencesService.GetSetting(FirstStartKey, () => true);
		set => _preferencesService.SetSetting(FirstStartKey, value);
	}

	private const string LaunchCountKey = "AppLaunchCount";

	public int LaunchCount
	{
		get => _preferencesService.GetSetting(LaunchCountKey, () => 0);
		set => _preferencesService.SetSetting(LaunchCountKey, value);
	}

	private const string OfferUserRatingKey = "OfferUserRating";

	public bool OfferUserRating
	{
		get => _preferencesService.GetSetting(OfferUserRatingKey, () => true, true);
		set => _preferencesService.SetSetting(OfferUserRatingKey, value, true);
	}

	private const string AppThemeKey = "AppTheme";

	public AppTheme Theme
	{
		get => _preferencesService.GetComplex(AppThemeKey, () => AppTheme.System, true);
		set => _preferencesService.SetComplex(AppThemeKey, value, true);
	}

    private const string CurrentStopwatchTimeKey = "CurrentStopwatchTime";

    public TimeSpan CurrentStopwatchTime
    {
        get => _preferencesService.GetSetting(CurrentStopwatchTimeKey, () => TimeSpan.Zero);
        set => _preferencesService.SetSetting(CurrentStopwatchTimeKey, value);
    }
}
