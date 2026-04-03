using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BudgetSystem : MonoBehaviour
{
    private static BudgetSystem _instance;

    [Header("Initial Settings")]
    [SerializeField] private int startingBudget = 100000;
    [SerializeField] private int monthlyIncome = 5000;

    private int _currentBudget;

    public static event Action<int> OnBudgetChanged;
    public static event Action<int> OnIncomeReceived;
    public static event Action<int, string> OnExpense;

    public static int CurrentBudget => _instance != null ? _instance._currentBudget : 0;
    public static int MonthlyIncome => _instance != null ? _instance.monthlyIncome : 0;

    private void OnEnable()
    {
        _instance = this;
        TimeSystem.OnMonthTick += HandleMonthTick;
    }

    private void OnDestroy()
    {
        TimeSystem.OnMonthTick -= HandleMonthTick;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        _currentBudget = startingBudget;
        OnBudgetChanged?.Invoke(_currentBudget);
    }

    public static bool TryGetInstance(out BudgetSystem budget)
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<BudgetSystem>(FindObjectsInactive.Include);
        }

        budget = _instance;
        return budget != null;
    }

    public static bool CanAfford(int amount)
    {
        return _instance != null && _instance._currentBudget >= amount;
    }

    public static bool TrySpend(int amount, string reason = null)
    {
        if (_instance == null || amount < 0)
        {
            return false;
        }

        if (_instance._currentBudget < amount)
        {
            return false;
        }

        _instance._currentBudget -= amount;
        OnExpense?.Invoke(amount, reason);
        OnBudgetChanged?.Invoke(_instance._currentBudget);
        return true;
    }

    public static void AddFunds(int amount, string reason = null)
    {
        if (_instance == null || amount <= 0)
        {
            return;
        }

        _instance._currentBudget += amount;
        OnBudgetChanged?.Invoke(_instance._currentBudget);
    }

    public static void SetMonthlyIncome(int income)
    {
        if (_instance != null)
        {
            _instance.monthlyIncome = Mathf.Max(0, income);
        }
    }

    public static string FormatBudget(int amount)
    {
        if (Mathf.Abs(amount) >= 1000000)
        {
            float millions = amount / 1000000f;
            return millions.ToString("0.#") + " млн";
        }

        if (Mathf.Abs(amount) >= 1000)
        {
            float thousands = amount / 1000f;
            return thousands.ToString("0.#") + " тыс";
        }

        return amount.ToString();
    }

    private void HandleMonthTick(int year, int month)
    {
        if (monthlyIncome <= 0)
        {
            return;
        }

        _currentBudget += monthlyIncome;
        OnIncomeReceived?.Invoke(monthlyIncome);
        OnBudgetChanged?.Invoke(_currentBudget);
    }
}
