using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class GameZone : ContextMenuTargetBase, IPointerEnterHandler, IPointerExitHandler, IContextCommandReceiver
{
    private const string BuildCommandId = "build";

    [System.Serializable]
    private sealed class StringEvent : UnityEvent<string>
    {
    }

    [SerializeField, Range(0f, 1f)] private float hoverDarkenAmount = 0.15f;
    [SerializeField] private GameObject buildReplacementPrefab;
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private StringEvent onContextCommand;

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;
    private bool _isBuildQueued;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
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
    }

    public void HandleContextCommand(string commandId)
    {
        onContextCommand?.Invoke(commandId);

        if (!this)
        {
            return;
        }

        if (!string.Equals(commandId, BuildCommandId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        TryBuild();
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
    }

    private void ApplyHoverColor()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        float brightnessMultiplier = 1f - hoverDarkenAmount;
        Color hoverColor = _baseColor;
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

        _spriteRenderer.color = _baseColor;
    }

    private void TryBuild()
    {
        if (_isBuildQueued)
        {
            return;
        }

        if (buildReplacementPrefab == null)
        {
            Debug.LogWarning($"'{name}' received '{BuildCommandId}' command, but replacement prefab is not assigned.", this);
            return;
        }

        Transform currentTransform = transform;
        Transform parent = currentTransform.parent;
        int siblingIndex = currentTransform.GetSiblingIndex();

        Vector3 worldPosition = currentTransform.position;
        Quaternion worldRotation = currentTransform.rotation;
        Vector3 localPosition = currentTransform.localPosition;
        Quaternion localRotation = currentTransform.localRotation;
        Vector3 localScale = currentTransform.localScale;

        GameObject builtObject = Instantiate(buildReplacementPrefab, parent);
        Transform builtTransform = builtObject.transform;

        if (parent != null)
        {
            builtTransform.localPosition = localPosition;
            builtTransform.localRotation = localRotation;
            builtTransform.localScale = localScale;
        }
        else
        {
            builtTransform.position = worldPosition;
            builtTransform.rotation = worldRotation;
            builtTransform.localScale = localScale;
        }

        builtTransform.SetSiblingIndex(siblingIndex);

        _isBuildQueued = true;
        Destroy(gameObject);
    }
}
