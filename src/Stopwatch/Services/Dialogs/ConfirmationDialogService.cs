using Microsoft.Extensions.Localization;
using MZikmund.Toolkit.WinUI.Infrastructure;
using MZikmund.Toolkit.WinUI.Services;
using Stopwatch.Services.Localization;

namespace Stopwatch.Services;

public class ConfirmationDialogService : IConfirmationDialogService
{
	private readonly IStringLocalizer _localization;
	private readonly IDialogCoordinator _dialogCoordinator;
	private readonly IXamlRootProvider _xamlRootProvider;

	public ConfirmationDialogService(
		IStringLocalizer localization,
		IDialogCoordinator dialogCoordinator,
		IXamlRootProvider xamlRootProvider)
	{
		_localization = localization ?? throw new ArgumentNullException(nameof(localization));
		_dialogCoordinator = dialogCoordinator ?? throw new ArgumentNullException(nameof(dialogCoordinator));
		_xamlRootProvider = xamlRootProvider ?? throw new ArgumentNullException(nameof(xamlRootProvider));
	}

	public async Task<ConfirmationResult> ShowAsync(string title, string text)
	{
		ContentDialog dialog = new()
		{
			Title = title,
			Content = text,
			PrimaryButtonText = Localizer.Instance.GetString("Yes"),
			SecondaryButtonText = Localizer.Instance.GetString("No"),
			DefaultButton = ContentDialogButton.Secondary,
			XamlRoot = _xamlRootProvider.XamlRoot
		};

		var result = await _dialogCoordinator.ShowAsync(dialog);
		if (result == ContentDialogResult.Primary)
		{
			return ConfirmationResult.Confirmed;
		}

		return ConfirmationResult.Denied;
	}
}
