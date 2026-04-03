using UnityEngine;

public enum ZoneType
{
    Any,
    Street,
    Metro,
    Park,
    Government,
    Housing,
    Infrastructure
}

[CreateAssetMenu(fileName = "ZoneDefinition", menuName = "Game/Zone Definition")]
public class ZoneDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string zoneId = "zone";
    [SerializeField] private string displayName = "Зона проекта";
    [SerializeField, TextArea(2, 4)] private string description;

    [Header("Rules")]
    [SerializeField] private ZoneType zoneType = ZoneType.Any;
    [SerializeField] private ProjectData defaultProject;
    [SerializeField] private ProjectData[] allowedProjects;

    public string ZoneId => string.IsNullOrWhiteSpace(zoneId) ? name : zoneId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public ZoneType ZoneType => zoneType;
    public ProjectData DefaultProject => defaultProject;
    public ProjectData[] AllowedProjects => allowedProjects;

    public bool Allows(ProjectData project)
    {
        if (project == null)
        {
            return false;
        }

        if (defaultProject == project)
        {
            return true;
        }

        bool hasExplicitAllowedProjects = allowedProjects != null && allowedProjects.Length > 0;
        if (hasExplicitAllowedProjects)
        {
            for (int i = 0; i < allowedProjects.Length; i++)
            {
                if (allowedProjects[i] == project)
                {
                    return true;
                }
            }

            return false;
        }

        return zoneType == ZoneType.Any
            || project.RequiredZoneType == ZoneType.Any
            || project.RequiredZoneType == zoneType;
    }
}
