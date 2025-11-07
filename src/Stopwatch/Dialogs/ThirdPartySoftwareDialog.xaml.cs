using System;
using System.Collections.Generic;

namespace Stopwatch.Dialogs;

public sealed partial class ThirdPartySoftwareDialog : ContentDialog
{
	public ThirdPartySoftwareDialog()
	{
		this.InitializeComponent();

		// Initialize the list of packages from Directory.Packages.props
		var packages = new List<PackageInfo>
		{
			new PackageInfo("CommunityToolkit.WinUI.Controls.SettingsControls", "8.1.240916", "https://www.nuget.org/packages/CommunityToolkit.WinUI.Controls.SettingsControls"),
			new PackageInfo("CommunityToolkit.WinUI.Converters", "8.1.240916", "https://www.nuget.org/packages/CommunityToolkit.WinUI.Converters"),
			new PackageInfo("CommunityToolkit.WinUI.Helpers", "8.1.240916", "https://www.nuget.org/packages/CommunityToolkit.WinUI.Helpers"),
			new PackageInfo("LiteDB", "5.0.21", "https://www.nuget.org/packages/LiteDB"),
			new PackageInfo("MZikmund.Toolkit.WinUI", "0.1.13-dev.43", "https://www.nuget.org/packages/MZikmund.Toolkit.WinUI"),
			new PackageInfo("Plugin.InAppBilling", "8.0.5", "https://www.nuget.org/packages/Plugin.InAppBilling"),
		};

		PackagesList.ItemsSource = packages;
	}
}

public record PackageInfo(string Name, string Version, string Url);
