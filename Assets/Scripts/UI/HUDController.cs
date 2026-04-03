using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HUDController : MonoBehaviour
{
    [Header("Date Display")]
    [SerializeField] private TMP_Text dateText;

    [Header("Budget Display")]
    [SerializeField] private TMP_Text budgetText;
    [SerializeField] private TMP_Text incomeText;

    [Header("Time Controls")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private TMP_Text speedLabel;

    [Header("Panels")]
    [SerializeField] private Button projectsButton;
    [SerializeField] private Button objectivesButton;

    [Header("Button Opacity")]
    [SerializeField, Range(0f, 1f)] private float inactiveAlpha = 0.65f;
    [SerializeField, Range(0f, 1f)] private float activeAlpha = 1f;

    private Image _pauseButtonImage;
    private Image _playButtonImage;
    private Image _speedButtonImage;

    private void Awake()
    {
        if (pauseButton != null) _pauseButtonImage = pauseButton.GetComponent<Image>();
        if (playButton != null) _playButtonImage = playButton.GetComponent<Image>();
        if (speedButton != null) _speedButtonImage = speedButton.GetComponent<Image>();
    }

    private void OnEnable()
    {
        TimeSystem.OnMonthTick += HandleMonthTick;
        TimeSystem.OnSpeedChanged += HandleSpeedChanged;
        BudgetSystem.OnBudgetChanged += HandleBudgetChanged;
        GameManager.OnStateChanged += HandleGameStateChanged;

        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (speedButton != null) speedButton.onClick.AddListener(OnSpeedClicked);
        if (projectsButton != null) projectsButton.onClick.AddListener(OnProjectsClicked);
        if (objectivesButton != null) objectivesButton.onClick.AddListener(OnObjectivesClicked);
    }

    private void OnDisable()
    {
        TimeSystem.OnMonthTick -= HandleMonthTick;
        TimeSystem.OnSpeedChanged -= HandleSpeedChanged;
        BudgetSystem.OnBudgetChanged -= HandleBudgetChanged;
        GameManager.OnStateChanged -= HandleGameStateChanged;

        if (pauseButton != null) pauseButton.onClick.RemoveListener(OnPauseClicked);
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
        if (speedButton != null) speedButton.onClick.RemoveListener(OnSpeedClicked);
        if (projectsButton != null) projectsButton.onClick.RemoveListener(OnProjectsClicked);
        if (objectivesButton != null) objectivesButton.onClick.RemoveListener(OnObjectivesClicked);
    }

    private void Start()
    {
        RefreshAll();
    }

    private void Update()
    {
        UpdateDateDisplay();
    }

    private void RefreshAll()
    {
        UpdateDateDisplay();
        UpdateBudgetDisplay(BudgetSystem.CurrentBudget);
        UpdateIncomeDisplay();
        UpdateSpeedButtonLabel();
        UpdateTimeControlVisuals();
    }

    private void HandleMonthTick(int year, int month)
    {
        UpdateDateDisplay();
        UpdateIncomeDisplay();
    }

    private void HandleSpeedChanged(TimeSpeed speed)
    {
        UpdateSpeedButtonLabel();
        UpdateTimeControlVisuals();
    }

    private void HandleBudgetChanged(int newBudget)
    {
        UpdateBudgetDisplay(newBudget);
    }

    private void HandleGameStateChanged(GameState previous, GameState current)
    {
        UpdateTimeControlVisuals();
    }

    private void UpdateDateDisplay()
    {
        if (dateText != null)
        {
            dateText.text = TimeSystem.CurrentDateFormatted;
        }
    }

    private void UpdateBudgetDisplay(int budget)
    {
        if (budgetText != null)
        {
            budgetText.text = BudgetSystem.FormatBudget(budget);
        }
    }

    private void UpdateIncomeDisplay()
    {
        if (incomeText != null)
        {
            int income = BudgetSystem.MonthlyIncome;
            incomeText.text = income > 0 ? $"+{BudgetSystem.FormatBudget(income)}/мес" : "";
        }
    }

    private void UpdateSpeedButtonLabel()
    {
        if (speedLabel == null)
        {
            return;
        }

        TimeSpeed speed = TimeSystem.CurrentSpeed;
        speedLabel.text = speed switch
        {
            TimeSpeed.Fast => "x2",
            TimeSpeed.VeryFast => "x4",
            _ => ""
        };
    }

    private void UpdateTimeControlVisuals()
    {
        bool isPaused = GameManager.CurrentState == GameState.Paused
            || TimeSystem.CurrentSpeed == TimeSpeed.Paused;

        TimeSpeed speed = TimeSystem.CurrentSpeed;

        SetImageAlpha(_pauseButtonImage, isPaused ? activeAlpha : inactiveAlpha);
        SetImageAlpha(_playButtonImage, !isPaused && speed == TimeSpeed.Normal ? activeAlpha : inactiveAlpha);
        SetImageAlpha(_speedButtonImage, speed >= TimeSpeed.Fast ? activeAlpha : inactiveAlpha);
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private void OnPauseClicked()
    {
        GameManager.Pause();
    }

    private void OnPlayClicked()
    {
        if (GameManager.CurrentState == GameState.Paused)
        {
            GameManager.Resume();
        }

        TimeSystem.SetSpeed(TimeSpeed.Normal);
    }

    private void OnSpeedClicked()
    {
        if (GameManager.CurrentState == GameState.Paused)
        {
            GameManager.Resume();
        }

        TimeSystem.CycleSpeed();
    }

    private void OnProjectsClicked()
    {
        if (ObjectivesPanel.TryGetGlobal(out ObjectivesPanel objectivesPanel))
        {
            objectivesPanel.Hide();
        }

        if (ProjectListPanel.TryGetGlobal(out ProjectListPanel browserPanel))
        {
            browserPanel.ToggleGlobal();
        }
    }

    private void OnObjectivesClicked()
    {
        if (ProjectListPanel.TryGetGlobal(out ProjectListPanel browserPanel))
        {
            browserPanel.Hide();
        }

        if (ObjectivesPanel.TryGetGlobal(out ObjectivesPanel objectivesPanel))
        {
            objectivesPanel.Toggle();
        }
    }
}
