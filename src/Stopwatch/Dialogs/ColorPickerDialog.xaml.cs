using Windows.UI;

namespace Stopwatch.Dialogs
{
	public sealed partial class ColorPickerDialog : ContentDialog
	{
		public ColorPickerDialog()
		{
			this.InitializeComponent();
		}

		public ColorPicker ColorPicker => Picker;

		public Color SelectedColor
		{
			get => Picker.Color;
			set => Picker.Color = value;
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}
	}
}
