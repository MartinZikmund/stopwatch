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

	public ElementTheme Theme
	{
		get => _preferences.GetComplex<ElementTheme>(AppThemeKey, ElementTheme.Default);
		set => _preferences.SetComplex(AppThemeKey, value);
	}

	private const string BackgroundColorKey = "BackgroundColor";

	public string? BackgroundColor
	{
		get => _preferences.Get<string?>(BackgroundColorKey, null);
		set => _preferences.Set(BackgroundColorKey, value);
	}

	private const string AutoHideButtonsKey = "AutoHideButtons";

	public bool AutoHideButtons
	{
		get => _preferences.Get(AutoHideButtonsKey, false);
		set => _preferences.Set(AutoHideButtonsKey, value);
	}

	private const string KeepScreenOnKey = "KeepScreenOn";

	public bool KeepScreenOn
	{
		get => _preferences.Get(KeepScreenOnKey, false);
		set => _preferences.Set(KeepScreenOnKey, value);
	}
}
