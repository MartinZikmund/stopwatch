using Stopwatch.Services.ConfirmationDialog;
using MZikmund.Services.Dialogs;
using Microsoft.Extensions.Localization;

namespace Stopwatch.Services;

public class ConfirmationDialogService : IConfirmationDialogService
{
	private readonly IStringLocalizer _localization;
	private readonly IDialogCoordinator _dialogCoordinator;

	public ConfirmationDialogService(IStringLocalizer localization, IDialogCoordinator dialogCoordinator)
	{
		_localization = localization ?? throw new ArgumentNullException(nameof(localization));
		_dialogCoordinator = dialogCoordinator ?? throw new ArgumentNullException(nameof(dialogCoordinator));
	}

	public async Task ShowAsync(string title, string text, Action yesAction, Action noAction)
	{
		ContentDialog dialog = new()
		{
			Title = title,
			Content = text,
			PrimaryButtonText = _localization.GetString("Yes"),
			SecondaryButtonText = _localization.GetString("No"),
		};
		var result = await _dialogCoordinator.ShowAsync(dialog);
		if (result == ContentDialogResult.Primary)
		{
			yesAction();
		}
		else if (result == ContentDialogResult.Secondary)
		{
			noAction();
		}
	}
}
