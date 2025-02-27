using Microsoft.UI.Xaml.Markup;

namespace Stopwatch.Extensions.Xaml;

[MarkupExtensionReturnType(ReturnType = typeof(FontIcon))]
public class FontIconExtension : MarkupExtension
{
	public string Glyph { get; set; } = "";

	protected override object ProvideValue() => new FontIcon() { Glyph = Glyph };
}
