namespace Stopwatch.Services.Theming;

public interface IThemeManager
{
	void SetTheme(ElementTheme theme);

	ElementTheme CurrentTheme { get; }
}
