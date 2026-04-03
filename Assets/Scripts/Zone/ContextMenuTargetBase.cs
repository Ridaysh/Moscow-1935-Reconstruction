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
        if (!IsValidClick(eventData))
        {
            return;
        }

        if (ShouldOpenContextMenu(eventData))
        {
            ShowContextMenu(eventData.position);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnPrimaryClicked(eventData);
        }
    }

    protected virtual void OnPrimaryClicked(PointerEventData eventData)
    {
    }

    protected virtual bool ShouldOpenContextMenuOnLeftClick()
    {
        return false;
    }

    protected virtual GameObject GetContextActionTarget()
    {
        return gameObject;
    }

    protected virtual ContextMenuDefinition GetContextMenuDefinition()
    {
        return contextMenuDefinition;
    }

    protected void ShowContextMenu(Vector2 screenPosition)
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

    private bool ShouldOpenContextMenu(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            return true;
        }

        return eventData.button == PointerEventData.InputButton.Left && ShouldOpenContextMenuOnLeftClick();
    }

    private bool IsValidClick(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return false;
        }

        if (eventData.button != PointerEventData.InputButton.Left &&
            eventData.button != PointerEventData.InputButton.Right)
        {
            return false;
        }

        Vector2 pointerMoveDelta = eventData.position - eventData.pressPosition;
        return pointerMoveDelta.sqrMagnitude <= clickMoveThresholdPixels * clickMoveThresholdPixels;
    }
}
