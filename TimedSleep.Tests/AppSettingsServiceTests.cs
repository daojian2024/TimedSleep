using TimedSleep.Core;

namespace TimedSleep.Tests;

public sealed class AppSettingsServiceTests
{
    [Fact]
    public void SettingsRoundTripPreservesRecentChoices()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"timedsleep-{Guid.NewGuid():N}.json");
        var service = new AppSettingsService(filePath);
        var settings = new AppSettings
        {
            LastTriggerMode = TriggerMode.ClockTime,
            LastPowerAction = PowerAction.Shutdown,
            LastCountdownMinutes = 45,
            LastClockTime = "23:30"
        };

        service.Save(settings);
        var loaded = service.Load();

        Assert.Equal(TriggerMode.ClockTime, loaded.LastTriggerMode);
        Assert.Equal(PowerAction.Shutdown, loaded.LastPowerAction);
        Assert.Equal(45, loaded.LastCountdownMinutes);
        Assert.Equal("23:30", loaded.LastClockTime);

        File.Delete(filePath);
    }
}
