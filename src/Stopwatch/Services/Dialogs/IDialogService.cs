using System.Reflection;

namespace Stopwatch.Services;

public interface IDialogService
{
	Task<ContentDialogResult> ShowAsync(object viewModel);

	Task<ContentDialogResult> ShowAsync(string title, string content);

	Task<ContentDialogResult> ShowAsync(ContentDialog contentDialog);

	void RegisterDialogsFromAssembly(Assembly assembly);
}
