namespace TimedSleep.Core;

public interface IAppSettingsService
{
    AppSettings Load();

    void Save(AppSettings settings);
}
