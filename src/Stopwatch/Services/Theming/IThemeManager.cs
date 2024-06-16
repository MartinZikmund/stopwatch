namespace Stopwatch.Services.Theming;

public interface IThemeManager
{
	void SetTheme(AppTheme theme);

	AppTheme CurrentTheme { get; }
}
