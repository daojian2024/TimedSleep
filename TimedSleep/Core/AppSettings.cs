namespace TimedSleep.Core;

public sealed class AppSettings
{
    public TriggerMode LastTriggerMode { get; set; } = TriggerMode.Countdown;

    public PowerAction LastPowerAction { get; set; } = PowerAction.Hibernate;

    public int LastCountdownMinutes { get; set; } = 30;

    public string LastClockTime { get; set; } = "23:30";
}
