using Microsoft.UI.Xaml.Data;
using Stopwatch.Services.Localization;

namespace Stopwatch.Converter;

public class EnumLocalizationConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is Enum enumValue)
		{
			var enumType = enumValue.GetType();
			var enumName = Enum.GetName(enumType, enumValue);
			var key = $"{enumType.Name}_{enumName}";
			return Localizer.Instance.GetString(key);
		}

		return "";
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
