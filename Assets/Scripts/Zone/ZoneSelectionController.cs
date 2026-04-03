using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ZoneSelectionController : MonoBehaviour
{
    private static ZoneSelectionController _instance;

    private GameZone _currentZone;
    private ProjectData _pendingProject;

    public static event Action<GameZone> OnZoneSelected;
    public static event Action OnSelectionCleared;
    public static event Action<ProjectData> OnPendingProjectChanged;

    public static GameZone CurrentZone => EnsureInstance()._currentZone;
    public static ProjectData PendingProject => EnsureInstance()._pendingProject;

    public static void SelectZone(GameZone zone)
    {
        ZoneSelectionController controller = EnsureInstance();
        controller._currentZone = zone;
        OnZoneSelected?.Invoke(zone);

        if (zone == null || controller._pendingProject == null)
        {
            return;
        }

        if (!ConstructionManager.TryStartProject(controller._pendingProject, zone))
        {
            return;
        }

        controller._pendingProject = null;
        OnPendingProjectChanged?.Invoke(null);
    }

    public static void BeginProjectPlacement(ProjectData project)
    {
        ZoneSelectionController controller = EnsureInstance();
        controller._pendingProject = project;
        OnPendingProjectChanged?.Invoke(project);
    }

    public static void ClearPendingProject()
    {
        ZoneSelectionController controller = EnsureInstance();
        if (controller._pendingProject == null)
        {
            return;
        }

        controller._pendingProject = null;
        OnPendingProjectChanged?.Invoke(null);
    }

    public static void ClearSelection()
    {
        ZoneSelectionController controller = EnsureInstance();
        controller._currentZone = null;
        OnSelectionCleared?.Invoke();
    }

    private static ZoneSelectionController EnsureInstance()
    {
        if (_instance != null)
        {
            return _instance;
        }

        _instance = FindAnyObjectByType<ZoneSelectionController>(FindObjectsInactive.Include);
        if (_instance != null)
        {
            return _instance;
        }

        GameObject bootstrap = new(nameof(ZoneSelectionController));
        _instance = bootstrap.AddComponent<ZoneSelectionController>();
        return _instance;
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
