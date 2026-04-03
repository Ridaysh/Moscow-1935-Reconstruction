using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ConstructionManager : MonoBehaviour
{
    private static ConstructionManager _instance;

    [SerializeField] private List<ProjectData> allProjects = new();

    private readonly List<ConstructionJob> _activeJobs = new();
    private readonly List<ProjectData> _completedProjects = new();

    public static event Action<ConstructionJob> OnConstructionStarted;
    public static event Action<ConstructionJob> OnConstructionProgress;
    public static event Action<ConstructionJob> OnConstructionCompleted;
    public static event Action OnConstructionListChanged;

    public static IReadOnlyList<ProjectData> AllProjects =>
        _instance != null ? _instance.allProjects : Array.Empty<ProjectData>();

    public static IReadOnlyList<ConstructionJob> ActiveJobs =>
        _instance != null ? _instance._activeJobs : Array.Empty<ConstructionJob>();

    public static IReadOnlyList<ProjectData> CompletedProjects =>
        _instance != null ? _instance._completedProjects : Array.Empty<ProjectData>();

    protected virtual void OnEnable()
    {
        _instance = this;
        TimeSystem.OnMonthTick += HandleMonthTick;
    }

    protected virtual void OnDestroy()
    {
        TimeSystem.OnMonthTick -= HandleMonthTick;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static bool TryGetInstance(out ConstructionManager manager)
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<ConstructionManager>(FindObjectsInactive.Include);
        }

        manager = _instance;
        return manager != null;
    }

    public static bool IsProjectAvailable(ProjectData data)
    {
        return TryGetProjectAvailability(data, out _);
    }

    public static bool TryGetProjectAvailability(ProjectData data, out string reason)
    {
        reason = string.Empty;

        if (_instance == null)
        {
            reason = "Менеджер строительства не найден.";
            return false;
        }

        if (data == null)
        {
            reason = "Проект не выбран.";
            return false;
        }

        if (!data.IsRepeatable && _instance._completedProjects.Contains(data))
        {
            reason = "Проект уже завершен.";
            return false;
        }

        for (int i = 0; i < _instance._activeJobs.Count; i++)
        {
            if (_instance._activeJobs[i].Project == data && !data.IsRepeatable)
            {
                reason = "Проект уже строится.";
                return false;
            }
        }

        int year = TimeSystem.CurrentYear;
        int month = TimeSystem.CurrentMonth;
        if (year < data.AvailableFromYear ||
            (year == data.AvailableFromYear && month < data.AvailableFromMonth))
        {
            reason = "Проект еще не доступен по дате.";
            return false;
        }

        ProjectData[] required = data.RequiredProjects;
        if (required != null)
        {
            for (int i = 0; i < required.Length; i++)
            {
                ProjectData dependency = required[i];
                if (dependency != null && !_instance._completedProjects.Contains(dependency))
                {
                    reason = $"Требуется проект: {dependency.ProjectName}.";
                    return false;
                }
            }
        }

        return true;
    }

    public static bool CanStartProject(ProjectData data)
    {
        return TryGetProjectAvailability(data, out _) && BudgetSystem.CanAfford(data.Cost);
    }

    public static bool CanStartProject(ProjectData data, GameZone zone, out string reason)
    {
        if (!TryGetProjectAvailability(data, out reason))
        {
            return false;
        }

        if (zone == null)
        {
            reason = "Зона не выбрана.";
            return false;
        }

        if (zone.State != ZoneState.Empty)
        {
            reason = "Зона уже занята.";
            return false;
        }

        if (!zone.CanAccept(data))
        {
            reason = "Проект нельзя строить в этой зоне.";
            return false;
        }

        if (!BudgetSystem.CanAfford(data.Cost))
        {
            reason = "Недостаточно средств.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public static bool TryStartProject(ProjectData data, GameZone zone)
    {
        if (_instance == null)
        {
            return false;
        }

        if (!CanStartProject(data, zone, out _))
        {
            return false;
        }

        if (!BudgetSystem.TrySpend(data.Cost, data.ProjectName))
        {
            return false;
        }

        ConstructionJob job = new(data, zone, TimeSystem.CurrentYear, TimeSystem.CurrentMonth);
        _instance._activeJobs.Add(job);
        zone.SetProjectInProgress(job);

        OnConstructionStarted?.Invoke(job);
        OnConstructionListChanged?.Invoke();
        return true;
    }

    public static bool TryGetActiveJob(GameZone zone, out ConstructionJob job)
    {
        job = null;
        if (_instance == null || zone == null)
        {
            return false;
        }

        for (int i = 0; i < _instance._activeJobs.Count; i++)
        {
            if (_instance._activeJobs[i].Zone != zone)
            {
                continue;
            }

            job = _instance._activeJobs[i];
            return true;
        }

        return false;
    }

    public static void GetAvailableProjects(List<ProjectData> buffer)
    {
        FillProjects(buffer, null);
    }

    public static void GetAvailableProjectsForZone(GameZone zone, List<ProjectData> buffer)
    {
        FillProjects(buffer, zone);
    }

    public static bool IsCompleted(ProjectData data)
    {
        return _instance != null && _instance._completedProjects.Contains(data);
    }

    private static void FillProjects(List<ProjectData> buffer, GameZone zone)
    {
        if (buffer == null)
        {
            return;
        }

        buffer.Clear();
        if (_instance == null)
        {
            return;
        }

        for (int i = 0; i < _instance.allProjects.Count; i++)
        {
            ProjectData data = _instance.allProjects[i];
            if (!IsProjectAvailable(data))
            {
                continue;
            }

            if (zone != null && !zone.CanAccept(data))
            {
                continue;
            }

            buffer.Add(data);
        }
    }

    private void HandleMonthTick(int year, int month)
    {
        for (int i = _activeJobs.Count - 1; i >= 0; i--)
        {
            ConstructionJob job = _activeJobs[i];
            job.AdvanceMonth();

            if (job.IsComplete)
            {
                CompleteJob(job, i);
            }
            else
            {
                OnConstructionProgress?.Invoke(job);
            }
        }
    }

    private void CompleteJob(ConstructionJob job, int index)
    {
        _activeJobs.RemoveAt(index);

        if (job.Project != null && !job.Project.IsRepeatable)
        {
            _completedProjects.Add(job.Project);
        }

        if (job.Zone != null)
        {
            job.Zone.MarkBuilt(job.Project);
        }

        OnConstructionCompleted?.Invoke(job);
        OnConstructionListChanged?.Invoke();
    }
}
