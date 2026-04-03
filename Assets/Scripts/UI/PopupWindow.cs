using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class PopupWindow : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    protected virtual void Start()
    {
        SetOpenFromManager(false);
    }

    public void Show()
    {
        PopupManager.Show(this);
    }

    public void Hide()
    {
        PopupManager.Hide(this);
    }

    internal void SetOpenFromManager(bool isOpen)
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.alpha = isOpen ? 1f : 0f;
        _canvasGroup.interactable = isOpen;
        _canvasGroup.blocksRaycasts = isOpen;

        OnPopupVisibilityChanged(isOpen);
    }

    protected virtual void OnPopupVisibilityChanged(bool isOpen)
    {
    }
}
