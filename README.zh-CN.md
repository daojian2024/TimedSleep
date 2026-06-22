# TimedSleep

TimedSleep 是一个 Windows 桌面小工具，用来快速设置电脑在倒计时结束后，或在指定时间点执行休眠或关机。

## 功能

- 倒计时模式：支持 15、30、60、90 分钟快捷设置，也可以自定义分钟数。
- 指定时间模式：支持设置具体时间，例如 `23:30`。
- 执行动作：支持休眠和关机。
- 实时显示剩余时间。
- 支持最小化到系统托盘。
- 点击窗口关闭按钮会直接退出程序。
- 自动保存最近一次使用的触发方式、执行动作和时间设置。

## 技术栈

- C#
- WPF
- .NET 8
- xUnit

## 开发环境

- Windows
- .NET 8 SDK

## 构建

```powershell
dotnet build TimedSleep.sln
```

## 测试

```powershell
dotnet test TimedSleep.Tests\TimedSleep.Tests.csproj
```

## 发布

```powershell
dotnet publish TimedSleep\TimedSleep.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

发布后的文件位于：

```text
TimedSleep\bin\Release\net8.0-windows\win-x64\publish\
```

普通使用只需要复制其中的 `TimedSleep.exe`。

## 注意事项

- 休眠会先执行 `powercfg /hibernate on`，然后执行 `shutdown /h`。
- 关机会执行 `shutdown /s /t 0`。
- 自动化测试使用假的电源服务，不会真的让电脑休眠或关机。
- 该项目只面向 Windows。

## 目录说明

```text
TimedSleep/              主程序源码
TimedSleep.Tests/        自动化测试
TimedSleep.sln           解决方案文件
README.md                英文说明
README.zh-CN.md          中文说明
.gitignore               Git 忽略规则
```
