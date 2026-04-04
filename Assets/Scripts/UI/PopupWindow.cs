using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class PopupWindow : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private UIWindowAnimator _windowAnimator;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _windowAnimator = GetComponent<UIWindowAnimator>();
        if (_windowAnimator == null)
        {
            _windowAnimator = gameObject.AddComponent<UIWindowAnimator>();
        }

        _windowAnimator.ApplyPreset(UIWindowAnimator.AnimationPreset.ScalePopup);
    }

    protected virtual void Start()
    {
        if (_windowAnimator != null)
        {
            _windowAnimator.Hide(true);
            return;
        }

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

        if (_windowAnimator != null)
        {
            if (isOpen)
            {
                _windowAnimator.Show();
            }
            else
            {
                _windowAnimator.Hide();
            }
        }
        else
        {
            _canvasGroup.alpha = isOpen ? 1f : 0f;
            _canvasGroup.interactable = isOpen;
            _canvasGroup.blocksRaycasts = isOpen;
        }

        OnPopupVisibilityChanged(isOpen);
    }

    protected virtual void OnPopupVisibilityChanged(bool isOpen)
    {
    }
}
