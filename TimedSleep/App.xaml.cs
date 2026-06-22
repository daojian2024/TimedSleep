using System.Windows;
using TimedSleep.Core;
using TimedSleep.ViewModels;

namespace TimedSleep;

public partial class App : System.Windows.Application
{
    private TrayService? _trayService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var powerService = new SystemPowerService();
        var scheduler = new PowerTaskScheduler(powerService);
        var settingsService = new AppSettingsService();
        var viewModel = new MainViewModel(scheduler, settingsService, startUiTimer: true);
        var mainWindow = new MainWindow(viewModel);

        _trayService = new TrayService(mainWindow);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        base.OnExit(e);
    }
}
