namespace Stopwatch.Dialogs;

public sealed partial class ThirdPartySoftwareDialog : ContentDialog
{
	public ThirdPartySoftwareDialog()
	{
		this.InitializeComponent();

		// Initialize the list of packages from generated source
		PackagesList.ItemsSource = GeneratedPackageInfo.GetPackages();
	}
}

public record PackageInfo(string Name, string Version, string Url);
