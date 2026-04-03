using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingInfo : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ProjectData projectData;

    public ProjectData ProjectData => projectData;

    public void Init(ProjectData data)
    {
        projectData = data;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (projectData == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(projectData.HistoricalInfo))
        {
            return;
        }

        InfoPopup.Show(projectData);
    }
}
