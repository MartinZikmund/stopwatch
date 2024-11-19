using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Stopwatch.Extensions;
using Stopwatch.Services.Navigation;
using Stopwatch.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Stopwatch.Services.Dialogs;
public sealed partial class ProOnlyFeatureDialog : ContentDialog
{
	public ProOnlyFeatureDialog()
	{
		this.InitializeComponent();
	}

	private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
	{
		sender.GetServiceProvider()?.GetRequiredService<INavigationService>().Navigate<GetProViewModel>();
	}
}
