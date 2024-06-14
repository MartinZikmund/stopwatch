using Stopwatch.Services.Settings;

namespace Stopwatch.Services;

public class SettingsService : ISettingsService
{
	public T GetSetting<T>(string key, Func<T> defaultValueBuilder, bool roamed = false)
	{
		var container = roamed ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
		if (container.Values.TryGetValue(key, out var value))
		{
			//get existing
			try
			{
				return (T)value;
			}
			catch
			{
				//invalid value, remove
				container.Values.Remove(key);
			}
		}
		return defaultValueBuilder();
	}

	public void SetSetting<T>(string key, T value, bool roamed = false)
	{
		var container = roamed ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
		if (container.Values.ContainsKey(key))
		{
			container.Values[key] = value;
		}
		else
		{
			container.Values.Add(key, value);
		}
	}
}
