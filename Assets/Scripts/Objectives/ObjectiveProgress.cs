using UnityEngine;

public sealed class ObjectiveProgress
{
    public ObjectiveProgress(ObjectiveData data)
    {
        Data = data;
    }

    public ObjectiveData Data { get; }
    public int CurrentValue { get; private set; }
    public int TargetValue => Data != null ? Data.TargetCount : 1;
    public bool IsCompleted => CurrentValue >= TargetValue;

    public bool RegisterCompletedConstruction(ProjectData project)
    {
        if (Data == null || IsCompleted)
        {
            return false;
        }

        bool shouldIncrement = Data.Type switch
        {
            ObjectiveType.BuildCount => project != null,
            ObjectiveType.BuildSpecificProject => project == Data.TargetProject,
            _ => false
        };

        if (!shouldIncrement)
        {
            return false;
        }

        CurrentValue = Mathf.Min(TargetValue, CurrentValue + 1);
        return true;
    }

    public string GetProgressText()
    {
        return $"{CurrentValue}/{TargetValue}";
    }

    public string GetStatusText()
    {
        return IsCompleted ? "<color=green>Выполнено</color>" : "В процессе";
    }
}
