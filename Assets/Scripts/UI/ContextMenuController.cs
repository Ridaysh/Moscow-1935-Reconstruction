using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ContextMenuController : MonoBehaviour
{
    private static ContextMenuController _global;

    [SerializeField] private RectTransform rootCanvasRect;
    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private Button contextButtonPrefab;
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool hideOnButtonClicked = true;

    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private readonly List<Button> _spawnedButtons = new();
    private Button _embeddedTemplateButton;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ResolveCanvasReferences();
        ResolveButtonReferences();

        if (hideOnStart)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        _global = this;
    }

    private void OnDestroy()
    {
        if (_global == this)
        {
            _global = null;
        }
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (!TryGetPointerDownScreenPosition(out Vector2 pointerScreenPosition))
        {
            return;
        }

        if (!IsScreenPointInsideMenu(pointerScreenPosition))
        {
            Hide();
        }
    }

    public static bool TryGetGlobal(out ContextMenuController controller)
    {
        if (_global == null)
        {
            _global = FindFirstObjectByType<ContextMenuController>(FindObjectsInactive.Include);
        }

        controller = _global;
        return controller != null;
    }

    public void ShowAtScreenPosition(Vector2 screenPosition)
    {
        if (!PrepareForShow(screenPosition, out Vector2 localPoint))
        {
            return;
        }

        ClearSpawnedButtons();
        SetTemplateButtonVisible(_embeddedTemplateButton != null);
        PositionMenu(localPoint);
    }

    public void ShowAtScreenPosition(Vector2 screenPosition, IReadOnlyList<ContextMenuAction> actions)
    {
        if (!PrepareForShow(screenPosition, out Vector2 localPoint))
        {
            return;
        }

        BuildDynamicButtons(actions);
        if (_spawnedButtons.Count == 0 && _embeddedTemplateButton == null)
        {
            Hide();
            return;
        }

        if (_spawnedButtons.Count > 0)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        PositionMenu(localPoint);
    }

    public void Hide()
    {
        ClearSpawnedButtons();
        SetTemplateButtonVisible(false);
        gameObject.SetActive(false);
    }

    private bool PrepareForShow(Vector2 screenPosition, out Vector2 localPoint)
    {
        localPoint = default;

        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        if (rootCanvasRect == null)
        {
            ResolveCanvasReferences();
            if (rootCanvasRect == null)
            {
                return false;
            }
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Camera uiCamera = null;
        if (_rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = _rootCanvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, screenPosition, uiCamera, out localPoint))
        {
            return false;
        }

        return true;
    }

    private bool TryGetPointerDownScreenPosition(out Vector2 pointerScreenPosition)
    {
        pointerScreenPosition = default;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            bool isAnyMouseButtonPressed = mouse.leftButton.wasPressedThisFrame
                || mouse.rightButton.wasPressedThisFrame
                || mouse.middleButton.wasPressedThisFrame;

            if (isAnyMouseButtonPressed)
            {
                pointerScreenPosition = mouse.position.ReadValue();
                return true;
            }
        }

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
        {
            return false;
        }

        for (int i = 0; i < touchscreen.touches.Count; i++)
        {
            TouchControl touch = touchscreen.touches[i];
            if (!touch.press.wasPressedThisFrame)
            {
                continue;
            }

            pointerScreenPosition = touch.position.ReadValue();
            return true;
        }

        return false;
    }

    private bool IsScreenPointInsideMenu(Vector2 screenPoint)
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        Camera uiCamera = null;
        if (_rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = _rootCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPoint, uiCamera);
    }

    private void ResolveCanvasReferences()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            return;
        }

        _rootCanvas = parentCanvas.rootCanvas;
        rootCanvasRect = _rootCanvas.transform as RectTransform;
    }

    private void ResolveButtonReferences()
    {
        if (buttonContainer == null)
        {
            buttonContainer = _rectTransform;
        }

        if (contextButtonPrefab == null && buttonContainer != null)
        {
            contextButtonPrefab = buttonContainer.GetComponentInChildren<Button>(true);
        }

        if (contextButtonPrefab != null && buttonContainer != null && contextButtonPrefab.transform.parent == buttonContainer.transform)
        {
            _embeddedTemplateButton = contextButtonPrefab;
        }

        SetTemplateButtonVisible(false);
    }

    private void BuildDynamicButtons(IReadOnlyList<ContextMenuAction> actions)
    {
        ClearSpawnedButtons();
        SetTemplateButtonVisible(false);

        if (actions == null || actions.Count == 0 || buttonContainer == null || contextButtonPrefab == null)
        {
            return;
        }

        for (int i = 0; i < actions.Count; i++)
        {
            ContextMenuAction action = actions[i];
            Button button = Instantiate(contextButtonPrefab, buttonContainer);
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            SetButtonLabel(button, action.Label);

            button.onClick.AddListener(() =>
            {
                action.Invoke();
                if (hideOnButtonClicked && action.HideMenuAfterInvoke)
                {
                    Hide();
                }
            });

            _spawnedButtons.Add(button);
        }
    }

    private void ClearSpawnedButtons()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            Button button = _spawnedButtons[i];
            if (button == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(button.gameObject);
            }
            else
            {
                DestroyImmediate(button.gameObject);
            }
        }

        _spawnedButtons.Clear();
    }

    private void SetTemplateButtonVisible(bool isVisible)
    {
        if (_embeddedTemplateButton != null)
        {
            _embeddedTemplateButton.gameObject.SetActive(isVisible);
        }
    }

    private void PositionMenu(Vector2 localPoint)
    {
        if (_rectTransform == null || rootCanvasRect == null)
        {
            return;
        }

        Rect canvasRect = rootCanvasRect.rect;
        Rect menuRect = _rectTransform.rect;
        Vector2 pivot = _rectTransform.pivot;

        float minX = canvasRect.xMin + menuRect.width * pivot.x;
        float maxX = canvasRect.xMax - menuRect.width * (1f - pivot.x);
        float minY = canvasRect.yMin + menuRect.height * pivot.y;
        float maxY = canvasRect.yMax - menuRect.height * (1f - pivot.y);

        Vector2 clampedPosition = localPoint;
        clampedPosition.x = Mathf.Clamp(localPoint.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(localPoint.y, minY, maxY);

        _rectTransform.anchoredPosition = clampedPosition;
    }

    private static void SetButtonLabel(Button button, string text)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        Text textComponent = button.GetComponentInChildren<Text>(true);
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
}
