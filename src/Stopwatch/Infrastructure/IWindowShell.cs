using Microsoft.UI.Dispatching;
using Stopwatch.ViewModels;

namespace Stopwatch.Infrastructure;

public interface IWindowShell
{
	WindowShellViewModel ViewModel { get; }

	XamlRoot? XamlRoot { get; }

	IServiceProvider ServiceProvider { get; }

	DispatcherQueue DispatcherQueue { get; }

	Frame RootFrame { get; }

	void SetTitleBar(UIElement? titleBar);
}
