using UnityEngine;

[CreateAssetMenu(fileName = "ContextCommandAction", menuName = "UI/Context Menu/Actions/Send Command")]
public sealed class ContextCommandActionAsset : ContextMenuActionAsset
{
    [SerializeField] private string commandId = "action_id";

    public override bool IsAvailable(GameObject target)
    {
        return base.IsAvailable(target)
            && !string.IsNullOrWhiteSpace(commandId)
            && HasCommandReceiver(target);
    }

    protected override void Execute(GameObject target)
    {
        if (target == null || string.IsNullOrWhiteSpace(commandId))
        {
            return;
        }

        MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
        bool handled = false;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is not IContextCommandReceiver receiver)
            {
                continue;
            }

            receiver.HandleContextCommand(commandId);
            handled = true;
        }

        if (!handled)
        {
            Debug.LogWarning($"No {nameof(IContextCommandReceiver)} found on '{target.name}' for command '{commandId}'.", target);
        }
    }

    private static bool HasCommandReceiver(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IContextCommandReceiver)
            {
                return true;
            }
        }

        return false;
    }
}
