using TimedSleep.Core;

namespace TimedSleep.Tests;

public sealed class PowerTaskSchedulerTests
{
    [Fact]
    public void CountdownMinutesBelowOneIsInvalid()
    {
        var scheduler = new PowerTaskScheduler(new FakePowerService(), () => new DateTime(2026, 6, 22, 21, 0, 0));

        var result = scheduler.TryCreateTargetTime(TriggerMode.Countdown, 0, "23:30", out _);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CountdownMinutesAboveOneDayIsInvalid()
    {
        var scheduler = new PowerTaskScheduler(new FakePowerService(), () => new DateTime(2026, 6, 22, 21, 0, 0));

        var result = scheduler.TryCreateTargetTime(TriggerMode.Countdown, 1441, "23:30", out _);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ClockTimeWithInvalidFormatIsInvalid()
    {
        var scheduler = new PowerTaskScheduler(new FakePowerService(), () => new DateTime(2026, 6, 22, 21, 0, 0));

        var result = scheduler.TryCreateTargetTime(TriggerMode.ClockTime, 30, "bad", out _);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ClockTimeLaterThanNowTargetsToday()
    {
        var now = new DateTime(2026, 6, 22, 21, 0, 0);
        var scheduler = new PowerTaskScheduler(new FakePowerService(), () => now);

        var result = scheduler.TryCreateTargetTime(TriggerMode.ClockTime, 30, "23:30", out var targetTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2026, 6, 22, 23, 30, 0), targetTime);
    }

    [Fact]
    public void ClockTimeEarlierThanNowTargetsTomorrow()
    {
        var now = new DateTime(2026, 6, 22, 21, 0, 0);
        var scheduler = new PowerTaskScheduler(new FakePowerService(), () => now);

        var result = scheduler.TryCreateTargetTime(TriggerMode.ClockTime, 30, "20:30", out var targetTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2026, 6, 23, 20, 30, 0), targetTime);
    }

    [Fact]
    public async Task DueTaskInvokesSelectedHibernateAction()
    {
        var now = new DateTime(2026, 6, 22, 21, 0, 0);
        var powerService = new FakePowerService();
        var scheduler = new PowerTaskScheduler(powerService, () => now);

        scheduler.Start(PowerAction.Hibernate, now.AddSeconds(1));
        await scheduler.ExecuteIfDueAsync(now.AddSeconds(1));

        Assert.Equal(PowerAction.Hibernate, powerService.LastAction);
        Assert.False(scheduler.IsRunning);
    }

    [Fact]
    public async Task DueTaskInvokesSelectedShutdownAction()
    {
        var now = new DateTime(2026, 6, 22, 21, 0, 0);
        var powerService = new FakePowerService();
        var scheduler = new PowerTaskScheduler(powerService, () => now);

        scheduler.Start(PowerAction.Shutdown, now.AddSeconds(1));
        await scheduler.ExecuteIfDueAsync(now.AddSeconds(1));

        Assert.Equal(PowerAction.Shutdown, powerService.LastAction);
        Assert.False(scheduler.IsRunning);
    }

    [Fact]
    public void CancelStopsRunningTask()
    {
        var now = new DateTime(2026, 6, 22, 21, 0, 0);
        var scheduler = new PowerTaskScheduler(new FakePowerService(), () => now);

        scheduler.Start(PowerAction.Hibernate, now.AddMinutes(15));
        scheduler.Cancel();

        Assert.False(scheduler.IsRunning);
        Assert.Null(scheduler.TargetTime);
    }

    private sealed class FakePowerService : IPowerService
    {
        public PowerAction? LastAction { get; private set; }

        public Task<PowerCommandResult> ExecuteAsync(PowerAction action)
        {
            LastAction = action;
            return Task.FromResult(PowerCommandResult.Success());
        }
    }
}
