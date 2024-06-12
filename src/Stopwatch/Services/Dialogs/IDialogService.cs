using System.Reflection;

namespace MZikmund.Services.Dialogs;

public interface IDialogService
{
	Task<ContentDialogResult> ShowAsync<TViewModel>(TViewModel viewModel);

	Task<ContentDialogResult> ShowAsync(string title, string content);

	void RegisterDialogsFromAssembly(Assembly assembly);
}
