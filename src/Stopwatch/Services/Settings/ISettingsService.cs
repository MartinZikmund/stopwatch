namespace Stopwatch.Services.Settings;

public interface ISettingsService
{
	T GetSetting<T>(string key, Func<T> defaultValueBuilder, bool roamed = false);
	void SetSetting<T>(string key, T value, bool roamed = false);
}
