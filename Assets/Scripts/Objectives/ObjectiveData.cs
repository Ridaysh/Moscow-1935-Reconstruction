using UnityEngine;

public enum ObjectiveType
{
    BuildCount,
    BuildSpecificProject
}

[CreateAssetMenu(fileName = "NewObjective", menuName = "Game/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Основное")]
    [SerializeField] private string objectiveId = "objective";
    [SerializeField] private string title = "Новая цель";
    [SerializeField, TextArea(2, 4)] private string description;

    [Header("Тип")]
    [SerializeField] private ObjectiveType objectiveType = ObjectiveType.BuildCount;
    [SerializeField, Min(1)] private int targetCount = 1;
    [SerializeField] private ProjectData targetProject;

    public string ObjectiveId => string.IsNullOrWhiteSpace(objectiveId) ? name : objectiveId;
    public string Title => title;
    public string Description => description;
    public ObjectiveType Type => objectiveType;
    public int TargetCount => Mathf.Max(1, targetCount);
    public ProjectData TargetProject => targetProject;
}
