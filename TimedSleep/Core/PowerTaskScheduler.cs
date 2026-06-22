using System.Globalization;

namespace TimedSleep.Core;

public sealed class PowerTaskScheduler
{
    private readonly IPowerService _powerService;
    private readonly Func<DateTime> _nowProvider;
    private PowerAction _action;

    public PowerTaskScheduler(IPowerService powerService, Func<DateTime>? nowProvider = null)
    {
        _powerService = powerService;
        _nowProvider = nowProvider ?? (() => DateTime.Now);
    }

    public bool IsRunning { get; private set; }

    public DateTime? TargetTime { get; private set; }

    public OperationResult TryCreateTargetTime(
        TriggerMode triggerMode,
        int countdownMinutes,
        string clockTime,
        out DateTime targetTime)
    {
        var now = _nowProvider();

        if (triggerMode == TriggerMode.Countdown)
        {
            if (countdownMinutes is < 1 or > 1440)
            {
                targetTime = default;
                return OperationResult.Failure("倒计时分钟数必须在 1 到 1440 之间。");
            }

            targetTime = now.AddMinutes(countdownMinutes);
            return OperationResult.Success();
        }

        if (!TimeSpan.TryParseExact(clockTime, @"hh\:mm", CultureInfo.InvariantCulture, out var parsedTime))
        {
            targetTime = default;
            return OperationResult.Failure("指定时间必须使用 24 小时格式，例如 23:30。");
        }

        targetTime = now.Date.Add(parsedTime);
        if (targetTime <= now)
        {
            targetTime = targetTime.AddDays(1);
        }

        return OperationResult.Success();
    }

    public void Start(PowerAction action, DateTime targetTime)
    {
        _action = action;
        TargetTime = targetTime;
        IsRunning = true;
    }

    public void Cancel()
    {
        TargetTime = null;
        IsRunning = false;
    }

    public TimeSpan GetRemaining(DateTime now)
    {
        if (!IsRunning || TargetTime is null)
        {
            return TimeSpan.Zero;
        }

        var remaining = TargetTime.Value - now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public async Task<PowerCommandResult?> ExecuteIfDueAsync(DateTime now)
    {
        if (!IsRunning || TargetTime is null || now < TargetTime.Value)
        {
            return null;
        }

        IsRunning = false;
        TargetTime = null;
        return await _powerService.ExecuteAsync(_action);
    }
}
