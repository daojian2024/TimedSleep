using System.Windows;
using Forms = System.Windows.Forms;

namespace TimedSleep;

public sealed class TrayService : IDisposable
{
    private readonly Window _window;
    private readonly Forms.NotifyIcon _notifyIcon;

    public TrayService(Window window)
    {
        _window = window;
        _window.StateChanged += OnWindowStateChanged;

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示", null, (_, _) => ShowWindow());
        menu.Items.Add("退出", null, (_, _) => ExitApplication());

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "TimedSleep",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => ShowWindow();
    }

    public void Dispose()
    {
        _window.StateChanged -= OnWindowStateChanged;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (_window.WindowState == WindowState.Minimized)
        {
            _window.Hide();
        }
    }

    private void ShowWindow()
    {
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void ExitApplication()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
