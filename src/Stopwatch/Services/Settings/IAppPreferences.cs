using Stopwatch.Models;

namespace Stopwatch.Services.Settings;

public interface IAppPreferences
{
	int DataVersion { get; set; }

	bool FirstStart { get; set; }

	int LaunchCount { get; set; }

	bool OfferUserRating { get; set; }

	ElementTheme Theme { get; set; }

	string BackgroundColor { get; set; }

	bool AutoHideButtons { get; set; }

	bool KeepScreenOn { get; set; }

	bool HasSeenRenameTeachingTip { get; set; }

	bool HasSeenTabsTeachingTip { get; set; }

	bool HasSeenLapsTeachingTip { get; set; }

	bool HasSeenExportTeachingTip { get; set; }
}
