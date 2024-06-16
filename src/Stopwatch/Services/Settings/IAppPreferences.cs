namespace Stopwatch.Services.Settings;

public interface IAppPreferences
{
	int DataVersion { get; set; }

	bool FirstStart { get; set; }

	int LaunchCount { get; set; }

	bool OfferUserRating { get; set; }

	AppTheme Theme { get; set; }

    TimeSpan CurrentStopwatchTime { get; set; }
}
