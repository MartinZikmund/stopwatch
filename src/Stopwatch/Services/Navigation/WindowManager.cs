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
    private static readonly HashSet<int> OpenedInWindow = new();
    private static readonly List<Window> SecondaryWindows = new();
    private static bool _mainWindowClosedSubscribed = false;

    public static IReadOnlyCollection<int> GetOpenedInWindowIds() => OpenedInWindow;

    public WindowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        SubscribeToMainWindowClosed();
    }

    private void SubscribeToMainWindowClosed()
    {
        if (_mainWindowClosedSubscribed) return;
        var mainWindowProvider = _serviceProvider.GetRequiredService<IWindowShellProvider>();
        var mainWindow = mainWindowProvider.Window;
        mainWindow.Closed += (s, e) =>
        {
            lock (SecondaryWindows)
            {
                foreach (var win in SecondaryWindows.ToList())
                {
                    try { win.Close(); } catch { }
                }
                SecondaryWindows.Clear();
            }
        };
        _mainWindowClosedSubscribed = true;
    }

    public async Task OpenStopwatchInNewWindowAsync(StopwatchViewModel stopwatchViewModel)
    {
        if (!IsMultiWindowSupported())
            return;

        var mainWindowProvider = _serviceProvider.GetRequiredService<IWindowShellProvider>();
        var mainViewModel = mainWindowProvider.Shell.ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;
        if (mainViewModel == null)
            return;

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
                lock (SecondaryWindows)
                {
                    SecondaryWindows.Add(newWindow);
                }
                newWindow.Title = $"Fluent Stopwatch - {stopwatchViewModel.Name}";
                var newWindowShell = new WindowShell(_serviceProvider, newWindow);
                newWindow.Content = newWindowShell;
                var navigationService = newWindowShell.ServiceProvider.GetRequiredService<INavigationService>();
                navigationService.Navigate<StopwatchWindowViewModel>(stopwatchViewModel.Id);
                newWindow.Closed += (s, e) =>
                {
                    lock (SecondaryWindows)
                    {
                        SecondaryWindows.Remove(newWindow);
                    }
                    var mainProvider = _serviceProvider.GetRequiredService<IWindowShellProvider>();
                    var mainVM = mainProvider.Shell.ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;
                    if (mainVM != null && OpenedInWindow.Contains(stopwatchViewModel.Id))
                    {
                        if (!mainVM.Stopwatches.Any(sw => sw.Id == stopwatchViewModel.Id))
                        {
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
