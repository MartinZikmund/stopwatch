using System.Reflection;

namespace Stopwatch.Services;

public interface IDialogService
{
	Task<ContentDialogResult> ShowAsync<TViewModel>(TViewModel viewModel);

	Task<ContentDialogResult> ShowAsync(string title, string content);

	Task<ContentDialogResult> ShowAsync(ContentDialog contentDialog);

	void RegisterDialogsFromAssembly(Assembly assembly);
}
