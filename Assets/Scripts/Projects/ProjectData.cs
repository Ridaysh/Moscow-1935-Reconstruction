using UnityEngine;

[CreateAssetMenu(fileName = "NewProject", menuName = "Game/Project Data")]
public class ProjectData : ScriptableObject
{
    [Header("Основное")]
    [SerializeField] private string projectId = "project";
    [SerializeField] private string projectName = "Новый проект";
    [SerializeField, TextArea(2, 4)] private string description;

    [Header("Стоимость и время")]
    [SerializeField, Min(0)] private int cost = 10000;
    [SerializeField, Min(1)] private int durationMonths = 6;

    [Header("Визуал")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject builtPrefab;

    [Header("Историческая справка")]
    [SerializeField, TextArea(3, 8)] private string historicalInfo;
    [SerializeField] private Sprite historicalImage;

    [Header("Условия доступности")]
    [SerializeField] private ZoneType requiredZoneType = ZoneType.Any;
    [SerializeField] private bool isRepeatable;
    [SerializeField, Min(0)] private int availableFromYear = 1935;
    [SerializeField, Range(1, 12)] private int availableFromMonth = 1;
    [SerializeField] private ProjectData[] requiredProjects;

    public string ProjectId => string.IsNullOrWhiteSpace(projectId) ? name : projectId;
    public string ProjectName => projectName;
    public string Description => description;
    public int Cost => cost;
    public int DurationMonths => durationMonths;
    public Sprite Icon => icon;
    public GameObject BuiltPrefab => builtPrefab;
    public string HistoricalInfo => historicalInfo;
    public Sprite HistoricalImage => historicalImage;
    public ZoneType RequiredZoneType => requiredZoneType;
    public bool IsRepeatable => isRepeatable;
    public int AvailableFromYear => availableFromYear;
    public int AvailableFromMonth => availableFromMonth;
    public ProjectData[] RequiredProjects => requiredProjects;
}
