using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Stopwatch.ViewModels;
using Stopwatch.Services.Data;
using Stopwatch.Services.Settings;
using Stopwatch.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stopwatch.Services.Navigation;

public class WindowManager : IWindowManager
{
    private readonly IServiceProvider _serviceProvider;
    // Track open stopwatches in secondary windows
    private static readonly HashSet<int> OpenedInWindow = new();
    public static IReadOnlyCollection<int> GetOpenedInWindowIds() => OpenedInWindow;

    public WindowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OpenStopwatchInNewWindowAsync(StopwatchViewModel stopwatchViewModel)
    {
        if (!IsMultiWindowSupported())
            return;

        var mainWindowProvider = _serviceProvider.GetRequiredService<IWindowShellProvider>();
        var mainViewModel = mainWindowProvider.Shell.ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;
        if (mainViewModel == null)
            return;

        // Remove from main view if present
        if (mainViewModel.Stopwatches.Contains(stopwatchViewModel))
        {
            mainViewModel.Stopwatches.Remove(stopwatchViewModel);
            OpenedInWindow.Add(stopwatchViewModel.Id);
        }

        await mainWindowProvider.DispatcherQueue.EnqueueAsync(() =>
        {
            try
            {
                var newWindow = new Window();
                newWindow.Title = $"Fluent Stopwatch - {stopwatchViewModel.Name}";
                var newWindowShell = new WindowShell(_serviceProvider, newWindow);
                newWindow.Content = newWindowShell;
                var navigationService = newWindowShell.ServiceProvider.GetRequiredService<INavigationService>();
                navigationService.Navigate<StopwatchWindowViewModel>(stopwatchViewModel.Id);
                newWindow.Closed += (s, e) =>
                {
                    // On close, re-add to main view
                    var mainProvider = _serviceProvider.GetRequiredService<IWindowShellProvider>();
                    var mainVM = mainProvider.Shell.ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;
                    if (mainVM != null && OpenedInWindow.Contains(stopwatchViewModel.Id))
                    {
                        // Only add if not already present
                        if (!mainVM.Stopwatches.Any(sw => sw.Id == stopwatchViewModel.Id))
                        {
                            // Reload from data source to get latest state
                            var ds = _serviceProvider.GetRequiredService<IDataSource>();
                            var appPrefs = _serviceProvider.GetRequiredService<IAppPreferences>();
                            var hist = _serviceProvider.GetRequiredService<IHistoryService>();
                            var conf = _serviceProvider.GetRequiredService<IConfirmationDialogService>();
                            var model = ds.Stopwatches.Get(stopwatchViewModel.Id);
                            if (model != null)
                            {
                                var swVm = new StopwatchViewModel(model, ds, appPrefs, hist, conf);
                                mainVM.Stopwatches.Add(swVm);
                            }
                        }
                        OpenedInWindow.Remove(stopwatchViewModel.Id);
                    }
                };
                newWindow.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening new window: {ex.Message}");
            }
        });
    }

    private static bool IsMultiWindowSupported()
    {
#if HAS_UNO && (__ANDROID__ || __IOS__ || __WASM__)
        return false;
#else
        return true;
#endif
    }
}
