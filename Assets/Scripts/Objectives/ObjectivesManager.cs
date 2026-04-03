using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObjectivesManager : MonoBehaviour
{
    private static readonly IReadOnlyList<ObjectiveProgress> EmptyObjectives = Array.Empty<ObjectiveProgress>();

    private static ObjectivesManager _instance;

    [SerializeField] private List<ObjectiveData> objectives = new();

    private readonly List<ObjectiveProgress> _progressEntries = new();
    private bool _hasRaisedCompletedEvent;

    public static event Action OnObjectivesChanged;
    public static event Action OnAllObjectivesCompleted;

    public static IReadOnlyList<ObjectiveProgress> Objectives =>
        _instance != null ? _instance._progressEntries : EmptyObjectives;

    public static bool AreAllCompleted
    {
        get
        {
            if (_instance == null || _instance._progressEntries.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < _instance._progressEntries.Count; i++)
            {
                if (!_instance._progressEntries[i].IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private void Awake()
    {
        RebuildProgressEntries();
    }

    private void OnEnable()
    {
        _instance = this;
        ConstructionManager.OnConstructionCompleted += HandleConstructionCompleted;
    }

    private void OnDisable()
    {
        ConstructionManager.OnConstructionCompleted -= HandleConstructionCompleted;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static bool TryGetGlobal(out ObjectivesManager manager)
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<ObjectivesManager>(FindObjectsInactive.Include);
        }

        manager = _instance;
        return manager != null;
    }

    public static void GetObjectives(List<ObjectiveProgress> buffer)
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

        buffer.AddRange(_instance._progressEntries);
    }

    [ContextMenu("Reset Objectives")]
    public void ResetObjectives()
    {
        RebuildProgressEntries();
        OnObjectivesChanged?.Invoke();
    }

    private void RebuildProgressEntries()
    {
        _progressEntries.Clear();
        _hasRaisedCompletedEvent = false;

        for (int i = 0; i < objectives.Count; i++)
        {
            ObjectiveData data = objectives[i];
            if (data == null)
            {
                continue;
            }

            _progressEntries.Add(new ObjectiveProgress(data));
        }
    }

    private void HandleConstructionCompleted(ConstructionJob job)
    {
        if (job == null || job.Project == null)
        {
            return;
        }

        bool hasChanges = false;
        for (int i = 0; i < _progressEntries.Count; i++)
        {
            if (_progressEntries[i].RegisterCompletedConstruction(job.Project))
            {
                hasChanges = true;
            }
        }

        if (!hasChanges)
        {
            return;
        }

        OnObjectivesChanged?.Invoke();

        if (!_hasRaisedCompletedEvent && AreAllCompleted)
        {
            _hasRaisedCompletedEvent = true;
            OnAllObjectivesCompleted?.Invoke();
        }
    }
}
