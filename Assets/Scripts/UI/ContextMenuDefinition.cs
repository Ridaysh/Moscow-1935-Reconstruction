using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContextMenuDefinition", menuName = "UI/Context Menu/Definition")]
public sealed class ContextMenuDefinition : ScriptableObject
{
    [SerializeField] private List<ContextMenuActionAsset> actions = new();

    public void BuildActions(GameObject target, List<ContextMenuAction> buffer)
    {
        if (buffer == null)
        {
            return;
        }

        buffer.Clear();
        if (actions == null || actions.Count == 0)
        {
            return;
        }

        for (int i = 0; i < actions.Count; i++)
        {
            ContextMenuActionAsset actionAsset = actions[i];
            if (actionAsset == null || !actionAsset.IsAvailable(target))
            {
                continue;
            }

            buffer.Add(actionAsset.CreateAction(target));
        }
    }
}
