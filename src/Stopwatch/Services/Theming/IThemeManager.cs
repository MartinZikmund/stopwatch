namespace Stopwatch.Services.Theming;

public interface IThemeManager : IDisposable
{
	void SetTheme(ElementTheme theme);

	ElementTheme CurrentTheme { get; }

	ApplicationTheme ActualTheme { get; }
}
