using MZikmund.Toolkit.WinUI.Services;
using Stopwatch.Model;

namespace Stopwatch.Services.Settings;

public class AppPreferences : IAppPreferences
{
	private readonly IPreferences _preferences;

	public AppPreferences(IPreferences preferences)
	{
		_preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
	}

	private const string DataVersionKey = "AppDataVersion";

	public int DataVersion
	{
		get => _preferences.Get(DataVersionKey, 0);
		set => _preferences.Set(DataVersionKey, value);
	}

	private const string FirstStartKey = "AppFirstStart";

	public bool FirstStart
	{
		get => _preferences.Get(FirstStartKey, true);
		set => _preferences.Set(FirstStartKey, value);
	}

	private const string LaunchCountKey = "AppLaunchCount";

	public int LaunchCount
	{
		get => _preferences.Get(LaunchCountKey, 0);
		set => _preferences.Set(LaunchCountKey, value);
	}

	private const string OfferUserRatingKey = "OfferUserRating";

	public bool OfferUserRating
	{
		get => _preferences.Get(OfferUserRatingKey, true);
		set => _preferences.Set(OfferUserRatingKey, value);
	}

	private const string AppThemeKey = "AppTheme";

	public AppTheme Theme
	{
		get => _preferences.GetComplex<AppTheme>(AppThemeKey, AppTheme.System);
		set => _preferences.SetComplex(AppThemeKey, value);
	}

    private const string CurrentStopwatchKey = "CurrentStopwatch";

    public StopwatchModel? CurrentStopwatch
	{
        get => _preferences.GetComplex<StopwatchModel?>(CurrentStopwatchKey, null);
        set => _preferences.SetComplex(CurrentStopwatchKey, value);
    }
}
