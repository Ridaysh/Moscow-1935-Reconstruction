using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class BuildConfirmationPopup : PopupWindow
{
    private static BuildConfirmationPopup _instance;

    [Header("References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private GameZone _pendingZone;
    private ProjectData _pendingProject;

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }

    private void OnEnable()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HandleConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Hide);
        }
    }

    private void OnDisable()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(Hide);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static void Show(GameZone zone, ProjectData project)
    {
        if (zone == null || project == null)
        {
            return;
        }

        if (!TryGetInstance(out BuildConfirmationPopup popup))
        {
            Debug.LogWarning($"{nameof(BuildConfirmationPopup)} is not present in the scene.");
            return;
        }

        popup.Bind(zone, project);
    }

    private static bool TryGetInstance(out BuildConfirmationPopup popup)
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<BuildConfirmationPopup>(FindObjectsInactive.Include);
        }

        popup = _instance;
        return popup != null;
    }

    private void Bind(GameZone zone, ProjectData project)
    {
        _pendingZone = zone;
        _pendingProject = project;

        bool canStart = ConstructionManager.CanStartProject(project, zone, out string reason);

        if (titleText != null)
        {
            titleText.text = $"Начать проект \"{project.ProjectName}\"?";
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(project.Description)
                ? "Подтвердите запуск строительства."
                : project.Description;
        }

        var duration = $"Срок: {project.DurationMonths} мес.";

        if (costText != null)
        {
            costText.text = $"Стоимость: {BudgetSystem.FormatBudget(project.Cost)}";
            if (durationText == null)
                costText.text += $"\t{duration}";
        }

        if (durationText != null)
        {
            durationText.text = duration;
        }

        if (statusText != null)
        {
            statusText.text = canStart
                ? "После подтверждения бюджет будет списан, а строительство начнется немедленно."
                : reason;
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = canStart;
        }

        Show();
    }

    protected override void OnPopupVisibilityChanged(bool isOpen)
    {
        if (isOpen)
        {
            return;
        }

        _pendingZone = null;
        _pendingProject = null;
    }

    private void HandleConfirmClicked()
    {
        if (_pendingZone == null || _pendingProject == null)
        {
            Hide();
            return;
        }

        if (ConstructionManager.TryStartProject(_pendingProject, _pendingZone))
        {
            Hide();
            return;
        }

        bool canStart = ConstructionManager.CanStartProject(_pendingProject, _pendingZone, out string reason);
        if (statusText != null)
        {
            statusText.text = canStart ? "Не удалось запустить строительство." : reason;
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = canStart;
        }
    }
}
