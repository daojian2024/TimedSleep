namespace TimedSleep.Core;

public enum TriggerMode
{
    Countdown,
    ClockTime
}

public enum PowerAction
{
    Hibernate,
    Shutdown
}

public sealed class OperationResult
{
    private OperationResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string ErrorMessage { get; }

    public static OperationResult Success()
    {
        return new OperationResult(true, string.Empty);
    }

    public static OperationResult Failure(string errorMessage)
    {
        return new OperationResult(false, errorMessage);
    }
}

public sealed class PowerCommandResult
{
    private PowerCommandResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string ErrorMessage { get; }

    public static PowerCommandResult Success()
    {
        return new PowerCommandResult(true, string.Empty);
    }

    public static PowerCommandResult Failure(string errorMessage)
    {
        return new PowerCommandResult(false, errorMessage);
    }
}
