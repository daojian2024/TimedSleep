using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using TimedSleep.Core;

namespace TimedSleep.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly PowerTaskScheduler _scheduler;
    private readonly IAppSettingsService _settingsService;
    private readonly DispatcherTimer? _timer;
    private TriggerMode _selectedTriggerMode;
    private PowerAction _selectedPowerAction;
    private int _countdownMinutes;
    private string _clockTime = "23:30";
    private bool _isRunning;
    private string _statusText = "未开始";
    private string _remainingText = "00:00:00";

    public MainViewModel(PowerTaskScheduler scheduler, IAppSettingsService settingsService, bool startUiTimer = false)
    {
        _scheduler = scheduler;
        _settingsService = settingsService;

        var settings = _settingsService.Load();
        _selectedTriggerMode = settings.LastTriggerMode;
        _selectedPowerAction = settings.LastPowerAction;
        _countdownMinutes = settings.LastCountdownMinutes;
        _clockTime = settings.LastClockTime;

        SetQuickMinutesCommand = new RelayCommand(SetQuickMinutes, _ => !IsRunning);
        StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
        CancelCommand = new RelayCommand(_ => Cancel(), _ => IsRunning);

        if (startUiTimer)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (_, _) => await TickAsync();
            _timer.Start();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<TriggerMode> TriggerModes { get; } = new[] { TriggerMode.Countdown, TriggerMode.ClockTime };

    public IReadOnlyList<PowerAction> PowerActions { get; } = new[] { PowerAction.Hibernate, PowerAction.Shutdown };

    public ICommand SetQuickMinutesCommand { get; }

    public ICommand StartCommand { get; }

    public ICommand CancelCommand { get; }

    public TriggerMode SelectedTriggerMode
    {
        get => _selectedTriggerMode;
        set
        {
            if (SetField(ref _selectedTriggerMode, value))
            {
                OnPropertyChanged(nameof(IsCountdownMode));
                OnPropertyChanged(nameof(IsClockTimeMode));
            }
        }
    }

    public PowerAction SelectedPowerAction
    {
        get => _selectedPowerAction;
        set => SetField(ref _selectedPowerAction, value);
    }

    public int CountdownMinutes
    {
        get => _countdownMinutes;
        set => SetField(ref _countdownMinutes, value);
    }

    public string ClockTime
    {
        get => _clockTime;
        set => SetField(ref _clockTime, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetField(ref _isRunning, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                RaiseCommandStates();
            }
        }
    }

    public bool CanEdit => !IsRunning;

    public bool IsCountdownMode => SelectedTriggerMode == TriggerMode.Countdown;

    public bool IsClockTimeMode => SelectedTriggerMode == TriggerMode.ClockTime;

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public string RemainingText
    {
        get => _remainingText;
        private set => SetField(ref _remainingText, value);
    }

    public async Task TickAsync(DateTime? now = null)
    {
        var current = now ?? DateTime.Now;
        var result = await _scheduler.ExecuteIfDueAsync(current);
        if (result is not null)
        {
            IsRunning = false;
            StatusText = result.IsSuccess ? "任务已执行" : $"执行失败：{result.ErrorMessage}";
            RemainingText = "00:00:00";
            return;
        }

        if (IsRunning)
        {
            UpdateRemaining(current);
        }
    }

    private void SetQuickMinutes(object? parameter)
    {
        if (parameter is int minutes)
        {
            CountdownMinutes = minutes;
        }
        else if (parameter is string text && int.TryParse(text, out var parsed))
        {
            CountdownMinutes = parsed;
        }
    }

    private void Start()
    {
        var result = _scheduler.TryCreateTargetTime(SelectedTriggerMode, CountdownMinutes, ClockTime, out var targetTime);
        if (!result.IsSuccess)
        {
            StatusText = result.ErrorMessage;
            return;
        }

        _settingsService.Save(new AppSettings
        {
            LastTriggerMode = SelectedTriggerMode,
            LastPowerAction = SelectedPowerAction,
            LastCountdownMinutes = CountdownMinutes,
            LastClockTime = ClockTime
        });

        _scheduler.Start(SelectedPowerAction, targetTime);
        IsRunning = true;
        UpdateRemaining(DateTime.Now);
    }

    private void Cancel()
    {
        _scheduler.Cancel();
        IsRunning = false;
        RemainingText = "00:00:00";
        StatusText = "未开始";
    }

    private void UpdateRemaining(DateTime now)
    {
        var remaining = _scheduler.GetRemaining(now);
        RemainingText = $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
        StatusText = $"将在 {RemainingText} 后{GetActionText(SelectedPowerAction)}";
    }

    private static string GetActionText(PowerAction action)
    {
        return action == PowerAction.Hibernate ? "休眠" : "关机";
    }

    private void RaiseCommandStates()
    {
        ((RelayCommand)SetQuickMinutesCommand).RaiseCanExecuteChanged();
        ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
        ((RelayCommand)CancelCommand).RaiseCanExecuteChanged();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
