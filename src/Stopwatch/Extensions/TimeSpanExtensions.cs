namespace Stopwatch.Extensions;

public static class TimeSpanExtensions
{
	public static string ToStopwatchString(this TimeSpan timeSpan, bool includeFractionsOfSeconds)
	{
		var totalWholeHours = (int)timeSpan.TotalHours;
		return !includeFractionsOfSeconds
			? $"{totalWholeHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
			: $"{totalWholeHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.ToFractionsOfSecondsString()}";
	}

	public static string ToFractionsOfSecondsString(this TimeSpan timeSpan) => $"{timeSpan.Milliseconds / 10:D2}";
}
