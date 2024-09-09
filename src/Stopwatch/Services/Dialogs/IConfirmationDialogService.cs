using Stopwatch.Services;

namespace Stopwatch.Services;

public interface IConfirmationDialogService
{
	Task<ConfirmationResult> ShowAsync(string title, string text);
}
