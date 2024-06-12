namespace Stopwatch.Services.ConfirmationDialog;

public interface IConfirmationDialogService
{
	Task ShowAsync(string title, string text, Action yesAction, Action noAction);
}
