using System.Reflection;
using Windows.UI.Core;

namespace Stopwatch.Services.Navigation;

public class NavigationService : INavigationService
{
	private readonly Dictionary<string, Type> _views = new();
	private readonly IFrameProvider _frameProvider;

	public NavigationService(IFrameProvider frameProvider)
	{
		_frameProvider = frameProvider ?? throw new ArgumentNullException(nameof(frameProvider));
	}

	private Frame Frame => _frameProvider.GetForCurrentScope();

	public bool CanGoBack => Frame.CanGoBack;

	public bool GoBack()
	{
		if (Frame.CanGoBack)
		{
			Frame.GoBack();
			return true;
		}
		return false;
	}

	public void Navigate<TViewModel>() => Navigate<TViewModel>(null);

	public void Navigate<TViewModel>(object? parameter)
	{
		if (!TryFindViewForViewModel(typeof(TViewModel), out var viewType))
		{
			throw new InvalidOperationException($"ViewModel type {typeof(TViewModel).Name} is not registered for navigation.");
		}

		Frame.Navigate(viewType, parameter);
	}

	private bool TryFindViewForViewModel(Type viewModelType, out Type? viewType)
	{
		if (!viewModelType.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("ViewModel name must end with 'ViewModel' by convention.");
		}

		var viewModelName = viewModelType.Name;
		return _views.TryGetValue(viewModelName.Substring(0, viewModelName.Length - "Model".Length), out viewType);
	}

	public void RegisterViewsFromAssembly(Assembly sourceAssembly)
	{
		_views.Add(typeof(Stopwatch.Views.HistoryView).Name, typeof(Stopwatch.Views.HistoryView));
		_views.Add(typeof(Stopwatch.Views.GetProView).Name, typeof(Stopwatch.Views.GetProView));
		_views.Add(typeof(Stopwatch.Views.SettingsView).Name, typeof(Stopwatch.Views.SettingsView));
		_views.Add(typeof(Stopwatch.Views.MainView).Name, typeof(Stopwatch.Views.MainView));
		_views.Add(typeof(Stopwatch.Views.OnboardingView).Name, typeof(Stopwatch.Views.OnboardingView));
	}

	public void Initialize()
	{
#if HAS_UNO
		SystemNavigationManager.GetForCurrentView().BackRequested += NavigationManagerBackRequested;
#endif
	}

	private void NavigationManagerBackRequested(object? sender, BackRequestedEventArgs? e)
	{
		if (GoBack())
		{
			e.Handled = true;
		}
	}

	public void ClearBackStack() => Frame.BackStack.Clear();
}
