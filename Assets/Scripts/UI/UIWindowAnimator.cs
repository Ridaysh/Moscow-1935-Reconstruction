using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class UIWindowAnimator : MonoBehaviour
{
    public enum AnimationPreset
    {
        ScalePopup,
        SlideFromLeft,
        SlideFromRight,
        ContextMenu
    }

    [SerializeField] private RectTransform animatedTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AnimationPreset preset = AnimationPreset.ScalePopup;
    [SerializeField] private bool deactivateGameObjectOnHide;
    [SerializeField] private float showDuration = 0.28f;
    [SerializeField] private float hideDuration = 0.2f;
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    private Sequence _transition;
    private Vector2 _shownAnchoredPosition;
    private Vector3 _shownScale;
    private bool _hasShownState;
    private bool _isVisible;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        ResolveReferences();
        CacheShownState();
    }

    private void OnDisable()
    {
        KillTransition();
    }

    public void ApplyPreset(AnimationPreset newPreset, bool deactivateOnHide = false)
    {
        preset = newPreset;
        deactivateGameObjectOnHide = deactivateOnHide;

        switch (preset)
        {
            case AnimationPreset.ScalePopup:
                showDuration = 0.28f;
                hideDuration = 0.2f;
                showEase = Ease.OutBack;
                hideEase = Ease.InCubic;
                break;
            case AnimationPreset.SlideFromLeft:
            case AnimationPreset.SlideFromRight:
                showDuration = 0.24f;
                hideDuration = 0.18f;
                showEase = Ease.OutCubic;
                hideEase = Ease.InCubic;
                break;
            case AnimationPreset.ContextMenu:
                showDuration = 0.14f;
                hideDuration = 0.1f;
                showEase = Ease.OutCubic;
                hideEase = Ease.InQuad;
                break;
        }

        ResolveReferences();
        CacheShownState();
    }

    public void UpdateShownStateFromCurrent()
    {
        ResolveReferences();
        CacheShownState();
    }

    public void Show(bool instant = false, TweenCallback onComplete = null)
    {
        ResolveReferences();
        EnsureShownState();

        if (_isVisible && !IsTransitionActive() && IsInVisibleState())
        {
            onComplete?.Invoke();
            return;
        }

        if (deactivateGameObjectOnHide && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        KillTransition();
        _isVisible = true;

        PrepareForShow();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (instant)
        {
            ApplyVisibleState();
            onComplete?.Invoke();
            return;
        }

        _transition = DOTween.Sequence().SetUpdate(true);
        _transition.Join(canvasGroup.DOFade(1f, showDuration).SetEase(showEase));
        _transition.Join(animatedTransform.DOAnchorPos(_shownAnchoredPosition, showDuration).SetEase(showEase));
        _transition.Join(animatedTransform.DOScale(_shownScale, showDuration).SetEase(showEase));
        _transition.OnComplete(() =>
        {
            _transition = null;
            ApplyVisibleState();
            onComplete?.Invoke();
        });
    }

    public void Hide(bool instant = false, TweenCallback onComplete = null)
    {
        ResolveReferences();
        EnsureShownState();

        Vector2 hiddenPosition = GetHiddenAnchoredPosition();
        Vector3 hiddenScale = GetHiddenScale();

        if (!_isVisible && !IsTransitionActive() && IsInHiddenState(hiddenPosition, hiddenScale))
        {
            if (deactivateGameObjectOnHide && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            onComplete?.Invoke();
            return;
        }

        KillTransition();
        _isVisible = false;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (instant)
        {
            ApplyHiddenState(hiddenPosition, hiddenScale);
            if (deactivateGameObjectOnHide)
            {
                gameObject.SetActive(false);
            }

            onComplete?.Invoke();
            return;
        }

        _transition = DOTween.Sequence().SetUpdate(true);
        _transition.Join(canvasGroup.DOFade(0f, hideDuration).SetEase(hideEase));
        _transition.Join(animatedTransform.DOAnchorPos(hiddenPosition, hideDuration).SetEase(hideEase));
        _transition.Join(animatedTransform.DOScale(hiddenScale, hideDuration).SetEase(hideEase));
        _transition.OnComplete(() =>
        {
            _transition = null;
            ApplyHiddenState(hiddenPosition, hiddenScale);
            if (deactivateGameObjectOnHide)
            {
                gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        });
    }

    private void ResolveReferences()
    {
        if (animatedTransform == null)
        {
            animatedTransform = transform as RectTransform;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void CacheShownState()
    {
        if (animatedTransform == null)
        {
            return;
        }

        _shownAnchoredPosition = animatedTransform.anchoredPosition;
        _shownScale = animatedTransform.localScale;
        _hasShownState = true;
    }

    private void EnsureShownState()
    {
        if (_hasShownState)
        {
            return;
        }

        CacheShownState();
    }

    private void PrepareForShow()
    {
        if (canvasGroup.alpha > 0f)
        {
            return;
        }

        animatedTransform.anchoredPosition = GetHiddenAnchoredPosition();
        animatedTransform.localScale = GetHiddenScale();
    }

    private Vector2 GetHiddenAnchoredPosition()
    {
        return preset switch
        {
            AnimationPreset.SlideFromLeft => _shownAnchoredPosition + new Vector2(-Mathf.Max(animatedTransform.rect.width * 0.18f, 72f), 0f),
            AnimationPreset.SlideFromRight => _shownAnchoredPosition + new Vector2(Mathf.Max(animatedTransform.rect.width * 0.18f, 72f), 0f),
            AnimationPreset.ContextMenu => _shownAnchoredPosition + new Vector2(-10f, 10f),
            _ => _shownAnchoredPosition + new Vector2(0f, -24f)
        };
    }

    private Vector3 GetHiddenScale()
    {
        float scaleFactor = preset switch
        {
            AnimationPreset.ContextMenu => 0.96f,
            AnimationPreset.ScalePopup => 0.9f,
            _ => 0.985f
        };

        return _shownScale * scaleFactor;
    }

    private void ApplyVisibleState()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        animatedTransform.anchoredPosition = _shownAnchoredPosition;
        animatedTransform.localScale = _shownScale;
    }

    private void ApplyHiddenState(Vector2 hiddenPosition, Vector3 hiddenScale)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        animatedTransform.anchoredPosition = hiddenPosition;
        animatedTransform.localScale = hiddenScale;
    }

    private bool IsInVisibleState()
    {
        return canvasGroup.alpha >= 0.999f
            && canvasGroup.interactable
            && canvasGroup.blocksRaycasts;
    }

    private bool IsInHiddenState(Vector2 hiddenPosition, Vector3 hiddenScale)
    {
        return canvasGroup.alpha <= 0.001f
            && !canvasGroup.interactable
            && !canvasGroup.blocksRaycasts
            && animatedTransform.anchoredPosition == hiddenPosition
            && animatedTransform.localScale == hiddenScale;
    }

    private bool IsTransitionActive()
    {
        return _transition != null && _transition.IsActive();
    }

    private void KillTransition()
    {
        if (_transition == null)
        {
            return;
        }

        _transition.Kill();
        _transition = null;
    }
}
