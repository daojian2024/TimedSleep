namespace TimedSleep.Core;

public interface IPowerService
{
    Task<PowerCommandResult> ExecuteAsync(PowerAction action);
}
