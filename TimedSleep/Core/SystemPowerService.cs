using System.Diagnostics;

namespace TimedSleep.Core;

public sealed class SystemPowerService : IPowerService
{
    public async Task<PowerCommandResult> ExecuteAsync(PowerAction action)
    {
        return action switch
        {
            PowerAction.Hibernate => await HibernateAsync(),
            PowerAction.Shutdown => await RunCommandAsync("shutdown", "/s /t 0"),
            _ => PowerCommandResult.Failure("未知电源动作。")
        };
    }

    private static async Task<PowerCommandResult> HibernateAsync()
    {
        var enableResult = await RunCommandAsync("powercfg", "/hibernate on");
        if (!enableResult.IsSuccess)
        {
            return enableResult;
        }

        return await RunCommandAsync("shutdown", "/h");
    }

    private static async Task<PowerCommandResult> RunCommandAsync(string fileName, string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            process.Start();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0
                ? PowerCommandResult.Success()
                : PowerCommandResult.Failure(string.IsNullOrWhiteSpace(error) ? $"命令 {fileName} {arguments} 执行失败。" : error.Trim());
        }
        catch (Exception ex)
        {
            return PowerCommandResult.Failure(ex.Message);
        }
    }
}
