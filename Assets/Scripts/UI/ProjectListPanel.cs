using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class ProjectListPanel : MonoBehaviour
{
    private enum PanelMode
    {
        Hidden,
        GlobalBrowser
    }

    private static ProjectListPanel _global;

    [Header("References")]
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private ProjectItemView projectItemPrefab;
    [SerializeField] private Button closeButton;

    [Header("Selected Zone Info")]
    [SerializeField] private TMP_Text zoneTitleText;

    private readonly List<GameObject> _spawnedItems = new();
    private readonly List<ProjectData> _availableBuffer = new();
    private CanvasGroup _canvasGroup;
    private UIWindowAnimator _windowAnimator;
    private PanelMode _mode = PanelMode.Hidden;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _windowAnimator = GetComponent<UIWindowAnimator>();
        if (_windowAnimator == null)
        {
            _windowAnimator = gameObject.AddComponent<UIWindowAnimator>();
        }

        _windowAnimator.ApplyPreset(UIWindowAnimator.AnimationPreset.SlideFromRight);
    }

    private void OnEnable()
    {
        _global = this;
        ConstructionManager.OnConstructionListChanged += RefreshList;
        if (closeButton != null) closeButton.onClick.AddListener(HandleCloseClicked);
    }

    private void OnDisable()
    {
        if (_global == this)
        {
            _global = null;
        }

        ConstructionManager.OnConstructionListChanged -= RefreshList;
        if (closeButton != null) closeButton.onClick.RemoveListener(HandleCloseClicked);
    }

    private void Start()
    {
        if (_windowAnimator != null)
        {
            _windowAnimator.Hide(true);
            return;
        }

        SetVisible(false);
    }

    public static bool TryGetGlobal(out ProjectListPanel panel)
    {
        if (_global == null)
        {
            _global = FindAnyObjectByType<ProjectListPanel>(FindObjectsInactive.Include);
        }

        panel = _global;
        return panel != null;
    }

    public void ShowGlobal()
    {
        _mode = PanelMode.GlobalBrowser;
        UpdateTitle();
        SetVisible(true);
        RefreshList();
    }

    public void ToggleGlobal()
    {
        if (_mode == PanelMode.GlobalBrowser)
        {
            Hide();
            return;
        }

        ShowGlobal();
    }

    public void Hide()
    {
        _mode = PanelMode.Hidden;
        if (_windowAnimator != null)
        {
            _windowAnimator.Hide(onComplete: ClearItems);
            return;
        }

        SetVisible(false);
        ClearItems();
    }

    private void RefreshList()
    {
        ClearItems();

        if (_mode == PanelMode.Hidden || contentContainer == null || projectItemPrefab == null)
        {
            return;
        }

        ConstructionManager.GetAvailableProjects(_availableBuffer);

        for (int i = 0; i < _availableBuffer.Count; i++)
        {
            ProjectData data = _availableBuffer[i];
            ProjectItemView item = Instantiate(projectItemPrefab, contentContainer);
            bool canNavigate = TryFindZoneForProject(data, out GameZone zone);
            item.Setup(data, canNavigate, () =>
            {
                if (zone != null)
                {
                    NavigateToZone(zone);
                }

                Hide();
            }, "Перейти");
            _spawnedItems.Add(item.gameObject);
        }
    }

    private void ClearItems()
    {
        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            if (_spawnedItems[i] != null)
            {
                Destroy(_spawnedItems[i]);
            }
        }

        _spawnedItems.Clear();
    }

    private void SetVisible(bool isVisible)
    {
        if (_windowAnimator != null)
        {
            if (isVisible)
            {
                _windowAnimator.Show();
            }
            else
            {
                _windowAnimator.Hide();
            }

            return;
        }

        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;
    }

    private void HandleCloseClicked()
    {
        Hide();
    }

    private bool TryFindZoneForProject(ProjectData data, out GameZone zone)
    {
        zone = null;

        if (data == null)
        {
            return false;
        }

        GameZone[] zones = FindObjectsByType<GameZone>(FindObjectsInactive.Exclude);
        for (int i = 0; i < zones.Length; i++)
        {
            GameZone candidate = zones[i];
            if (candidate == null || candidate.State != ZoneState.Empty)
            {
                continue;
            }

            if (candidate.AssignedProject == data)
            {
                zone = candidate;
                return true;
            }
        }

        for (int i = 0; i < zones.Length; i++)
        {
            GameZone candidate = zones[i];
            if (candidate == null || candidate.State != ZoneState.Empty)
            {
                continue;
            }

            if (candidate.CanAccept(data))
            {
                zone = candidate;
                return true;
            }
        }

        return false;
    }

    private static void NavigateToZone(GameZone zone)
    {
        if (zone == null)
        {
            return;
        }

        if (CameraController.TryGetGlobal(out CameraController cameraController))
        {
            cameraController.FocusOnWorldPosition(zone.transform.position);
        }
    }

    private void UpdateTitle()
    {
        if (zoneTitleText == null)
        {
            return;
        }

        zoneTitleText.text = _mode switch
        {
            PanelMode.GlobalBrowser => "Все проекты",
            _ => "Проекты"
        };
    }
}
