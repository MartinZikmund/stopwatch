using Microsoft.UI.Xaml.Markup;
using Stopwatch.Services.Localization;

namespace Stopwatch.Extensions.Xaml;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public class LocalizeExtension : MarkupExtension
{
	public string Key { get; set; } = "";

	protected override object ProvideValue() => Localizer.Instance.GetString(Key);
}
