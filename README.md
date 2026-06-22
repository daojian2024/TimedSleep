# TimedSleep

TimedSleep is a small Windows desktop tool for scheduling hibernate or shutdown tasks.

## Features

- Countdown mode: run after 15, 30, 60, 90 minutes, or a custom number of minutes.
- Clock-time mode: run at a specific time such as `23:30`.
- Power actions: hibernate or shutdown.
- Live remaining-time display.
- Minimize to system tray.
- Close button exits the app.
- Saves the last selected mode, action, and time settings.

## Tech Stack

- C#
- WPF
- .NET 8
- xUnit tests

## Requirements

- Windows
- .NET 8 SDK for development

## Build

```powershell
dotnet build TimedSleep.sln
```

## Test

```powershell
dotnet test TimedSleep.Tests\TimedSleep.Tests.csproj
```

## Publish

```powershell
dotnet publish TimedSleep\TimedSleep.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

The published executable will be generated under:

```text
TimedSleep\bin\Release\net8.0-windows\win-x64\publish\
```

For normal use, `TimedSleep.exe` is enough.

## Notes

- Hibernate uses `powercfg /hibernate on` followed by `shutdown /h`.
- Shutdown uses `shutdown /s /t 0`.
- Tests use fake power services and do not actually hibernate or shut down the computer.
