using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum ZoneState
{
    Empty,
    Reserved,
    InProgress,
    UnderConstruction = InProgress,
    Built,
    Blocked
}

[RequireComponent(typeof(SpriteRenderer))]
public class GameZone : ContextMenuTargetBase, IPointerEnterHandler, IPointerExitHandler, IContextCommandReceiver
{
    private const string BuildCommandId = "build";
    private const string AboutCommandId = "about";

    [System.Serializable]
    private sealed class StringEvent : UnityEvent<string>
    {
    }

    [SerializeField, Range(0f, 1f)] private float hoverDarkenAmount = 0.15f;
    [SerializeField] private GameObject buildReplacementPrefab;
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private StringEvent onContextCommand;

    [Header("Project")]
    [SerializeField] private ZoneDefinition zoneDefinition;
    [SerializeField] private ProjectData assignedProject;
    [SerializeField] private Transform builtVisualAnchor;

    [Header("Construction Overlay")]
    [SerializeField] private TextMeshPro remainingDaysLabel;

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;
    private ZoneState _state = ZoneState.Empty;
    private ProjectData _currentProject;
    private ProjectData _builtProject;
    private ConstructionJob _activeJob;
    private GameObject _builtVisualInstance;
    private string _lastRemainingDaysText = string.Empty;

    public ZoneState State => _state;
    public ZoneDefinition Definition => zoneDefinition;
    public ProjectData AssignedProject => zoneDefinition != null && zoneDefinition.DefaultProject != null
        ? zoneDefinition.DefaultProject
        : assignedProject;
    public ProjectData CurrentProject => _currentProject;
    public ProjectData BuiltProject => _builtProject;
    public string DisplayName => zoneDefinition != null ? zoneDefinition.DisplayName : (AssignedProject != null ? AssignedProject.ProjectName : name);

