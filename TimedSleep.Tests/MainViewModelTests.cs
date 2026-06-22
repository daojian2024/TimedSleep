using TimedSleep.Core;
using TimedSleep.ViewModels;

namespace TimedSleep.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public void QuickMinuteButtonUpdatesCountdownMinutes()
    {
        var viewModel = CreateViewModel();

        viewModel.SetQuickMinutesCommand.Execute(90);

        Assert.Equal(90, viewModel.CountdownMinutes);
    }

    [Fact]
    public void StartMovesViewModelIntoRunningState()
    {
        var viewModel = CreateViewModel();
        viewModel.CountdownMinutes = 15;

        viewModel.StartCommand.Execute(null);

        Assert.True(viewModel.IsRunning);
        Assert.Contains("将在", viewModel.StatusText);
    }

    [Fact]
    public void CancelMovesViewModelBackToIdleState()
    {
        var viewModel = CreateViewModel();
        viewModel.StartCommand.Execute(null);

        viewModel.CancelCommand.Execute(null);

        Assert.False(viewModel.IsRunning);
        Assert.Equal("未开始", viewModel.StatusText);
    }

    private static MainViewModel CreateViewModel()
    {
        var now = new DateTime(2026, 6, 22, 21, 0, 0);
        var settings = new AppSettings
        {
            LastTriggerMode = TriggerMode.Countdown,
            LastPowerAction = PowerAction.Hibernate,
            LastCountdownMinutes = 30,
            LastClockTime = "23:30"
        };

        return new MainViewModel(
            new PowerTaskScheduler(new FakePowerService(), () => now),
            new FakeSettingsService(settings));
    }

    private sealed class FakePowerService : IPowerService
    {
        public Task<PowerCommandResult> ExecuteAsync(PowerAction action)
        {
            return Task.FromResult(PowerCommandResult.Success());
        }
    }

    private sealed class FakeSettingsService : IAppSettingsService
    {
        private AppSettings _settings;

        public FakeSettingsService(AppSettings settings)
        {
            _settings = settings;
        }

        public AppSettings Load()
        {
            return _settings;
        }

        public void Save(AppSettings settings)
        {
            _settings = settings;
        }
    }
}
