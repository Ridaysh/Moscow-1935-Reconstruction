using System;
using UnityEngine;

public enum TimeSpeed
{
    Paused = 0,
    Normal = 1,
    Fast = 2,
    VeryFast = 3
}

[DisallowMultipleComponent]
public class TimeSystem : MonoBehaviour
{
    private static TimeSystem _instance;

    [Header("Start Date")]
    [SerializeField] private int startYear = 1935;
    [SerializeField, Range(1, 12)] private int startMonth = 1;

    [Header("Speed Settings")]
    [SerializeField] private float secondsPerMonth = 5f;
    [SerializeField] private TimeSpeed initialSpeed = TimeSpeed.Normal;

    private int _currentYear;
    private int _currentMonth;
    private TimeSpeed _currentSpeed;
    private float _monthTimer;

    public static event Action<int, int> OnMonthTick;
    public static event Action<int> OnYearTick;
    public static event Action<TimeSpeed> OnSpeedChanged;

    public static int CurrentYear => _instance != null ? _instance._currentYear : 1935;
    public static int CurrentMonth => _instance != null ? _instance._currentMonth : 1;
    public static int CurrentDay => _instance != null ? _instance.GetCurrentDay() : 1;
    public static TimeSpeed CurrentSpeed => _instance != null ? _instance._currentSpeed : TimeSpeed.Paused;

    public static string CurrentDateFormatted
    {
        get
        {
            if (_instance == null) return "1 Январь 1935";
            return $"{_instance.GetCurrentDay()} {GetMonthName(_instance._currentMonth)} {_instance._currentYear}";
        }
    }

    private void OnEnable()
    {
        _instance = this;
        GameManager.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= HandleGameStateChanged;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        _currentYear = startYear;
        _currentMonth = startMonth;
        _monthTimer = 0f;
        SetSpeed(initialSpeed);
        OnMonthTick?.Invoke(_currentYear, _currentMonth);
    }

    private void Update()
    {
        if (!GameManager.IsPlaying || _currentSpeed == TimeSpeed.Paused)
        {
            return;
        }

        float speedMultiplier = GetSpeedMultiplier(_currentSpeed);
        _monthTimer += Time.deltaTime * speedMultiplier;

        if (_monthTimer >= secondsPerMonth)
        {
            _monthTimer -= secondsPerMonth;
            AdvanceMonth();
        }
    }

    public static void SetSpeed(TimeSpeed speed)
    {
        if (_instance == null)
        {
            return;
        }

        if (_instance._currentSpeed == speed)
        {
            return;
        }

        _instance._currentSpeed = speed;
        OnSpeedChanged?.Invoke(speed);
    }

    public static void CycleSpeed()
    {
        if (_instance == null)
        {
            return;
        }

        TimeSpeed next = _instance._currentSpeed switch
        {
            TimeSpeed.Normal => TimeSpeed.Fast,
            TimeSpeed.Fast => TimeSpeed.VeryFast,
            TimeSpeed.VeryFast => TimeSpeed.Normal,
            _ => TimeSpeed.Normal
        };

        SetSpeed(next);
    }

    public static float GetMonthProgress()
    {
        if (_instance == null || _instance.secondsPerMonth <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(_instance._monthTimer / _instance.secondsPerMonth);
    }

    private int GetCurrentDay()
    {
        int daysInMonth = GetDaysInMonthValue(_currentMonth, _currentYear);
        float progress = secondsPerMonth > 0f ? Mathf.Clamp01(_monthTimer / secondsPerMonth) : 0f;
        return Mathf.Clamp(1 + Mathf.FloorToInt(progress * daysInMonth), 1, daysInMonth);
    }

    public static int GetDaysInMonthValue(int month, int year)
    {
        return month switch
        {
            1 => 31,
            2 => (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? 29 : 28,
            3 => 31,
            4 => 30,
            5 => 31,
            6 => 30,
            7 => 31,
            8 => 31,
            9 => 30,
            10 => 31,
            11 => 30,
            12 => 31,
            _ => 30
        };
    }

    public static int GetRemainingDaysInCurrentMonth(bool includeCurrentDay = true)
    {
        if (_instance == null)
        {
            return 0;
        }

        int daysInMonth = GetDaysInMonthValue(_instance._currentMonth, _instance._currentYear);
        int daysRemaining = daysInMonth - CurrentDay + (includeCurrentDay ? 1 : 0);
        return Mathf.Max(0, daysRemaining);
    }

    private void AdvanceMonth()
    {
        _currentMonth++;

        if (_currentMonth > 12)
        {
            _currentMonth = 1;
            _currentYear++;
            OnYearTick?.Invoke(_currentYear);
        }

        OnMonthTick?.Invoke(_currentYear, _currentMonth);
    }

    private void HandleGameStateChanged(GameState previous, GameState current)
    {
        if (current == GameState.Paused)
        {
            _currentSpeed = TimeSpeed.Paused;
            OnSpeedChanged?.Invoke(_currentSpeed);
        }
        else if (current == GameState.Playing && previous == GameState.Paused)
        {
            SetSpeed(TimeSpeed.Normal);
        }
    }

    private static float GetSpeedMultiplier(TimeSpeed speed)
    {
        return speed switch
        {
            TimeSpeed.Normal => 1f,
            TimeSpeed.Fast => 2f,
            TimeSpeed.VeryFast => 4f,
            _ => 0f
        };
    }

    private static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "Январь",
            2 => "Февраль",
            3 => "Март",
            4 => "Апрель",
            5 => "Май",
            6 => "Июнь",
            7 => "Июль",
            8 => "Август",
            9 => "Сентябрь",
            10 => "Октябрь",
            11 => "Ноябрь",
            12 => "Декабрь",
            _ => "???"
        };
    }
}
