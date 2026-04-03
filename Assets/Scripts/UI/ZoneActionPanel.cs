using UnityEngine;

[DisallowMultipleComponent]
public class ZoneActionPanel : MonoBehaviour
{
    private static ZoneActionPanel _global;

    [SerializeField] private ProjectListPanel panel;

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

    public static bool TryGetGlobal(out ZoneActionPanel panel)
    {
        if (_global == null)
        {
            _global = FindAnyObjectByType<ZoneActionPanel>(FindObjectsInactive.Include);
        }

        panel = _global;
        return panel != null;
    }

    public void Show(GameZone zone)
    {
        // This panel is currently unused in the simplified flow.
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.Hide();
        }
    }
}
