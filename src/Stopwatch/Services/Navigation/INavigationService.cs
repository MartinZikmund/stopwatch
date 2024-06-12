using System.Reflection;

namespace Stopwatch.Services.Navigation;

public interface INavigationService
{
	void ClearBackStack();

	void Navigate<TViewModel>();

	void Navigate<TViewModel>(object? parameter);

	bool GoBack();

	bool CanGoBack { get; }

	void Initialize();

	void RegisterViewsFromAssembly(Assembly sourceAssembly);
}
