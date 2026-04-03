using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class ObjectivesPanel : MonoBehaviour
{
    private static ObjectivesPanel _global;

    [Header("References")]
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private ObjectiveItemView objectiveItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleText;

    private readonly List<GameObject> _spawnedItems = new();
    private readonly List<ObjectiveProgress> _buffer = new();
    private CanvasGroup _canvasGroup;
    private bool _isVisible;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _global = this;
        ObjectivesManager.OnObjectivesChanged += RefreshList;
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void OnDisable()
    {
        if (_global == this)
        {
            _global = null;
        }

        ObjectivesManager.OnObjectivesChanged -= RefreshList;
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }
    }

    private void Start()
    {
        SetVisible(false);
        UpdateTitle();
    }

    public static bool TryGetGlobal(out ObjectivesPanel panel)
    {
        if (_global == null)
        {
            _global = FindAnyObjectByType<ObjectivesPanel>(FindObjectsInactive.Include);
        }

        panel = _global;
        return panel != null;
    }

    public void Show()
    {
        _isVisible = true;
        SetVisible(true);
        RefreshList();
    }

    public void Hide()
    {
        _isVisible = false;
        SetVisible(false);
        ClearItems();
    }

    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
            return;
        }

        Show();
    }

    private void RefreshList()
    {
        if (!_isVisible)
        {
            return;
        }

        ClearItems();

        if (contentContainer == null || objectiveItemPrefab == null)
        {
            return;
        }

        ObjectivesManager.GetObjectives(_buffer);
        for (int i = 0; i < _buffer.Count; i++)
        {
            ObjectiveItemView item = Instantiate(objectiveItemPrefab, contentContainer);
            item.Setup(_buffer[i], OpenProjectsPanel);
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
        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;
    }

    private void UpdateTitle()
    {
        if (titleText != null)
        {
            titleText.text = "Цели";
        }
    }

    private void OpenProjectsPanel()
    {
        Hide();

        if (ProjectListPanel.TryGetGlobal(out ProjectListPanel projectListPanel))
        {
            projectListPanel.ShowGlobal();
        }
    }
}
