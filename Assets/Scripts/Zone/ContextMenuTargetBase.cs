using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ContextMenuTargetBase : MonoBehaviour, IPointerClickHandler
{
    [SerializeField, Min(0f)] private float clickMoveThresholdPixels = 12f;
    [SerializeField] private ContextMenuDefinition contextMenuDefinition;

    private readonly List<ContextMenuAction> _cachedActions = new();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsValidPrimaryClick(eventData))
        {
            return;
        }

        ShowContextMenu(eventData.position);
        OnPrimaryClicked(eventData);
    }

    protected virtual void OnPrimaryClicked(PointerEventData eventData)
    {
    }

    protected virtual GameObject GetContextActionTarget()
    {
        return gameObject;
    }

    protected virtual ContextMenuDefinition GetContextMenuDefinition()
    {
        return contextMenuDefinition;
    }

    private void ShowContextMenu(Vector2 screenPosition)
    {
        ContextMenuDefinition menuDefinition = GetContextMenuDefinition();
        if (menuDefinition == null)
        {
            return;
        }

        if (!ContextMenuController.TryGetGlobal(out ContextMenuController contextMenu))
        {
            return;
        }

        _cachedActions.Clear();
        menuDefinition.BuildActions(GetContextActionTarget(), _cachedActions);

        if (_cachedActions.Count == 0)
        {
            contextMenu.Hide();
            return;
        }

        contextMenu.ShowAtScreenPosition(screenPosition, _cachedActions);
    }

    private bool IsValidPrimaryClick(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
        {
            return false;
        }

        Vector2 pointerMoveDelta = eventData.position - eventData.pressPosition;
        return pointerMoveDelta.sqrMagnitude <= clickMoveThresholdPixels * clickMoveThresholdPixels;
    }
}