    public static event Action<GameZone> OnZoneClicked;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
        SetRemainingDaysLabelVisible(_state == ZoneState.InProgress);
    }

    private void OnEnable()
    {
        SetRemainingDaysLabelVisible(_state == ZoneState.InProgress);
    }

    private void Update()
    {
        if (_state != ZoneState.InProgress)
        {
            return;
        }

        UpdateRemainingDaysLabel();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHoverColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RestoreBaseColor();
    }

    protected override void OnPrimaryClicked(PointerEventData eventData)
    {
        onClick?.Invoke();

        if (_state == ZoneState.Built)
        {
            ShowInfo();
            return;
        }

        ZoneSelectionController.SelectZone(this);
        OnZoneClicked?.Invoke(this);
        ShowContextMenu(eventData.position);
    }

    public void HandleContextCommand(string commandId)
    {
        onContextCommand?.Invoke(commandId);

        if (!this)
        {
            return;
        }

        if (string.Equals(commandId, AboutCommandId, StringComparison.OrdinalIgnoreCase))
        {
            ShowInfo();
            return;
        }

        if (!string.Equals(commandId, BuildCommandId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (_state != ZoneState.Empty)
        {
            return;
        }

        ProjectData project = AssignedProject;
        if (project == null)
        {
            Debug.LogWarning($"Для зоны '{name}' не назначен проект строительства.", this);
            return;
        }

        BuildConfirmationPopup.Show(this, project);
    }

    public bool CanAccept(ProjectData project)
    {
        if (project == null)
        {
            return false;
        }

        if (zoneDefinition != null)
        {
            return zoneDefinition.Allows(project);
        }

        if (assignedProject != null)
        {
            return assignedProject == project;
        }

        return project.RequiredZoneType == ZoneType.Any;
    }

    public void SetProjectInProgress(ConstructionJob job)
    {
        if (_state != ZoneState.Empty)
        {
            return;
        }

        _activeJob = job;
        _currentProject = job != null ? job.Project : null;
        _state = ZoneState.InProgress;
        UpdateStateVisual();
        UpdateRemainingDaysLabel();
    }

    public void ClearConstruction()
    {
        _activeJob = null;
        _currentProject = null;
        if (_state == ZoneState.InProgress)
        {
            _state = ZoneState.Empty;
        }

        SetRemainingDaysLabelVisible(false);
        UpdateStateVisual();
    }

    public void MarkBuilt(ProjectData project)
    {
        _state = ZoneState.Built;
        _activeJob = null;
        _builtProject = project != null ? project : AssignedProject;
        _currentProject = null;
        SetRemainingDaysLabelVisible(false);
        SpawnBuiltVisual(_builtProject);
        UpdateStateVisual();
    }

    private void OnDisable()
    {
        RestoreBaseColor();
    }

    private void OnValidate()
    {
        hoverDarkenAmount = Mathf.Clamp01(hoverDarkenAmount);
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_spriteRenderer != null)
        {
            _baseColor = _spriteRenderer.color;
        }

        if (!Application.isPlaying)
        {
            SetRemainingDaysLabelVisible(_state == ZoneState.InProgress);
        }
    }

    protected override ContextMenuDefinition GetContextMenuDefinition()
    {
        return _state == ZoneState.InProgress ? null : base.GetContextMenuDefinition();
    }

    protected override bool ShouldOpenContextMenuOnLeftClick()
    {
        return _state != ZoneState.Built;
    }

    private void ApplyHoverColor()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (_state == ZoneState.InProgress)
        {
            return;
        }

        Color hoverColor = GetDisplayColor();
        float brightnessMultiplier = 1f - hoverDarkenAmount;
        hoverColor.r *= brightnessMultiplier;
        hoverColor.g *= brightnessMultiplier;
        hoverColor.b *= brightnessMultiplier;

        _spriteRenderer.color = hoverColor;
    }

    private void RestoreBaseColor()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.color = GetDisplayColor();
    }

    private void UpdateStateVisual()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.enabled = _builtVisualInstance == null;
        _spriteRenderer.color = GetDisplayColor();
    }

    private Color GetDisplayColor()
    {
        Color color = _baseColor;
        if (_state == ZoneState.InProgress)
        {
            color.a *= 0.5f;
        }

        return color;
    }

    private void EnsureRemainingDaysLabel()
    {
        if (remainingDaysLabel == null)
        {
            return;
        }
    }

    private void UpdateRemainingDaysLabel()
    {
        if (_activeJob == null)
        {
            SetRemainingDaysLabelVisible(false);
            return;
        }

        EnsureRemainingDaysLabel();
        if (remainingDaysLabel == null)
        {
            return;
        }

        int daysRemaining = _activeJob.GetRemainingDaysEstimate();
        string remainingDaysText = $"{daysRemaining} {GetDayWord(daysRemaining)}";
        if (_lastRemainingDaysText != remainingDaysText)
        {
            _lastRemainingDaysText = remainingDaysText;
            remainingDaysLabel.text = remainingDaysText;
        }

        SetRemainingDaysLabelVisible(true);
    }

    private void SetRemainingDaysLabelVisible(bool isVisible)
    {
        if (remainingDaysLabel == null)
        {
            return;
        }

        remainingDaysLabel.gameObject.SetActive(isVisible);
        if (!isVisible)
        {
            _lastRemainingDaysText = string.Empty;
        }
    }

    private static string GetDayWord(int days)
    {
        int absDays = Mathf.Abs(days) % 100;
        int lastDigit = absDays % 10;
        if (absDays is >= 11 and <= 14)
        {
            return "дней";
        }

        return lastDigit switch
        {
            1 => "день",
            2 or 3 or 4 => "дня",
            _ => "дней"
        };
    }

    private void SpawnBuiltVisual(ProjectData project)
    {
        if (_builtVisualInstance != null)
        {
            Destroy(_builtVisualInstance);
            _builtVisualInstance = null;
        }

        GameObject prefab = project != null && project.BuiltPrefab != null
            ? project.BuiltPrefab
            : buildReplacementPrefab;

        if (prefab == null)
        {
            return;
        }

        Transform parent = builtVisualAnchor != null ? builtVisualAnchor : transform;
        _builtVisualInstance = Instantiate(prefab, parent);

        Transform builtTransform = _builtVisualInstance.transform;
        builtTransform.localPosition = Vector3.zero;
        builtTransform.localRotation = Quaternion.identity;
        builtTransform.localScale = Vector3.one;

        BuildingInfo info = _builtVisualInstance.GetComponent<BuildingInfo>();
        if (info != null)
        {
            info.Init(project);
        }
    }

    private void ShowInfo()
    {
        ProjectData data = _builtProject != null ? _builtProject : AssignedProject;
        if (data == null || string.IsNullOrEmpty(data.HistoricalInfo))
        {
            return;
        }

        InfoPopup.Show(data);
    }
}
