using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectManager : ConstructionManager
{
    public static event Action<ConstructionJob> OnProjectStarted
    {
        add => ConstructionManager.OnConstructionStarted += value;
        remove => ConstructionManager.OnConstructionStarted -= value;
    }

    public static event Action<ConstructionJob> OnProjectCompleted
    {
        add => ConstructionManager.OnConstructionCompleted += value;
        remove => ConstructionManager.OnConstructionCompleted -= value;
    }

    public static event Action<ConstructionJob> OnProjectProgress
    {
        add => ConstructionManager.OnConstructionProgress += value;
        remove => ConstructionManager.OnConstructionProgress -= value;
    }

    public static event Action OnProjectListChanged
    {
        add => ConstructionManager.OnConstructionListChanged += value;
        remove => ConstructionManager.OnConstructionListChanged -= value;
    }

    public new static bool CanStartProject(ProjectData data) => ConstructionManager.CanStartProject(data);
    public new static bool CanStartProject(ProjectData data, GameZone zone, out string reason) =>
        ConstructionManager.CanStartProject(data, zone, out reason);
    public new static bool IsProjectAvailable(ProjectData data) => ConstructionManager.IsProjectAvailable(data);
    public new static bool TryStartProject(ProjectData data, GameZone zone) =>
        ConstructionManager.TryStartProject(data, zone);
    public new static void GetAvailableProjects(List<ProjectData> buffer) =>
        ConstructionManager.GetAvailableProjects(buffer);
    public new static void GetAvailableProjectsForZone(GameZone zone, List<ProjectData> buffer) =>
        ConstructionManager.GetAvailableProjectsForZone(zone, buffer);
    public new static bool IsCompleted(ProjectData data) => ConstructionManager.IsCompleted(data);
}
