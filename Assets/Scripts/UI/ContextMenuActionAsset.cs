using UnityEngine;

public abstract class ContextMenuActionAsset : ScriptableObject
{
    [SerializeField] private string label = "Action";
    [SerializeField] private bool hideMenuAfterInvoke = true;

    public virtual bool IsAvailable(GameObject target)
    {
        return target != null;
    }

    public ContextMenuAction CreateAction(GameObject target)
    {
        string resolvedLabel = string.IsNullOrWhiteSpace(label) ? name : label;
        return new ContextMenuAction(resolvedLabel, () => Execute(target), hideMenuAfterInvoke);
    }

    protected abstract void Execute(GameObject target);
}
